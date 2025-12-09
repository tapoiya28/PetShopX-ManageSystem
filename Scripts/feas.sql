USE PetcareX;
GO

-- BÁO CÁO THỐNG KÊ
-- tính tổng sản phẩm trong 1 hoá đơn
GO
CREATE OR ALTER FUNCTION f_TongSanPhamCuaHoaDon(@MAHD INTEGER)
RETURNS INTEGER
AS
BEGIN
    DECLARE @TONG INTEGER

    SELECT @TONG = SUM(CT.SOLUONG)
    FROM HOADON HD
    JOIN CHITIETHOADON CT ON HD.MAHD = CT.MAHD
    WHERE HD.MAHD = @MAHD

    RETURN @TONG
END
GO

-- tình hình kinh doanh theo tháng/năm
GO
CREATE OR ALTER PROCEDURE sp_TinhHinhKinhDoanh 
    @Nam INT = NULL,
    @Thang INT = NULL
AS
BEGIN
    BEGIN TRY
    IF @Thang IS NOT NULL AND (@Thang < 1 OR @Thang > 12)
        RAISERROR (N'Tháng không hợp lệ', 16, 1)

    IF @Nam < 1900
        RAISERROR (N'Năm không hợp lệ', 16, 1)

    DECLARE @NGAYBD DATE
    DECLARE @NGAYKT DATE

    IF @Nam IS NULL SET @Nam = YEAR(GETDATE())

    IF @Thang IS NULL
    BEGIN
        SET @NGAYBD = DATEFROMPARTS(@Nam, 1, 1) 
        SET @NGAYKT = DATEFROMPARTS(@Nam, 12, 31) 
    END
    ELSE
    BEGIN
        SET @NGAYBD = DATEFROMPARTS(@Nam, @Thang, 1) 
        SET @NGAYKT = EOMONTH(@NGAYBD)
    END

    SELECT
        MACN,
        @Nam AS N'Năm',
        MONTH(NGAYLAP) AS N'Tháng',
        COUNT(DISTINCT MAHD) AS N'Số lượng đơn',
        SUM(TONGTIEN) AS N'Tổng Doanh thu',
        SUM((KHUYENMAI / 100) * TONGTIEN) AS N'Tổng tiền chiết khấu'
    FROM HOADON
    WHERE NGAYLAP BETWEEN @NGAYBD AND @NGAYKT
    GROUP BY MACN, YEAR(NGAYLAP), MONTH(NGAYLAP)
    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
    END CATCH
END
GO

-- thống kê lượng sản phẩm bán được trong tháng/năm theo từng loại hàng
GO
CREATE OR ALTER PROCEDURE sp_ThongKeSanPham
    @Thang INTEGER = NULL, 
    @Nam INTEGER = NULL
AS
BEGIN
    BEGIN TRY
        IF @Thang IS NOT NULL AND (@Thang < 1 OR @Thang > 12)
        RETURN 0;

        DECLARE @NGAYBD DATE
        DECLARE @NGAYKT DATE

        IF @Nam IS NULL SET @Nam = YEAR(GETDATE());

        IF @Thang IS NULL
        BEGIN
            SET @NGAYBD = DATEFROMPARTS(@Nam, 1, 1)
            SET @NGAYKT = DATEFROMPARTS(@Nam, 12, 31)
        END
        ELSE IF @Thang IS NOT NULL
        BEGIN
            SET @NGAYBD = DATEFROMPARTS(@Nam, @Thang, 1)
            SET @NGAYKT = EOMONTH(@NGAYBD)
        END

        SELECT 
            @Nam AS N'Năm',
            @Thang AS N'Tháng',
            SP.LOAI AS N'Loại',
            SUM(CT.SOLUONG) AS N'Tổng số lượng'
        FROM HOADON HD
        JOIN CHITIETHOADON CT ON HD.MAHD = CT.MAHD
        JOIN SANPHAM SP ON CT.MASP = SP.MASP
        WHERE HD.NGAYLAP BETWEEN @NGAYBD AND @NGAYKT
        GROUP BY SP.LOAI

        RETURN 1;
    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
        RETURN -1;
    END CATCH
END
GO

-- thống kê sản phẩm có số lượng tồn kho thấp
-- function trả về danh sách mã sản phẩm (sản phẩm, thuốc, vacxin) có số lượng dưới ngưỡng min
GO
CREATE OR ALTER FUNCTION f_TonKhoThap()
RETURNS TABLE
AS
    RETURN SELECT *
    FROM SANPHAM
    WHERE TONKHO < 100
GO

-- phân tích khách hàng tiềm năng (trên toàn hệ thống)
GO
CREATE OR ALTER PROCEDURE sp_PhanTichKhachHang
AS
BEGIN   
    SET NOCOUNT ON;
    BEGIN TRY
        WITH RFM_khachHang AS (
            SELECT 
                MAKH,
                MAX(NGAYLAP) AS 'NgayMuaHangGanNhat',
                COUNT(DISTINCT MAHD) AS 'TanSuat',
                SUM(TONGTIEN) AS 'TongChi'
            FROM HOADON
            GROUP BY MAKH
        ),
        RFM_Diem AS (
            SELECT 
                MAKH, 
                DATEDIFF(DAY, NgayMuaHangGanNhat, GETDATE()) AS 'SoNgay_NgayMuaHangGanNhat',
                TanSuat,
                TongChi,
                NTILE(5) OVER (ORDER BY DATEDIFF(DAY, GETDATE(), NgayMuaHangGanNhat) DESC) AS R_DIEM,
                NTILE(5) OVER (ORDER BY TanSuat ASC) AS F_Diem,
                NTILE(5) OVER (ORDER BY TongChi ASC) AS M_Diem
            FROM RFM_khachHang
        )
        SELECT 
            MAKH,
            SoNgay_NgayMuaHangGanNhat,
            TanSuat,
            TongChi,
            CASE 
                WHEN rfm.R_Diem >= 4 AND rfm.F_Diem >= 4 AND rfm.M_Diem >= 4 THEN N'Khách hàng gần đây'
                WHEN rfm.R_Diem >= 3 AND rfm.F_Diem >= 3 AND rfm.M_Diem >= 3 THEN N'Khách hàng trung thành'
                WHEN rfm.R_Diem >= 3 AND rfm.F_Diem >= 2 AND rfm.M_Diem >= 2 THEN N'Tiềm năng'
                WHEN rfm.R_Diem < 2 AND rfm.F_Diem >= 2 AND rfm.M_Diem >= 2 THEN N'Mua lâu, từng thường xuyên đến'
                WHEN rfm.R_Diem < 2 AND rfm.F_Diem < 2 AND rfm.M_Diem < 2 THEN N'Mua lâu, ít khi mua, chi ít'
            END AS N'Phân loại khách hàng'
        FROM RFM_Diem rfm
        ORDER BY MAKH
    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
    END CATCH
END
GO

-- Lượng khách hàng trong tháng (tổng, cũ, mới) (trên toàn hệ thống)
GO
CREATE OR ALTER PROCEDURE sp_ThongKeKhachHang
    @Thang INTEGER = NULL,
    @Nam INTEGER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY

        IF @Thang IS NOT NULL AND (@Thang < 1 OR @Thang > 12)
        RAISERROR (N'Tháng không hợp lệ', 16, 1)

        IF @Nam IS NOT NULL AND @Nam < 1900
        RAISERROR (N'Năm không hợp lệ', 16, 1)

        DECLARE @NGAYBD DATE
        DECLARE @NGAYKT DATE

        IF @Thang IS NOT NULL
        BEGIN
            SET @NGAYBD = DATEFROMPARTS(@Nam, @Thang, 1)
            SET @NGAYKT = EOMONTH(@NGAYBD)
        END
        ELSE IF @Thang IS NULL
        BEGIN
            SET @NGAYBD = DATEFROMPARTS(@Nam, 1, 1)
            SET @NGAYKT = DATEFROMPARTS(@Nam, 12, 31)
        END;

        WITH MuaHangTrongThang AS (
            SELECT 
                *
            FROM HOADON HD 
            WHERE HD.NGAYLAP BETWEEN @NGAYBD AND @NGAYKT
        ), KhachHangCTE AS (
            SELECT 
                MAKH, 
                CASE 
                    WHEN EXISTS (SELECT 1 
                                FROM HOADON HD
                                WHERE HD.MAKH = kh.MAKH 
                                AND HD.NGAYLAP < @NGAYBD
                                ) THEN 1
                    ELSE 0 
                END AS 'QuayLai'
            FROM MuaHangTrongThang kh
        )

        SELECT
            (SELECT COUNT(*) FROM MuaHangTrongThang) AS N'TongKhachHang',
            SUM(CASE WHEN kh.QuayLai = 1 THEN 1 ELSE 0 END) N'Số lượng khách hàng cũ quay lại',
            SUM(CASE WHEN kh.QuayLai = 0 THEN 1 ELSE 0 END) N'Số lượng khách hàng mới'
        FROM KhachHangCTE kh
    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
    END CATCH
