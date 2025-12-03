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

-- QuanLyHeThong
CREATE PROCEDURE sp_Sub_NhanVien_Xem
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
        RETURN;
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

CREATE PROCEDURE sp_Sub_NhanVien_Them
    @HOTEN NVARCHAR(50), @NGAYSINH DATE, @GIOITINH NVARCHAR(5), @SDT CHAR(10), @LUONGCOBAN INT,
    @VAITRO VARCHAR(2), @BANGCAP NVARCHAR(50) = NULL, @KINHNGHIEM INT = 0, @MACN_QUANLY INT = NULL
AS
BEGIN
    IF @VAITRO = 'BS' AND @BANGCAP IS NULL THROW 50001, N'Thiếu bằng cấp bác sĩ.', 1;
    IF @VAITRO = 'QL' AND @MACN_QUANLY IS NULL THROW 50002, N'Thiếu mã chi nhánh quản lý.', 1;
    IF EXISTS (SELECT 1 FROM NHANVIEN WHERE SDT = @SDT) THROW 50003, N'Trùng SĐT.', 1;

    INSERT INTO NHANVIEN (HOTEN, NGAYSINH, GIOITINH, SDT, LUONGCOBAN, VAITRO)
    VALUES (@HOTEN, @NGAYSINH, @GIOITINH, @SDT, @LUONGCOBAN, @VAITRO);

    DECLARE @NewMANV INT = SCOPE_IDENTITY();

    IF @VAITRO = 'BS' INSERT INTO BACSI (MANV, BANGCAP, KINHNGHIEM) VALUES (@NewMANV, @BANGCAP, @KINHNGHIEM);
    ELSE IF @VAITRO = 'QL' INSERT INTO QUANLY (MANV, NGAYBONHIEM, MACN) VALUES (@NewMANV, GETDATE(), @MACN_QUANLY);
    PRINT N'Đã thêm thành công. Mã: ' + CAST(@NewMANV AS NVARCHAR(20));
END;
GO

CREATE PROCEDURE sp_Sub_NhanVien_Sua
    @MANV INT, @HOTEN NVARCHAR(50), @NGAYSINH DATE, @GIOITINH NVARCHAR(5), @SDT CHAR(10), @LUONGCOBAN INT,
    @BANGCAP NVARCHAR(50) = NULL, @KINHNGHIEM INT = 0, @MACN_QUANLY INT = NULL
AS
BEGIN
    IF @MANV IS NULL THROW 50005, N'Thiếu mã NV.', 1;

    UPDATE NHANVIEN SET HOTEN = @HOTEN, NGAYSINH = @NGAYSINH, GIOITINH = @GIOITINH, SDT = @SDT, LUONGCOBAN = @LUONGCOBAN WHERE MANV = @MANV;

    IF EXISTS (SELECT 1 FROM BACSI WHERE MANV = @MANV)
        UPDATE BACSI SET BANGCAP = @BANGCAP, KINHNGHIEM = @KINHNGHIEM WHERE MANV = @MANV;
    ELSE IF EXISTS (SELECT 1 FROM QUANLY WHERE MANV = @MANV) AND @MACN_QUANLY IS NOT NULL
        UPDATE QUANLY SET MACN = @MACN_QUANLY WHERE MANV = @MANV;

    PRINT N'Đã cập nhật thông tin.';
END;
GO

--- QuanLyChiNhanh
CREATE PROCEDURE sp_Sub_ChiNhanh_Xem AS 
BEGIN SELECT * FROM CHINHANH; END;
GO

CREATE PROCEDURE sp_Sub_ChiNhanh_Them
    @TENCN NVARCHAR(50), @DIACHI NVARCHAR(100), @SDT CHAR(10), @GIOMOCUA TIME, @GIODONGCUA TIME
AS
BEGIN
    IF EXISTS (SELECT 1 FROM CHINHANH WHERE SDT = @SDT) THROW 50006, N'SĐT CN đã tồn tại.', 1;

    INSERT INTO CHINHANH (TENCN, DIACHI, SDT, GIOMOCUA, GIODONGCUA) VALUES (@TENCN, @DIACHI, @SDT, @GIOMOCUA, @GIODONGCUA);
    DECLARE @NewMACN INT = SCOPE_IDENTITY();
    PRINT N'Đã thêm CN: ' + CAST(@NewMACN AS NVARCHAR(20));
END;
GO

CREATE PROCEDURE sp_Sub_ChiNhanh_Sua
    @MACN INT, @TENCN NVARCHAR(50), @DIACHI NVARCHAR(100), @SDT CHAR(10), @GIODM TIME, @GIODONGCUA TIME
AS
BEGIN
    IF @MACN IS NULL THROW 50008, N'Thiếu mã CN.', 1;
    IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MACN) THROW 50009, N'CN không tồn tại.', 1;
    IF EXISTS (SELECT 1 FROM CHINHANH WHERE SDT = @SDT AND MACN <> @MACN) THROW 50010, N'SĐT trùng nơi khác.', 1;

    UPDATE CHINHANH SET TENCN = @TENCN, DIACHI = @DIACHI, SDT = @SDT, GIOMOCUA = @GIOMOCUA, GIODONGCUA = @GIODONGCUA 
    WHERE MACN = @MACN;
    PRINT N'Đã cập nhật CN: ' + CAST(@MACN AS NVARCHAR(20));
END;
GO

-- QuanLyLichSuPhanCong
CREATE PROCEDURE sp_Sub_LichSu_PhanCong
    @MACN INT, @MANV INT, @NGAYBATDAU DATE, @NGAYKETTHUC DATE, @VAITRO VARCHAR(2)
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MACN) THROW 50011, N'CN không tồn tại.', 1;
    IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MANV) THROW 50012, N'NV không tồn tại.', 1;
    IF @VAITRO NOT IN ('NV','QL','BS') THROW 50015, N'Vai trò không hợp lệ (NV/QL/BS).', 1;

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
            UPDATE LAMVIEC SET NGAYKETTHUC = @NGAYBATDAU WHERE MANV = @MANV AND NGAYKETTHUC IS NULL;
            PRINT N'Đã đóng công việc cũ.';
        END
        INSERT INTO LAMVIEC (MACN, MANV, NGAYBATDAU, NGAYKETTHUC, VAITRO) VALUES (@MACN, @MANV, @NGAYBATDAU, NULL, @VAITRO);
        UPDATE NHANVIEN SET VAITRO = @VAITRO WHERE MANV = @MANV;
        PRINT N'Đã phân công mới.';
    END
END;
GO

CREATE PROCEDURE sp_Sub_LichSu_XemNV @MANV INT 
AS
BEGIN
    SELECT LV.MANV, NV.HOTEN, LV.MACN, CN.TENCN, LV.NGAYBATDAU,
           CASE WHEN LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE() THEN N'Đang làm việc' ELSE N'Đã nghỉ' END AS TRANGTHAI,
           LV.NGAYKETTHUC
    FROM LAMVIEC LV JOIN NHANVIEN NV ON LV.MANV = NV.MANV JOIN CHINHANH CN ON LV.MACN = CN.MACN
    WHERE LV.MANV = @MANV ORDER BY LV.NGAYBATDAU DESC;
END;
GO

CREATE PROCEDURE sp_Sub_LichSu_XemCN @MACN INT 
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
CREATE PROCEDURE sp_Sub_TinhLuong @MACN INT = NULL
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

CREATE PROCEDURE sp_ThongKeHieuSuatNhanVien
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