END
GO

-- thống kê đánh giá khách hàng 
GO
CREATE OR ALTER PROCEDURE sp_ThongKeDanhGia
AS 
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        WITH Points AS (
            SELECT 1 AS DIEM UNION ALL
            SELECT 2 UNION ALL
            SELECT 3 UNION ALL
            SELECT 4 UNION ALL
            SELECT 5
        ),
        DiemDichVu AS (
            SELECT 
                HD.MACN,
                DG.DIEMDICHVU AS DIEM,
                COUNT(*) AS SOLUONG
            FROM HOADON HD
            JOIN DANHGIA DG ON HD.MAHD = DG.MAHD
            WHERE DG.DIEMDICHVU IS NOT NULL
            GROUP BY HD.MACN, DG.DIEMDICHVU
        ),
        DiemHaiLong AS (
            SELECT 
                HD.MACN,
                DG.MUCDOHAILONG AS DIEM,
                COUNT(*) AS SOLUONG
            FROM HOADON HD
            JOIN DANHGIA DG ON HD.MAHD = DG.MAHD
            WHERE DG.MUCDOHAILONG IS NOT NULL
            GROUP BY HD.MACN, DG.MUCDOHAILONG
        ),
        Branches AS (
            SELECT DISTINCT MACN FROM HOADON
        )

        SELECT 
            B.MACN AS N'Mã chi nhánh',
            P.DIEM AS N'Điểm',
            ISNULL(DV.SOLUONG, 0) AS N'Số lượng dịch vụ',
            ISNULL(HL.SOLUONG, 0) AS N'Số lượng hài lòng'
        FROM Branches B
        CROSS JOIN Points P
        LEFT JOIN DiemDichVu DV 
            ON DV.MACN = B.MACN AND DV.DIEM = P.DIEM
        LEFT JOIN DiemHaiLong HL
            ON HL.MACN = B.MACN AND HL.DIEM = P.DIEM
        ORDER BY B.MACN, P.DIEM;

    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
    END CATCH
END
GO

-- thống kê sản phẩm bán chạy
GO
CREATE OR ALTER PROCEDURE sp_SanPhamBanChay
    @Thang INTEGER = NULL,
    @Nam INTEGER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF @Thang IS NOT NULL AND (@Thang < 1 OR @Thang > 12)
        RAISERROR (N'Tháng không hợp lệ', 16, 1)

        IF @Nam IS NOT NULL AND @Nam < 1900
        RAISERROR (N'Năm không hợp lệ', 16, 1)
        IF @Nam IS NULL SET @Nam = YEAR(GETDATE());

        DECLARE @NGAYBD DATE
        DECLARE @NGAYKT DATE

        IF @Thang IS NOT NULL
        BEGIN
            SET @NGAYBD = DATEFROMPARTS(@Nam, @Thang, 1)
            SET @NGAYKT = EOMONTH(@NGAYBD)
        END
        ELSE IF @Thang IS NULL
        BEGIN
            SET @NGAYBD = DATEFROMPARTS(@Nam, 1, 1)
            SET @NGAYKT = DATEFROMPARTS(@Nam, 12, 31)
        END

        SELECT 
            SP.MASP,
            SP.TENSP,
            SP.LOAI,
            SUM(CT.SOLUONG) AS SOLUONG
        FROM CHITIETHOADON CT
        JOIN HOADON HD ON CT.MAHD = HD.MAHD
        JOIN SANPHAM SP ON CT.MASP = SP.MASP
        WHERE HD.NGAYLAP BETWEEN @NGAYBD AND @NGAYKT
        GROUP BY SP.MASP, SP.TENSP, SP.LOAI
        HAVING SUM(CT.SOLUONG) > 0
        ORDER BY SOLUONG DESC
    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
    END CATCH
END
GO

GO-- tra cứu thông tin sản phẩm
CREATE OR ALTER PROCEDURE sp_TraCuuThongTinSanPham
    @MASP INTEGER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (
            SELECT 1 
            FROM SANPHAM
            WHERE MASP = @MASP
        )
        RAISERROR (N'Mã sản phẩm không tồn tại', 16, 1)

        DECLARE @LOAISP NVARCHAR(10);
        
        SELECT 
            @LOAISP = LOAI
        FROM SANPHAM
        WHERE MASP = @MASP;
        
        IF @LOAISP = N'Sản phẩm'
        BEGIN
            SELECT 
                MASP AS N'Mã sản phẩm',
                TENSP AS N'Tên sản phẩm',
                DONGIA AS N'Đơn giá hiện tại',
                TONKHO AS N'Số lượng còn lại'
            FROM SANPHAM 
            WHERE MASP = @MASP;

            RETURN 1;
        END

        IF @LOAISP = N'Dịch vụ'
        BEGIN
            SELECT 
                SP.MASP AS N'Mã dịch vụ',
                SP.TENSP AS N'Tên dịch vụ',
                SP.DONGIA AS N'Đơn giá hiện tại',
                DV.THOIGIANTHUCHIEN AS N'Thời gian thực hiện',
                LDV.TENLOAIDV AS N'Loại dịch vụ'
            FROM SANPHAM SP
            JOIN DICHVU DV ON SP.MASP = DV.MADV
            JOIN LOAIDICHVU LDV ON DV.MALOAIDV = LDV.MALOAIDV
            WHERE MASP = @MASP;

            RETURN 1;
        END
        
        IF @LOAISP = 'Thuốc'
        BEGIN
            SELECT 
                SP.MASP AS N'Mã thuốc',
                SP.TENSP AS N'Tên thuốc',
                SP.DONGIA AS N'Đơn giá hiện tại',
                TH.DONVI AS N'Đơn vị tính',
                TH.NGAYSX AS N'Ngày sản xuất',
                TH.HSD AS N'Hạn sử dụng'
            FROM SANPHAM SP
            JOIN THUOC TH ON SP.MASP = TH.MATHUOC
            WHERE MASP = @MASP;

            RETURN 1;
        END
        IF @LOAISP = 'Vacxin'
        BEGIN
            SELECT 
                SP.MASP AS N'Mã vacxin',
                SP.TENSP AS N'Tên vacxin',
                SP.DONGIA AS N'Đơn giá hiện tại',
                VC.DOTUOIAPDUNG AS N'Độ tuổi áp dụng',
                VC.NGAYSX AS N'Ngày sản xuất',
                VC.HSD AS N'Hạn sử dụng'
            FROM SANPHAM SP
            JOIN VACXIN VC ON SP.MASP = VC.MAVACXIN
            WHERE MASP = @MASP;

            RETURN 1;
        END
        
        IF @LOAISP = 'Gói tiêm'
        BEGIN
            SELECT 
                SP.MASP AS N'Mã gói tiêm',
                SP.TENSP AS N'Tên gói tiêm',
                SP.DONGIA AS N'Đơn giá hiện tại',
                GT.THOIGIAN AS N'Thời gian',
                GT.KHUYENMAI AS N'Khuyến mãi',
                COUNT(CT.SOLUONG) AS N'Tổng mũi tiêm' 
            FROM SANPHAM SP
            JOIN GOITIEM GT ON SP.MASP = GT.MAGOITIEM
            JOIN CHITIETGOITIEM CT ON GT.MAGOITIEM = CT.MAGOITIEM
            WHERE MASP = @MASP
            GROUP BY SP.MASP, SP.TENSP, SP.DONGIA, GT.THOIGIAN, GT.KHUYENMAI;

            RETURN 1;
        END
    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
        RETURN -1;
    END CATCH
END
GO

-- QuanLyHeThong - toan
CREATE OR ALTER PROCEDURE sp_Sub_NhanVien_Xem
    @MANV INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @MANV IS NULL
    BEGIN
        SELECT NV.MANV, NV.HOTEN, NV.SDT, NV.GIOITINH, NV.LUONGCOBAN, NV.VAITRO,
               BS.BANGCAP, BS.KINHNGHIEM, QL.NGAYBONHIEM, QL.MACN AS MaChiNhanhQuanLy
        FROM NHANVIEN NV 
        LEFT JOIN BACSI BS ON NV.MANV = BS.MANV 
        LEFT JOIN QUANLY QL ON NV.MANV = QL.MANV;
        RETURN 0;
    END

    IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MANV)
        THROW 50016, N'Nhân viên không tồn tại.', 1;

    DECLARE @ROLE VARCHAR(2);
    SELECT @ROLE = VAITRO FROM NHANVIEN WHERE MANV = @MANV;

    IF @ROLE = 'BS'
    BEGIN
        SELECT NV.MANV, NV.HOTEN, NV.SDT, NV.GIOITINH, NV.LUONGCOBAN, NV.VAITRO,
               BS.BANGCAP, BS.KINHNGHIEM
        FROM NHANVIEN NV
        JOIN BACSI BS ON NV.MANV = BS.MANV
        WHERE NV.MANV = @MANV;
    END
    ELSE IF @ROLE = 'QL'
    BEGIN
        SELECT NV.MANV, NV.HOTEN, NV.SDT, NV.GIOITINH, NV.LUONGCOBAN, NV.VAITRO,
               QL.NGAYBONHIEM, QL.MACN AS MaChiNhanhQuanLy
        FROM NHANVIEN NV
        JOIN QUANLY QL ON NV.MANV = QL.MANV
        WHERE NV.MANV = @MANV;
    END
    ELSE
    BEGIN
        SELECT NV.MANV, NV.HOTEN, NV.SDT, NV.GIOITINH, NV.LUONGCOBAN, NV.VAITRO
        FROM NHANVIEN NV
        WHERE NV.MANV = @MANV;
    END
END;
GO

CREATE OR ALTER PROCEDURE sp_Sub_NhanVien_Them
    @HOTEN NVARCHAR(50), @NGAYSINH DATE, @GIOITINH NVARCHAR(5), @SDT CHAR(10), @LUONGCOBAN INT,
    @VAITRO VARCHAR(2), @BANGCAP NVARCHAR(50) = NULL, @KINHNGHIEM INT = 0, @MACN_QUANLY INT = NULL
AS
BEGIN
    IF @VAITRO = 'BS' AND @BANGCAP IS NULL THROW 50001, N'Thiếu bằng cấp bác sĩ.', 1;
    IF @VAITRO = 'QL' AND @MACN_QUANLY IS NULL THROW 50002, N'Thiếu mã chi nhánh quản lý.', 1;
    IF EXISTS (SELECT 1 FROM NHANVIEN WHERE SDT = @SDT) THROW 50003, N'SĐT đã được sử dụng.', 1;

    INSERT INTO NHANVIEN (HOTEN, NGAYSINH, GIOITINH, SDT, LUONGCOBAN, VAITRO)
    VALUES (@HOTEN, @NGAYSINH, @GIOITINH, @SDT, @LUONGCOBAN, @VAITRO);

    DECLARE @NewMANV INT = SCOPE_IDENTITY();

    IF @VAITRO = 'BS' INSERT INTO BACSI (MANV, BANGCAP, KINHNGHIEM) VALUES (@NewMANV, @BANGCAP, @KINHNGHIEM);
    ELSE IF @VAITRO = 'QL' INSERT INTO QUANLY (MANV, NGAYBONHIEM, MACN) VALUES (@NewMANV, GETDATE(), @MACN_QUANLY);
    PRINT N'Đã thêm thành công. Mã: ' + CAST(@NewMANV AS NVARCHAR(20));
END;
GO

CREATE OR ALTER PROCEDURE sp_Sub_NhanVien_Sua
    @MANV INT, @HOTEN NVARCHAR(50), @NGAYSINH DATE, @GIOITINH NVARCHAR(5), @SDT CHAR(10), @LUONGCOBAN INT,
    @BANGCAP NVARCHAR(50) = NULL, @KINHNGHIEM INT = 0, @MACN_QUANLY INT = NULL
AS
BEGIN
    IF @MANV IS NULL THROW 50005, N'Thiếu mã NV.', 1;

    UPDATE NHANVIEN 
    SET 
        HOTEN = ISNULL(@HOTEN, HOTEN),
        NGAYSINH = ISNULL(@NGAYSINH, NGAYSINH), 
        GIOITINH = ISNULL(@GIOITINH, GIOITINH), 
        SDT = ISNULL(@SDT, SDT), 
        LUONGCOBAN = ISNULL(@LUONGCOBAN, LUONGCOBAN) 
    WHERE MANV = @MANV;

    IF EXISTS (SELECT 1 FROM BACSI WHERE MANV = @MANV)
        UPDATE BACSI 
        SET 
            BANGCAP = ISNULL(@BANGCAP, BANGCAP), 
            KINHNGHIEM = ISNULL(@KINHNGHIEM, KINHNGHIEM) 
            WHERE MANV = @MANV;
    ELSE IF EXISTS (SELECT 1 FROM QUANLY WHERE MANV = @MANV) AND @MACN_QUANLY IS NOT NULL
        UPDATE QUANLY 
        SET 
            MACN = ISNULL(@MACN_QUANLY, MACN) 
        WHERE MANV = @MANV;

    PRINT N'Đã cập nhật thông tin.';
END;
GO

--- QuanLyChiNhanh
CREATE OR ALTER PROCEDURE sp_Sub_ChiNhanh_Xem AS 
BEGIN SELECT * FROM CHINHANH; END;
GO

CREATE OR ALTER PROCEDURE sp_Sub_ChiNhanh_Them
    @TENCN NVARCHAR(50), @DIACHI NVARCHAR(100), @SDT CHAR(10), 
    @GIODM TIME, @GIODONGCUA TIME
AS
BEGIN
    IF EXISTS (SELECT 1 FROM CHINHANH WHERE SDT = @SDT) 
        THROW 50006, N'SĐT chi nhánh đã tồn tại.', 1;

    INSERT INTO CHINHANH (TENCN, DIACHI, SDT, GIODM, GIODONGCUA) 
    VALUES (@TENCN, @DIACHI, @SDT, @GIODM, @GIODONGCUA);

    DECLARE @NewMACN INT = SCOPE_IDENTITY();
    PRINT N'Đã thêm CN: ' + CAST(@NewMACN AS NVARCHAR(20));
END;
GO

CREATE OR ALTER PROCEDURE sp_Sub_ChiNhanh_Sua
    @MACN INT, @TENCN NVARCHAR(50), @DIACHI NVARCHAR(100), 
    @SDT CHAR(10), @GIODM TIME, @GIODONGCUA TIME
AS
BEGIN
    IF @MACN IS NULL THROW 50008, N'Thiếu mã CN.', 1;

    IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MACN) 
    THROW 50009, N'Chi nhánh không tồn tại.', 1;

    IF EXISTS (SELECT 1 FROM CHINHANH WHERE SDT = @SDT AND MACN <> @MACN) 
    THROW 50010, N'SĐT đã được sử dụng bởi chi nhánh khác.', 1;

    UPDATE CHINHANH 
    SET 
        TENCN = ISNULL(@TENCN, TENCN), 
        DIACHI = ISNULL(@DIACHI, DIACHI), 
        SDT = ISNULL(@SDT, SDT), 
        GIODM = ISNULL(@GIODM, GIODM), 
        GIODONGCUA = ISNULL(@GIODONGCUA, GIODONGCUA) 
    WHERE MACN = @MACN;
    PRINT N'Đã cập nhật CN: ' + CAST(@MACN AS NVARCHAR(20));
END;
GO

-- QuanLyLichSuPhanCong
CREATE OR ALTER PROCEDURE sp_Sub_LichSu_PhanCong
    @MACN INT, 
    @MANV INT, 
    @NGAYBATDAU DATE, 
    @NGAYKETTHUC DATE
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MACN) THROW 50011, N'CN không tồn tại.', 1;
    IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MANV) THROW 50012, N'NV không tồn tại.', 1;

    IF @NGAYKETTHUC IS NOT NULL
    BEGIN
        DECLARE @LatestStartDate DATE;
        SELECT TOP 1 @LatestStartDate = NGAYBATDAU 
        FROM LAMVIEC 
        WHERE MACN = @MACN AND MANV = @MANV 
        ORDER BY NGAYBATDAU DESC;
        
        IF @LatestStartDate IS NULL THROW 50013, N'Không tìm thấy lịch sử.', 1;
        IF @NGAYKETTHUC < @LatestStartDate THROW 50014, N'Ngày kết thúc lỗi.', 1;
        
        UPDATE LAMVIEC SET NGAYKETTHUC = @NGAYKETTHUC 
        WHERE MACN = @MACN AND MANV = @MANV AND NGAYBATDAU = @LatestStartDate;
        PRINT N'Đã cập nhật ngày kết thúc.';
    END
    ELSE
    BEGIN
        IF @NGAYBATDAU IS NULL SET @NGAYBATDAU = GETDATE();
        IF EXISTS (SELECT 1 FROM LAMVIEC WHERE MANV = @MANV AND NGAYKETTHUC IS NULL)
        BEGIN
            UPDATE LAMVIEC 
            SET NGAYKETTHUC = @NGAYBATDAU 
            WHERE MANV = @MANV 
                AND NGAYKETTHUC IS NULL;
            PRINT N'Đã đóng công việc cũ.';
        END
        INSERT INTO LAMVIEC (MACN, MANV, NGAYBATDAU, NGAYKETTHUC) VALUES (@MACN, @MANV, @NGAYBATDAU, NULL);
        PRINT N'Đã phân công mới.';
    END
END;
GO

CREATE OR ALTER PROCEDURE sp_Sub_LichSu_XemNV @MANV INT 
AS
BEGIN
    SELECT LV.MANV, NV.HOTEN, LV.MACN, CN.TENCN, LV.NGAYBATDAU,
           CASE WHEN LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE() THEN N'Đang làm việc' 
           ELSE N'Đã nghỉ' END AS TRANGTHAI,
           LV.NGAYKETTHUC
    FROM LAMVIEC LV JOIN NHANVIEN NV ON LV.MANV = NV.MANV JOIN CHINHANH CN ON LV.MACN = CN.MACN
    WHERE LV.MANV = @MANV ORDER BY LV.NGAYBATDAU DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_Sub_LichSu_XemCN @MACN INT 
AS
BEGIN
    SELECT LV.MACN, CN.TENCN, LV.MANV, NV.HOTEN, NV.SDT, NV.LUONGCOBAN, LV.NGAYBATDAU,
           CASE WHEN LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE() 
           THEN N'Đang làm việc' ELSE N'Đã nghỉ việc' END AS TRANGTHAI
    FROM LAMVIEC LV 
    JOIN CHINHANH CN ON LV.MACN = CN.MACN 
    JOIN NHANVIEN NV ON LV.MANV = NV.MANV
    WHERE LV.MACN = @MACN AND (LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE())
    ORDER BY LV.NGAYBATDAU DESC;
END;
GO

--- QuanLyLuongChiNhanh
CREATE OR ALTER PROCEDURE sp_Sub_TinhLuong @MACN INT = NULL
AS
BEGIN
    SELECT CN.MACN, CN.TENCN, COUNT(NV.MANV) AS SoLuongNhanVien, ISNULL(SUM(NV.LUONGCOBAN), 0) AS TongLuongPhaiTra
    FROM CHINHANH CN
    LEFT JOIN LAMVIEC LV ON CN.MACN = LV.MACN
    LEFT JOIN NHANVIEN NV ON LV.MANV = NV.MANV
    WHERE (LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE()) AND (@MACN IS NULL OR CN.MACN = @MACN)
    GROUP BY CN.MACN, CN.TENCN ORDER BY TongLuongPhaiTra DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_ThongKeHieuSuatNhanVien
    @THANG INT,
    @NAM INT,
    @MACN INT = NULL 
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF @THANG < 1 OR @THANG > 12
        BEGIN
            PRINT N'Tháng không hợp lệ (1-12).';
            RETURN;
        END
        ;WITH CountHoaDon AS (
            SELECT MANV, COUNT(MAHD) AS SL_HoaDon
            FROM HOADON
            WHERE MONTH(NGAYLAP) = @THANG AND YEAR(NGAYLAP) = @NAM
            GROUP BY MANV
        ),
        CountCaKham AS (
            SELECT MANV, COUNT(MAKB) AS SL_CaKham
            FROM CAKHAMBENH
            WHERE MONTH(NGAYKHAM) = @THANG AND YEAR(NGAYKHAM) = @NAM
            GROUP BY MANV
        ),
        CountCaTiem AS (
            SELECT MANV, COUNT(MATIEM) AS SL_CaTiem
            FROM CATIEM
            WHERE MONTH(NGAYTIEM) = @THANG AND YEAR(NGAYTIEM) = @NAM
            GROUP BY MANV
        )
        SELECT 
            NV.MANV,
            NV.HOTEN,
            CASE 
                WHEN BS.MANV IS NOT NULL THEN N'Bác sĩ'
                WHEN QL.MANV IS NOT NULL THEN N'Quản lý'
                ELSE N'Nhân viên'
            END AS ChucVu,
            ISNULL(CN.TENCN, N'Chưa phân công') AS ChiNhanhHienTai,
            ISNULL(HD.SL_HoaDon, 0) AS [Số Đơn Hàng],
            ISNULL(CK.SL_CaKham, 0) AS [Số Ca Khám],
            ISNULL(CT.SL_CaTiem, 0) AS [Số Ca Tiêm],    
            (ISNULL(HD.SL_HoaDon, 0) + ISNULL(CK.SL_CaKham, 0) + ISNULL(CT.SL_CaTiem, 0)) AS [Tổng Lượt Phục Vụ]
        FROM NHANVIEN NV
        LEFT JOIN LAMVIEC LV ON NV.MANV = LV.MANV AND (LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE())
        LEFT JOIN CHINHANH CN ON LV.MACN = CN.MACN
        LEFT JOIN BACSI BS ON NV.MANV = BS.MANV
        LEFT JOIN QUANLY QL ON NV.MANV = QL.MANV
        LEFT JOIN CountHoaDon HD ON NV.MANV = HD.MANV
        LEFT JOIN CountCaKham CK ON NV.MANV = CK.MANV
        LEFT JOIN CountCaTiem CT ON NV.MANV = CT.MANV
        WHERE 
            (@MACN IS NULL OR LV.MACN = @MACN)
        ORDER BY [Tổng Lượt Phục Vụ] DESC;
    END TRY
    BEGIN CATCH
        PRINT N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- hao
CREATE OR ALTER PROCEDURE sp_KhachHang_Them
    @TENKH NVARCHAR(50),
    @SDT CHAR(10),
    @DIACHI NVARCHAR(100),
    @NewID INT OUTPUT 
AS
BEGIN
    IF EXISTS (SELECT 1 FROM KHACHHANG WHERE SDT = @SDT)
        THROW 50001, N'Số điện thoại này đã được sử dụng bởi khách hàng khác.', 1;

    INSERT INTO KHACHHANG (TENKH, SDT, DIACHI, TENCAPBAC,DIEMTICHLUY)
    VALUES (@TENKH, @SDT, @DIACHI, N'Đồng',0);

    SET @NewID = SCOPE_IDENTITY();
END;
GO

CREATE OR ALTER PROCEDURE sp_KhachHang_Sua
    @MAKH INT, 
    @TENKH NVARCHAR(50),
    @SDT CHAR(10),
    @DIACHI NVARCHAR(100)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MAKH)
        THROW 50002, N'Khách hàng không tồn tại.', 1;

    IF EXISTS (SELECT 1 FROM KHACHHANG WHERE SDT = @SDT AND MAKH <> @MAKH)
        THROW 50003, N'Số điện thoại này đã thuộc về khách hàng khác.', 1;

    UPDATE KHACHHANG
    SET TENKH = @TENKH,
        SDT = @SDT,
        DIACHI = @DIACHI
    WHERE MAKH = @MAKH;
END;
GO

CREATE OR ALTER PROCEDURE sp_KhachHang_Xoa
    @MAKH INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM HOADON WHERE MAKH = @MAKH)
        THROW 50004, N'Không thể xóa: Khách hàng này đã có hóa đơn giao dịch.', 1;

    IF EXISTS (SELECT 1 FROM THUCUNG WHERE MAKH = @MAKH)
        THROW 50005, N'Không thể xóa: Khách hàng này đang sở hữu thú cưng trên hệ thống.', 1;

    DELETE FROM KHACHHANG WHERE MAKH = @MAKH;
END;
GO

CREATE OR ALTER PROCEDURE sp_KhachHang_XemChiTiet
    @MAKH INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MAKH)
    BEGIN
        RETURN; 
    END

    SELECT 
        KH.MAKH,
        KH.TENKH,
        KH.SDT,
        KH.DIACHI,
        KH.TENCAPBAC,
        ISNULL((SELECT SUM(TONGTIEN) FROM HOADON WHERE MAKH = KH.MAKH), 0) AS TongChiTieu,
        (SELECT COUNT(*) FROM HOADON WHERE MAKH = KH.MAKH) AS SoLanGiaoDich,
        (SELECT COUNT(*) FROM THUCUNG WHERE MAKH = KH.MAKH) AS SoLuongThuCung,
        (SELECT MAX(NGAYLAP) FROM HOADON WHERE MAKH = KH.MAKH) AS LanGheGanNhat
    FROM KHACHHANG KH
    WHERE KH.MAKH = @MAKH;
END;
GO  

CREATE OR ALTER PROCEDURE sp_ThuCung_XemDanhSach
    @MAKH INT
AS
BEGIN
    SELECT 
        TC.MATC,
        TC.TENTC,       
        TC.LOAI,
        TC.TUOI,
        TC.GIOITINH,
        TC.TINHTRANG,
        KH.TENKH AS ChuSoHuu,
        KH.SDT
    FROM KHACHHANG KH
    LEFT JOIN THUCUNG TC ON TC.MAKH = KH.MAKH
    WHERE KH.MAKH = @MAKH 
    ORDER BY TC.MATC DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_ThuCung_Them
    @MAKH INT,              
    @TENTC NVARCHAR(50),    
    @LOAI NVARCHAR(20),    
    @TUOI TINYINT,         
    @GIOITINH NVARCHAR(5), 
    @NewID INT OUTPUT       
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MAKH)
        THROW 50001, N'Khách hàng không tồn tại trong hệ thống.', 1;

    INSERT INTO THUCUNG (MAKH, TENTC, LOAI, TUOI, GIOITINH, TINHTRANG)
    VALUES (@MAKH, @TENTC, @LOAI, @TUOI, @GIOITINH, N'Bình thường');

    SET @NewID = SCOPE_IDENTITY();
END;
GO

CREATE OR ALTER PROCEDURE sp_ThuCung_Sua
    @MATC INT,              
    @TENTC NVARCHAR(50),
    @LOAI NVARCHAR(20),
    @TUOI TINYINT,
    @GIOITINH NVARCHAR(5),
    @TINHTRANG NVARCHAR(20) 
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM THUCUNG WHERE MATC = @MATC)
        THROW 50002, N'Thú cưng không tồn tại hoặc đã bị xóa.', 1;

    UPDATE THUCUNG
    SET 
        TENTC = ISNULL(@TENTC, TENTC),
        LOAI = ISNULL(@LOAI, LOAI),
        TUOI = ISNULL(@TUOI, TUOI),
        GIOITINH = ISNULL(@GIOITINH, GIOITINH),
        TINHTRANG = ISNULL(@TINHTRANG, TINHTRANG)
    WHERE MATC = @MATC;
END;
GO

CREATE OR ALTER PROCEDURE sp_ThuCung_LichSuKham
    @MATC INT
AS
BEGIN
    SELECT 
        KB.MAKB,
        KB.NGAYKHAM,
        NV.HOTEN AS BacSiKham,
        ISNULL((SELECT STRING_AGG(TENTRIEUCHUNG, ', ') 
                FROM TRIEUCHUNG WHERE MAKB = KB.MAKB), N'Không ghi nhận') AS TrieuChung,
        
        ISNULL((SELECT STRING_AGG(TENCHANDOAN, ', ') 
                FROM CHANDOAN WHERE MAKB = KB.MAKB), N'Chưa kết luận') AS ChanDoan,   
        TT.GHICHU AS LoiDanBacSi    
    FROM CAKHAMBENH KB
    LEFT JOIN NHANVIEN NV ON KB.MANV = NV.MANV
    LEFT JOIN TOATHUOC TT ON KB.MAKB = TT.MAKB
    WHERE KB.MATC = @MATC
    ORDER BY KB.NGAYKHAM DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_ThuCung_LichSuTiem
    @MATC INT
AS
BEGIN
    SELECT 
        CT.MATIEM,
        CT.NGAYTIEM,
        NV.HOTEN AS NguoiTiem,
        SP.TENSP AS TenVacXin,
        VX.DOTUOIAPDUNG,
        CTX.SOLUONG AS SoMui
    FROM CATIEM CT
    JOIN CHITIETCATIEM CTX ON CT.MATIEM = CTX.MATIEM
    JOIN VACXIN VX ON CTX.MAVACXIN = VX.MAVACXIN
    JOIN SANPHAM SP ON VX.MAVACXIN = SP.MASP 
    JOIN NHANVIEN NV ON CT.MANV = NV.MANV
    WHERE CT.MATC = @MATC
    ORDER BY CT.NGAYTIEM DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_ThuCung_XemChiTiet
    @MATC INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM THUCUNG WHERE MATC = @MATC)
    BEGIN
        RETURN;
    END

    SELECT 
        TC.MATC,
        ISNULL(TC.TENTC, N'Chưa đặt tên') AS TENTC,
        TC.LOAI,
        TC.TUOI,
        TC.GIOITINH,
        TC.TINHTRANG, 
        KH.MAKH,
        KH.TENKH AS ChuSoHuu,
        KH.SDT AS SDTLienHe,
        KH.DIACHI
    FROM THUCUNG TC
    JOIN KHACHHANG KH ON TC.MAKH = KH.MAKH
    WHERE TC.MATC = @MATC;
END;
GO

CREATE OR ALTER PROCEDURE sp_HoaDon_XemChiTiet
    @MAHD INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM HOADON WHERE MAHD = @MAHD)
    BEGIN
        PRINT N'Hóa đơn không tồn tại';
        RETURN;
    END
    SELECT 
        HD.MAHD,
        HD.NGAYLAP,
        KH.TENKH,
        KH.SDT AS SDTKhachHang,
        KH.DIACHI AS DiaChiKhachHang,
        NV.HOTEN AS NhanVienLap,
        CN.TENCN AS TaiChiNhanh,
        CN.DIACHI AS DiaChiChiNhanh,
        HD.TONGTIEN,
        ISNULL(HD.KHUYENMAI, 0) AS KhuyenMai,
        (HD.TONGTIEN - ISNULL(HD.KHUYENMAI, 0)) AS ThucThu 
    FROM HOADON HD
    JOIN KHACHHANG KH ON HD.MAKH = KH.MAKH
    JOIN NHANVIEN NV ON HD.MANV = NV.MANV
    JOIN CHINHANH CN ON HD.MACN = CN.MACN
    WHERE HD.MAHD = @MAHD;
    SELECT 
        ROW_NUMBER() OVER(ORDER BY SP.TENSP) AS STT,
        SP.MASP,
        SP.TENSP,
        SP.LOAI,       
        SP.DONGIA,
        CT.SOLUONG,
        (SP.DONGIA * CT.SOLUONG) AS ThanhTien
    FROM CHITIETHOADON CT
    JOIN SANPHAM SP ON CT.MASP = SP.MASP
    WHERE CT.MAHD = @MAHD;
END;
GO

CREATE OR ALTER PROCEDURE sp_HoaDon_DanhSach
    @MAKH INT 
AS
BEGIN
    SELECT 
        HD.MAHD,
        HD.NGAYLAP,
        NV.HOTEN AS NhanVienLap,
        CN.TENCN AS TaiChiNhanh,
        HD.TONGTIEN,
        ISNULL(HD.KHUYENMAI, 0) AS KhuyenMai,
        (HD.TONGTIEN - ISNULL(HD.KHUYENMAI, 0)) AS SoTienThucTra
    FROM HOADON HD
    JOIN NHANVIEN NV ON HD.MANV = NV.MANV
    JOIN CHINHANH CN ON HD.MACN = CN.MACN
    WHERE HD.MAKH = @MAKH
    ORDER BY HD.NGAYLAP DESC; 
END;
GO

CREATE OR ALTER PROCEDURE sp_ThuCung_TheoDoiGoiTiem
    @MATC INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM DANGKYGOITIEM WHERE MATC = @MATC)
    BEGIN
        RETURN;
    END

    SELECT 
        SP_Goi.MASP AS MaGoi,
        SP_Goi.TENSP AS TenGoiTiem, 
        GT.KHUYENMAI AS MucGiamGia, 
        SP_VX.MASP AS MaVacXin,
        SP_VX.TENSP AS TenVacXin,   
        VX.DOTUOIAPDUNG,          
        CTGT.SOLUONG AS TongSoMui  
        
    FROM DANGKYGOITIEM DK
    JOIN GOITIEM GT ON DK.MAGOITIEM = GT.MAGOITIEM
    JOIN SANPHAM SP_Goi ON GT.MAGOITIEM = SP_Goi.MASP
    
    JOIN CHITIETGOITIEM CTGT ON GT.MAGOITIEM = CTGT.MAGOITIEM
    JOIN VACXIN VX ON CTGT.MAVACXIN = VX.MAVACXIN
    JOIN SANPHAM SP_VX ON VX.MAVACXIN = SP_VX.MASP
    
    WHERE DK.MATC = @MATC
    ORDER BY SP_Goi.TENSP, VX.DOTUOIAPDUNG;
END;
go
-- cap nhat vao 1/1 hang nam 
CREATE OR ALTER PROCEDURE sp_CapBac_CapNhatHangNam 
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MAKH INT;
    DECLARE @CapBacHienTai NVARCHAR(15);
    DECLARE @TienDaTieuNamNgoai DECIMAL(12,2); 
    
    DECLARE @MucGiuHang DECIMAL(12,2);
    DECLARE @CapBacMoi NVARCHAR(15);
    DECLARE @NamXetDuyet INT;

    SET @NamXetDuyet = YEAR(GETDATE()) - 1;

    DECLARE cur_KhachHang CURSOR FOR 
        SELECT MAKH, TENCAPBAC 
        FROM KHACHHANG;

    OPEN cur_KhachHang;
    
    FETCH NEXT FROM cur_KhachHang INTO @MAKH, @CapBacHienTai;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @TienDaTieuNamNgoai = 0; 

        SELECT @TienDaTieuNamNgoai = ISNULL(CHITIEU, 0) 
        FROM CHITIEU 
        WHERE MAKH = @MAKH AND NAM = @NamXetDuyet;

        SELECT @MucGiuHang = MUCGIUHANG FROM CAPBAC WHERE TENCAPBAC = @CapBacHienTai;
        
        IF @TienDaTieuNamNgoai >= @MucGiuHang
        BEGIN
            SET @CapBacMoi = @CapBacHienTai;
        END
        ELSE
        BEGIN
            SET @CapBacMoi = NULL;
            SELECT TOP 1 @CapBacMoi = TENCAPBAC 
            FROM CAPBAC 
            WHERE MUCCHITIEU <= @TienDaTieuNamNgoai
            ORDER BY MUCCHITIEU DESC;
            
            IF @CapBacMoi IS NULL SET @CapBacMoi = N'Cơ bản';
        END

        IF @CapBacMoi <> @CapBacHienTai
        BEGIN
            UPDATE KHACHHANG
            SET TENCAPBAC = @CapBacMoi
            WHERE MAKH = @MAKH;
        END

        IF NOT EXISTS (SELECT 1 FROM CHITIEU WHERE MAKH = @MAKH AND NAM = YEAR(GETDATE()))
        BEGIN
            INSERT INTO CHITIEU (MAKH, NAM, CHITIEU)
            VALUES (@MAKH, YEAR(GETDATE()), 0);
        END

        FETCH NEXT FROM cur_KhachHang INTO @MAKH, @CapBacHienTai;
    END

    CLOSE cur_KhachHang;
    DEALLOCATE cur_KhachHang;
END;
GO
CREATE OR ALTER PROCEDURE sp_HoaDon_LayThongTinChung
    @MAHD INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM HOADON WHERE MAHD = @MAHD)
    BEGIN
        PRINT N'Hóa đơn không tồn tại';
        RETURN;
    END
    SELECT 
        HD.MAHD,
        HD.NGAYLAP,
        KH.TENKH,
        KH.SDT AS SDTKhachHang,
        KH.DIACHI AS DiaChiKhachHang,
        NV.HOTEN AS NhanVienLap,
        CN.TENCN AS TaiChiNhanh,
        CN.DIACHI AS DiaChiChiNhanh,
        HD.TONGTIEN,
        ISNULL(HD.KHUYENMAI, 0) AS KhuyenMai,
        (HD.TONGTIEN - ISNULL(HD.KHUYENMAI, 0)) AS ThucThu 
    FROM HOADON HD
    JOIN KHACHHANG KH ON HD.MAKH = KH.MAKH
    JOIN NHANVIEN NV ON HD.MANV = NV.MANV
    JOIN CHINHANH CN ON HD.MACN = CN.MACN
    WHERE HD.MAHD = @MAHD;
END;
GO
CREATE OR ALTER PROCEDURE sp_HoaDon_LayChiTietHangHoa
    @MAHD INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ROW_NUMBER() OVER(ORDER BY SP.TENSP) AS STT,
        SP.MASP,
        SP.TENSP,
        SP.LOAI,        
        SP.DONGIA,
        CT.SOLUONG,
        (SP.DONGIA * CT.SOLUONG) AS ThanhTien
    FROM CHITIETHOADON CT
    JOIN SANPHAM SP ON CT.MASP = SP.MASP
    WHERE CT.MAHD = @MAHD;
END;
GO

--- hieu
/* Bảng cho chi tiết hoá đơn (khi dùng tham số bảng)
CREATE TYPE dbo.HoaDonChiTietType AS TABLE
(
    MASP    CHAR(5) NOT NULL,
    SOLUONG INT     NOT NULL
);
GO
*/

-- Procedure đặt lịch hẹn mới
CREATE OR ALTER PROCEDURE sp_DatLichHen
(
    @NgayHen    DATE,
    @ThoiGian   TIME,
    @NoiDung    NVARCHAR(200) = NULL,
    @MaKH       INT,
    @MaCN       INT,
    @MaLoaiDV   INT,
    @MaLichHen  INT OUTPUT     
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        -- Kiểm tra các tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM LOAIDICHVU WHERE MALOAIDV = @MaLoaiDV)
            RAISERROR(N'Loại dịch vụ không tồn tại', 16, 1);

        IF NOT EXISTS (
            SELECT 1 FROM CUNGCAP
            WHERE MACN = @MaCN AND MALOAIDV = @MaLoaiDV
        )
            RAISERROR(N'Dịch vụ không được cung cấp tại chi nhánh này', 16, 1);

        -- Kiểm tra trùng slot
        IF EXISTS (
            SELECT 1
            FROM LICHHEN
            WHERE NGAYHEN = @NgayHen
              AND THOIGIAN = @ThoiGian
              AND MACN = @MaCN
              AND MAKH = @MaKH
        )
            RAISERROR(N'Đã tồn tại lịch hẹn trùng thời gian với khách hàng tại chi nhánh này', 16, 1);

        INSERT INTO LICHHEN (NGAYHEN, THOIGIAN, NOIDUNG, MAKH, MACN, MALOAIDV)
        VALUES (@NgayHen, @ThoiGian, @NoiDung, @MaKH, @MaCN, @MaLoaiDV);

        SET @MaLichHen = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();

        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO
-- Processure tạo hóa đơn mới
CREATE OR ALTER PROCEDURE sp_TaoHoaDonMoi
(
    @NgayLap   DATETIME,
    @MaKH      INT,
    @MaCN      INT,
    @MaNV      INT,
    @KhuyenMai INT = 0,
    @MaHD      INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        -- Kiểm tra input
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MaNV)
            RAISERROR(N'Nhân viên không tồn tại', 16, 1);

        -- Nhân viên phải làm tại chi nhánh vào ngày lập hóa đơn
        IF NOT EXISTS (
            SELECT 1
            FROM LAMVIEC
            WHERE MANV = @MaNV
              AND MACN = @MaCN
              AND NGAYBATDAU <= CONVERT(DATE, @NgayLap)
              AND (NGAYKETTHUC IS NULL OR NGAYKETTHUC >= CONVERT(DATE, @NgayLap))
        )
            RAISERROR(N'Nhân viên không làm tại chi nhánh này vào ngày lập hóa đơn', 16, 1);

        -- Insert HOADON, trạng thái mặc định 'Chưa hoàn thành'
        INSERT INTO HOADON (NGAYLAP, KHUYENMAI, TONGTIEN, MACN, MAKH, TRANGTHAI, MANV)
        VALUES (@NgayLap, @KhuyenMai, NULL, @MaCN, @MaKH, N'Chưa hoàn thành', @MaNV);

        SET @MaHD = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO


-- Procedure cập nhật lịch hẹn
CREATE OR ALTER PROCEDURE sp_CapNhatLichHen
(
    @MaLichHen   INT,
    @NgayHenMoi  DATE,
    @ThoiGianMoi TIME,
    @NoiDungMoi  NVARCHAR(200) = NULL,
    @MaCNMoi     INT,
    @MaLoaiDVMoi INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @MaKH INT;

    BEGIN TRY
        -- Lấy khách của lịch
        SELECT @MaKH = MAKH
        FROM LICHHEN
        WHERE MALICHHEN = @MaLichHen;

        IF @MaKH IS NULL
            RAISERROR(N'Không tìm thấy lịch hẹn', 16, 1);

        -- Kiểm tra tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCNMoi)
            RAISERROR(N'Chi nhánh mới không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM LOAIDICHVU WHERE MALOAIDV = @MaLoaiDVMoi)
            RAISERROR(N'Loại dịch vụ mới không tồn tại', 16, 1);

        IF NOT EXISTS (
            SELECT 1
            FROM CUNGCAP
            WHERE MACN = @MaCNMoi AND MALOAIDV = @MaLoaiDVMoi
        )
            RAISERROR(N'Dịch vụ mới không được cung cấp tại chi nhánh này', 16, 1);

        -- Kiểm tra trùng slot với lịch khác
        IF EXISTS (
            SELECT 1
            FROM LICHHEN
            WHERE NGAYHEN = @NgayHenMoi
              AND THOIGIAN = @ThoiGianMoi
              AND MACN = @MaCNMoi
              AND MAKH = @MaKH
              AND MALICHHEN <> @MaLichHen
        )
            RAISERROR(N'Đã có lịch hẹn khác trùng slot', 16, 1);

        -- Cập nhật
        UPDATE LICHHEN
        SET NGAYHEN  = @NgayHenMoi,
            THOIGIAN = @ThoiGianMoi,
            NOIDUNG  = @NoiDungMoi,
            MACN     = @MaCNMoi,
            MALOAIDV = @MaLoaiDVMoi
        WHERE MALICHHEN = @MaLichHen;
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();

        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO
-- PRocedure tạo hóa đơn
CREATE OR ALTER PROCEDURE sp_TaoHoaDonMoi
(
    @NgayLap   DATETIME,
    @MaKH      INT,
    @MaCN      INT,
    @MaNV      INT,
    @MaHD      INT OUTPUT
)       
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @KhuyenMai INT;

    BEGIN TRY
        -- Kiểm tra input
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MaNV)
            RAISERROR(N'Nhân viên không tồn tại', 16, 1);

        -- Nhân viên phải làm tại chi nhánh vào ngày lập hóa đơn
        IF NOT EXISTS (
            SELECT 1
            FROM LAMVIEC
            WHERE MANV = @MaNV
              AND MACN = @MaCN
              AND NGAYBATDAU <= CONVERT(DATE, @NgayLap)
              AND (NGAYKETTHUC IS NULL OR NGAYKETTHUC >= CONVERT(DATE, @NgayLap))
        )
            RAISERROR(N'Nhân viên không làm tại chi nhánh này vào ngày lập hóa đơn', 16, 1);

        
        -- Lấy khuyến mãi theo CẤP BẬC khách hàng
        SELECT @KhuyenMai =
            CASE KH.TENCAPBAC
                WHEN N'Cơ bản'    THEN 0
                WHEN N'Thân thiết' THEN 10
                WHEN N'VIP'       THEN 30
                ELSE 0
            END
        FROM KHACHHANG KH
        WHERE KH.MAKH = @MaKH;

        IF @KhuyenMai IS NULL
            RAISERROR(N'Không xác định được cấp bậc khách hàng', 16, 1);

        -- Tao hoa don moi
        INSERT INTO HOADON (NGAYLAP, KHUYENMAI, TONGTIEN, MACN, MAKH, TRANGTHAI, MANV)
        VALUES (@NgayLap, @KhuyenMai, NULL, @MaCN, @MaKH, N'Chưa hoàn thành', @MaNV);

        SET @MaHD = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO

-- Procedure thêm chi tiết hoá đơn
CREATE OR ALTER PROCEDURE sp_ThemChiTietHoaDon
(
    @MaHD    INT,
    @MaSP    INT,
    @SoLuong INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        -- Kiểm tra tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM SANPHAM WHERE MASP = @MaSP)
            RAISERROR(N'Sản phẩm không tồn tại', 16, 1);

        IF @SoLuong <= 0
            RAISERROR(N'Số lượng phải lớn hơn 0', 16, 1);

        -- Hóa đơn phải còn trạng thái 'Chưa hoàn thành'
        IF NOT EXISTS (
            SELECT 1
            FROM HOADON
            WHERE MAHD = @MaHD
              AND TRANGTHAI = N'Chưa hoàn thành'
        )
            RAISERROR(N'Hóa đơn không tồn tại hoặc đã được thanh toán', 16, 1);

        -- Kiểm tra tồn kho + lấy đơn giá
        DECLARE @TonKho INT,
                @GiaBan INT;

        SELECT @TonKho = TONKHO,
               @GiaBan = DONGIA
        FROM SANPHAM
        WHERE MASP = @MaSP;

        IF @TonKho < @SoLuong
            RAISERROR(N'Tồn kho không đủ để đáp ứng yêu cầu', 16, 1);

        -- Thêm chi tiết hoá đơn, lưu GIABAN = DONGIA hiện tại
        INSERT INTO CHITIETHOADON (MAHD, MASP, SOLUONG, GIABAN)
        VALUES (@MaHD, @MaSP, @SoLuong, @GiaBan);

        -- Cập nhật tồn kho
        UPDATE SANPHAM
        SET TONKHO = TONKHO - @SoLuong
        WHERE MASP = @MaSP;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO
-- Procedure thanh toan
CREATE OR ALTER PROCEDURE sp_ThanhToanHoaDon
(
    @MaHD INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        -- Hóa đơn phải tồn tại và còn 'Chưa hoàn thành'
        IF NOT EXISTS (
            SELECT 1
            FROM HOADON
            WHERE MAHD = @MaHD
              AND TRANGTHAI = N'Chưa hoàn thành'
        )
            RAISERROR(N'Hóa đơn không tồn tại hoặc đã được thanh toán', 16, 1);

        DECLARE @TongHang  DECIMAL(12, 2),
                @TongTien  DECIMAL(12, 2),
                @KhuyenMai INT,
                @MaKH      INT,
                @NgayLap   DATE,
                @Nam       SMALLINT;

        -- Tính tổng tiền hàng từ chi tiết hoá đơn
        SELECT @TongHang = SUM(CAST(GIABAN AS DECIMAL(12,2)) * SOLUONG)
        FROM CHITIETHOADON
        WHERE MAHD = @MaHD;

        IF @TongHang IS NULL
            RAISERROR(N'Hóa đơn chưa có chi tiết', 16, 1);

        -- Lấy thông tin hóa đơn
        SELECT  @KhuyenMai = ISNULL(KHUYENMAI, 0),
                @MaKH      = MAKH,
                @NgayLap   = CONVERT(DATE, NGAYLAP)
        FROM HOADON
        WHERE MAHD = @MaHD;

        -- Áp khuyến mãi
        SET @TongTien = @TongHang * (1 - @KhuyenMai / 100.0);

        IF @TongTien <= 0
            RAISERROR(N'Tổng tiền không hợp lệ', 16, 1);

        UPDATE HOADON
        SET TONGTIEN  = @TongTien,
            TRANGTHAI = N'Đã thanh toán'
        WHERE MAHD = @MaHD;

        -- Cập nhật CHITIEU 
        SET @Nam = YEAR(@NgayLap);

        IF EXISTS (SELECT 1 FROM CHITIEU WHERE MAKH = @MaKH AND NAM = @Nam)
        BEGIN
            UPDATE CHITIEU
            SET CHITIEU = CHITIEU + @TongTien
            WHERE MAKH = @MaKH AND NAM = @Nam;
        END
        ELSE
        BEGIN
            INSERT INTO CHITIEU (MAKH, NAM, CHITIEU)
            VALUES (@MaKH, @Nam, @TongTien);
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_KhachHang_DangKy
    @HoTen          NVARCHAR(50),
    @SDT            CHAR(10),
    @DiaChi         NVARCHAR(100),
    @TenDangNhap    VARCHAR(20),
    @MatKhauHash    CHAR(32), 
    @Salt           CHAR(36) -- <--- THAM SỐ MỚI
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        IF EXISTS (SELECT 1 FROM TAIKHOAN WHERE TENDANGNHAP = @TenDangNhap)
            THROW 50002, N'Tên đăng nhập đã tồn tại.', 1;

        DECLARE @NewMAKH INT;
        EXEC sp_KhachHang_Them @TENKH = @HoTen, @SDT = @SDT, @DIACHI = @DiaChi, @NewID = @NewMAKH OUTPUT;

        -- Insert có thêm cột SALT
        INSERT INTO TAIKHOAN (TENDANGNHAP, MATKHAU, SALT, TRANGTHAI, MAKH, MANV)
        VALUES (@TenDangNhap, @MatKhauHash, @Salt, 1, @NewMAKH, NULL);

        COMMIT TRAN;
        SELECT N'Đăng ký thành công' AS ThongBao;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_TaiKhoan_DangNhap
    @InputIdentifier VARCHAR(20), 
    @MatKhauHash    CHAR(32)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TenDangNhapThuc VARCHAR(20);
    DECLARE @MatKhauDB CHAR(32);
    DECLARE @TrangThai BIT;
    DECLARE @MANV INT;
    DECLARE @MAKH INT;
    SELECT TOP 1 
        @TenDangNhapThuc = TK.TENDANGNHAP,
        @MatKhauDB = TK.MATKHAU,
        @TrangThai = TK.TRANGTHAI,
        @MANV = TK.MANV,
        @MAKH = TK.MAKH
    FROM TAIKHOAN TK
    LEFT JOIN NHANVIEN NV ON TK.MANV = NV.MANV
    LEFT JOIN KHACHHANG KH ON TK.MAKH = KH.MAKH
    WHERE TK.TENDANGNHAP = @InputIdentifier 
       OR NV.SDT = @InputIdentifier 
       OR KH.SDT = @InputIdentifier;
    IF @TenDangNhapThuc IS NULL 
    BEGIN
        RAISERROR(N'Tài khoản hoặc số điện thoại không tồn tại.', 16, 1);
        RETURN; 
    END

    IF @TrangThai = 0 
    BEGIN
        RAISERROR(N'Tài khoản đã bị khóa.', 16, 1);
        RETURN;
    END

    IF @MatKhauDB <> @MatKhauHash 
    BEGIN
        RAISERROR(N'Mật khẩu không chính xác.', 16, 1);
        RETURN;
    END
    IF @MANV IS NOT NULL
    BEGIN
        SELECT TOP 1
            TK.TENDANGNHAP AS UserName,
            NV.HOTEN AS FullName,
            NV.MANV AS UserId,
            CASE ISNULL(LV.VAITRO, NV.VAITRO)
                WHEN 'BS' THEN 'BacSi'
                WHEN 'QL' THEN 'QuanLy'
                WHEN 'NV' THEN 'NhanVien'
                ELSE 'NhanVien'
            END AS Role,
            ISNULL(CN.MACN, 0) AS WorkBranchId,
            ISNULL(CN.TENCN, N'Chưa phân công') AS WorkBranchName

        FROM TAIKHOAN TK
        JOIN NHANVIEN NV ON TK.MANV = NV.MANV
        LEFT JOIN LAMVIEC LV ON NV.MANV = LV.MANV 
             AND (LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE())
        LEFT JOIN CHINHANH CN ON LV.MACN = CN.MACN
        
        WHERE TK.TENDANGNHAP = @TenDangNhapThuc
        ORDER BY LV.NGAYBATDAU DESC; 
    END
    ELSE
    BEGIN
        SELECT 
            TK.TENDANGNHAP AS UserName,
            KH.TENKH AS FullName,
            KH.MAKH AS UserId,
            'KhachHang' AS Role,
            0 AS WorkBranchId,
            N'' AS WorkBranchName
        FROM TAIKHOAN TK
        JOIN KHACHHANG KH ON TK.MAKH = KH.MAKH
        WHERE TK.TENDANGNHAP = @TenDangNhapThuc;
    END
END;
GO

CREATE OR ALTER PROCEDURE sp_TaiKhoan_LaySalt
    @InputIdentifier VARCHAR(20)
AS
BEGIN
    -- Chỉ trả về Salt nếu tài khoản tồn tại và đang hoạt động
    SELECT SALT 
    FROM TAIKHOAN 
    WHERE TENDANGNHAP = @InputIdentifier  AND TRANGTHAI = 1;
END;
GO

CREATE OR ALTER PROCEDURE sp_NhanVien_XemLichHen
    @MaCN INT,
    @SdtKhachHang VARCHAR(20) = NULL -- Nếu NULL thì hiện tất cả hôm nay
AS
BEGIN
    SELECT 
        LH.MALICHHEN,
        LH.NGAYHEN,
        LH.THOIGIAN,
        KH.MAKH,
        KH.TENKH,
        KH.SDT,
        LDV.MALOAIDV,
        LDV.TENLOAIDV,
        LH.NOIDUNG AS GhiChu
    FROM LICHHEN LH
    JOIN KHACHHANG KH ON LH.MAKH = KH.MAKH
    JOIN LOAIDICHVU LDV ON LH.MALOAIDV = LDV.MALOAIDV
    WHERE LH.MACN = @MaCN -- Chỉ hiện lịch của chi nhánh nhân viên đang làm
      AND (@SdtKhachHang IS NULL OR KH.SDT LIKE '%' + @SdtKhachHang + '%')
    ORDER BY LH.NGAYHEN, LH.THOIGIAN;
END;
GO

-- 2. Procedure: Xác nhận lịch hẹn & Tạo hồ sơ (Khám hoặc Tiêm)
CREATE OR ALTER PROCEDURE sp_NhanVien_XacNhanLichHen
    @MaLichHen INT,
    @MaNV      INT,
    @MaTC      INT -- Bắt buộc phải chọn thú cưng để tạo hồ sơ
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        -- A. Lấy thông tin loại dịch vụ từ lịch hẹn
        DECLARE @MaLoaiDV INT;
        SELECT @MaLoaiDV = MALOAIDV FROM LICHHEN WHERE MALICHHEN = @MaLichHen;

        -- B. Cập nhật trạng thái lịch hẹn -> "Đã đến" (Hoặc Đã hoàn thành)
        -- Tùy quy trình bên bạn, ở đây tôi set là Đã hoàn thành để ẩn khỏi danh sách chờ
        -- UPDATE LICHHEN SET TRANGTHAI = N'Đã hoàn thành' WHERE MALICHHEN = @MaLichHen;
        -- Tuy nhiên trong bảng DDL của bạn check constraint là: 'Chưa thanh toán', 'Đã thanh toán', 'Đã hủy'
        -- Nên tạm thời ta không update trạng thái hoàn thành ở đây mà để quy trình thanh toán lo, 
        -- hoặc ta coi việc tạo hồ sơ là bước đầu tiên. 
        -- Ở đây tôi giữ nguyên hoặc bạn có thể thêm trạng thái 'Đang khám' vào Check Constraint sau.

        -- C. Tạo hồ sơ dựa trên loại dịch vụ
        
        -- Nếu là KHÁM BỆNH
        IF @MaLoaiDV = 3
        BEGIN
            INSERT INTO CAKHAMBENH (MATC, MANV, NGAYKHAM)
            VALUES (@MaTC, @MaNV, GETDATE());
        END
        
        -- Nếu là TIÊM PHÒNG
        ELSE IF @MaLoaiDV = 6
        BEGIN
            INSERT INTO CATIEM (MATC, MANV, NGAYTIEM)
            VALUES (@MaTC, @MaNV, GETDATE());
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        THROW;
    END CATCH
END;
GO