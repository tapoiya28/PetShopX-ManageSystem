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
-- Tạo kiểu bảng cho chi tiết hoá đơn(khi thêm tham số bảng)
CREATE TYPE dbo.HoaDonChiTietType AS TABLE
(
    MASP    INTEGER NOT NULL,
    SOLUONG INTEGER NOT NULL
);
GO
--- Procedure đặt lịch hẹn
CREATE OR ALTER PROCEDURE sp_DatLichHen
(
    @NgayHen   DATE,
    @ThoiGian  TIME,
    @NoiDung   NVARCHAR(200) = NULL,
    @MaKH      INTEGER,
    @MaCN      INTEGER,
    @MaLoaiDV  INTEGER
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Auto rollback nếu có lỗi

    BEGIN TRY
        BEGIN TRAN;

        -- Kiểm tra tồn tại
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM LOAIDICHVU WHERE MALOAIDV = @MaLoaiDV)
            RAISERROR(N'Loại dịch vụ không tồn tại', 16, 1);

        IF NOT EXISTS (
            SELECT 1
            FROM CUNGCAP
            WHERE MACN = @MaCN AND MALOAIDV = @MaLoaiDV
        )
            RAISERROR(N'Dịch vụ không được cung cấp tại chi nhánh này', 16, 1);

        -- Kiểm tra trùng slot (ngày, giờ, chi nhánh, khách hàng)
        IF EXISTS (
            SELECT 1
            FROM LICHHEN
            WHERE NGAYHEN  = @NgayHen
              AND THOIGIAN = @ThoiGian
              AND MACN     = @MaCN
              AND MAKH     = @MaKH
        )
            RAISERROR(N'Đã tồn tại lịch hẹn trùng thời gian với khách hàng tại chi nhánh này', 16, 1);
        /* comment vì code trên mac chưa sửa lại địa chỉ partition
        IF EXISTS (
            SELECT 1
            FROM LICHHEN_partitioned
            WHERE NGAYHEN  = @NgayHen
              AND THOIGIAN = @ThoiGian
              AND MACN     = @MaCN
              AND MAKH     = @MaKH
        )
            RAISERROR(N'Đã tồn tại lịch hẹn trùng thời gian với khách hàng tại chi nhánh này', 16, 1);
        */
        -- Insert LICHHEN
        INSERT INTO LICHHEN (NGAYHEN, THOIGIAN, NOIDUNG, MAKH, MACN, MALOAIDV)
        VALUES (@NgayHen, @ThoiGian, @NoiDung, @MaKH, @MaCN, @MaLoaiDV);

        /* comment vì code trên mac chưa sửa lại địa chỉ partition 
        -- Insert LICHHEN_partitioned (trên partition scheme)
        INSERT INTO LICHHEN_partitioned (MALICHHEN, NGAYHEN, THOIGIAN, NOIDUNG, MAKH, MACN, MALOAIDV)
        VALUES (@MaLichHen, @NgayHen, @ThoiGian, @NoiDung, @MaKH, @MaCN, @MaLoaiDV);
        */
        COMMIT TRAN;
        RETURN 0;
    END TRY
    -- Check các lỗi phát sinh
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);

        RETURN 1;
    END CATCH
END;
GO
--- Procedure cập nhật lịch hẹn
CREATE PROCEDURE sp_CapNhatLichHen
(
    @MaLichHen   INTEGER,
    @NgayHenMoi  DATE = NULL,
    @ThoiGianMoi TIME = NULL,
    @NoiDungMoi  NVARCHAR(200) = NULL,
    @MaCNMoi     INTEGER = NULL,
    @MaLoaiDVMoi INTEGER = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @NgayHenMoi IS NULL AND
       @ThoiGianMoi IS NULL AND
       @NoiDungMoi IS NULL AND
       @MaCNMoi IS NULL AND
       @MaLoaiDVMoi IS NULL
    BEGIN
        PRINT N'Không có thay đổi nào được cung cấp.';
        RETURN 0;
    END;

    DECLARE @MaKH INTEGER;

    BEGIN TRY
        BEGIN TRAN;
        
        -- Lấy thông tin hiện tại
        SELECT TOP 1
            @MaKH = MAKH
        FROM LICHHEN
        WHERE MALICHHEN = @MaLichHen;

        IF @MaKH IS NULL
            RAISERROR(N'Không tìm thấy lịch hẹn', 16, 1);

        -- Kiểm tra các tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCNMoi)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM LOAIDICHVU WHERE MALOAIDV = @MaLoaiDVMoi)
            RAISERROR(N'Loại dịch vụ mới không tồn tại', 16, 1);

        IF NOT EXISTS (
            SELECT 1 FROM CUNGCAP
            WHERE MACN = @MaCNMoi AND MALOAIDV = @MaLoaiDVMoi
        )
            RAISERROR(N'Dịch vụ mới không được cung cấp tại chi nhánh', 16, 1);

        -- Kiểm tra trùng slot với lịch khác
        IF EXISTS (
            SELECT 1
            FROM LICHHEN
            WHERE NGAYHEN  = @NgayHenMoi
              AND THOIGIAN = @ThoiGianMoi
              AND MACN     = @MaCNMoi
              AND MAKH     = @MaKH
              AND MALICHHEN <> @MaLichHen
        )
            RAISERROR(N'Đã có lịch hẹn khác trùng slot', 16, 1);
        /* comment vì code trên mac chưa sửa lại địa chỉ partition
        IF EXISTS (
            SELECT 1
            FROM LICHHEN_partitioned
            WHERE NGAYHEN  = @NgayHenMoi
              AND THOIGIAN = @ThoiGianMoi
              AND MACN     = @MaCNMoi
              AND MAKH     = @MaKH
              AND MALICHHEN <> @MaLichHen
        )
            RAISERROR(N'Đã có lịch hẹn khác trùng slot', 16, 1);
        */

        -- Cập nhật LICHHEN
        UPDATE LICHHEN
        SET NGAYHEN  = ISNULL(@NgayHenMoi, NGAYHEN),
            THOIGIAN = ISNULL(@ThoiGianMoi, THOIGIAN),
            NOIDUNG  = ISNULL(@NoiDungMoi, NOIDUNG),
            MACN     = ISNULL(@MaCNMoi, MACN),
            MALOAIDV = ISNULL(@MaLoaiDVMoi, MALOAIDV)
        WHERE MALICHHEN = @MaLichHen;

        /* comment vì code trên mac chưa sửa lại địa chỉ partition
        -- Cập nhật LICHHEN_partitioned
        UPDATE LICHHEN_partitioned
        SET NGAYHEN  = @NgayHenMoi,
            THOIGIAN = @ThoiGianMoi,
            NOIDUNG  = @NoiDungMoi,
            MACN     = @MaCNMoi,
            MALOAIDV = @MaLoaiDVMoi
        WHERE MALICHHEN = @MaLichHen;
        */
        COMMIT TRAN;
        RETURN 0;
    END TRY
    -- Check các lỗi phát sinh
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);

        RETURN 1
    END CATCH
END;
GO
--- Procedure lập hoá đơn và thanh toán
CREATE PROCEDURE sp_LapHoaDonVaThanhToan
(
    @MaHD      INTEGER,
    @NgayLap   DATETIME,
    @MaKH      INTEGER,
    @MaCN      INTEGER,
    @MaNV      INTEGER,
    @KhuyenMai INT = 0,
    @ChiTiet   dbo.HoaDonChiTietType READONLY -- Bảng chi tiết hoá đơn
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @TongTien DECIMAL(12,2);
    DECLARE @Nam SMALLINT, @Thang TINYINT;

    BEGIN TRY
        BEGIN TRAN;
        SET @KQ = 0;

        -- Kiểm tra các tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MaNV)
            RAISERROR(N'Nhân viên không tồn tại', 16, 1);

        -- Kiểm tra nhân viên có làm ở chi nhánh vào ngày lập không
        IF NOT EXISTS (
            SELECT 1 FROM LAMVIEC
            WHERE MANV = @MaNV
              AND MACN = @MaCN
              AND NGAYBATDAU <= CONVERT(DATE, @NgayLap)
              AND (NGAYKETTHUC IS NULL OR NGAYKETTHUC >= CONVERT(DATE, @NgayLap))
        )
            RAISERROR(N'Nhân viên không làm tại chi nhánh này vào ngày lập hóa đơn', 16, 1);

        -- Hoá đơn phải có ít nhất 1 dòng
        IF NOT EXISTS (SELECT 1 FROM @ChiTiet)
            RAISERROR(N'Hoá đơn phải có ít nhất một sản phẩm', 16, 1);

        -- Kiểm tra sản phẩm hợp lệ
        IF EXISTS (
            SELECT 1
            FROM @ChiTiet ct
            LEFT JOIN SANPHAM sp ON sp.MASP = ct.MASP
            WHERE sp.MASP IS NULL
        )
            RAISERROR(N'Tồn tại sản phẩm không hợp lệ trong hoá đơn', 16, 1);

        -- Kiểm tra tồn kho
        IF EXISTS (
            SELECT 1
            FROM @ChiTiet ct
            JOIN SANPHAM sp ON sp.MASP = ct.MASP
            GROUP BY ct.MASP, sp.TONKHO
            HAVING SUM(ct.SOLUONG) > sp.TONKHO
        )
            RAISERROR(N'Số lượng mua vượt quá tồn kho cho một số sản phẩm', 16, 1);

        -- Tính tổng tiền
        SELECT @TongTien = SUM(ct.SOLUONG * sp.DONGIA)
        FROM @ChiTiet ct
        JOIN SANPHAM sp ON sp.MASP = ct.MASP;

        SET @TongTien = @TongTien - ISNULL(@KhuyenMai, 0);

        IF @TongTien <= 0
            RAISERROR(N'Tổng tiền sau khuyến mãi phải > 0', 16, 1);

        -- Insert HOADON
        INSERT INTO HOADON (MAHD, NGAYLAP, KHUYENMAI, TONGTIEN, MACN, MAKH, MANV)
        VALUES (@MaHD, @NgayLap, @KhuyenMai, @TongTien, @MaCN, @MaKH, @MaNV);

        /* comment vì code trên mac chưa sửa lại địa chỉ partition
        -- Insert HOADON_partitioned (trên partition)
        INSERT INTO HOADON_partitioned (MAHD, NGAYLAP, KHUYENMAI, TONGTIEN, MACN, MAKH, MANV)
        VALUES (@MaHD, @NgayLap, @KhuyenMai, @TongTien, @MaCN, @MaKH, @MaNV);
        */

        -- Insert chi tiết hoá đơn
        INSERT INTO CHITIETHOADON (MAHD, MASP, SOLUONG)
        SELECT @MaHD, MASP, SOLUONG
        FROM @ChiTiet;

        -- Cập nhật tồn kho
        UPDATE sp
        SET TONKHO = sp.TONKHO - src.SL
        FROM SANPHAM sp
        JOIN (
            SELECT MASP, SUM(SOLUONG) AS SL
            FROM @ChiTiet
            GROUP BY MASP
        ) src ON sp.MASP = src.MASP;

        -- Cập nhật CHITIEU
        SET @Nam   = YEAR(@NgayLap);
        SET @Thang = MONTH(@NgayLap);
        -- Nếu đã có mục tiêu trong tháng thì cộng dồn, nếu chưa thì thêm mới
        IF EXISTS (SELECT 1 FROM CHITIEU WHERE MAKH = @MaKH AND NAM = @Nam AND THANG = @Thang)
        BEGIN
            UPDATE CHITIEU
            SET CHITIEU = CHITIEU + @TongTien
            WHERE MAKH = @MaKH AND NAM = @Nam AND THANG = @Thang;
        END
        ELSE
        BEGIN
            INSERT INTO CHITIEU (MAKH, NAM, THANG, CHITIEU)
            VALUES (@MaKH, @Nam, @Thang, @TongTien);
        END

        COMMIT TRAN;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);

        RETURN 1;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_ThanhToan

--- Procedure thêm đánh giá hoá đơn
CREATE PROCEDURE sp_ThemDanhGiaHoaDon
(
    @MaHD          INTEGER,
    @MaKH          INTEGER,
    @DiemDichVu    TINYINT,
    @MucDoHaiLong  TINYINT,
    @BinhLuan      NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        -- Kiểm tra các tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        -- Hóa đơn phải tồn tại và thuộc về khách này
        IF NOT EXISTS (
            SELECT 1
            FROM HOADON
            WHERE MAHD = @MaHD AND MAKH = @MaKH
        )
            RAISERROR(N'Hóa đơn không tồn tại hoặc không thuộc về khách hàng này.', 16, 1);

        -- Mỗi hóa đơn chỉ được đánh giá 1 lần
        IF EXISTS (SELECT 1 FROM DANHGIA WHERE MAHD = @MaHD)
            RAISERROR(N'Hóa đơn này đã được đánh giá', 16, 1);
        -- Ràng buộc điểm dịch vụ và mức độ hài lòng từ 1 đến 5
        IF @DiemDichVu NOT BETWEEN 1 AND 5
            RAISERROR(N'Điểm dịch vụ phải từ 1 đến 5', 16, 1);

        IF @MucDoHaiLong NOT BETWEEN 1 AND 5
            RAISERROR(N'Mức độ hài lòng phải từ 1 đến 5', 16, 1);

        INSERT INTO DANHGIA (DIEMDICHVU, MUCDOHAILONG, BINHLUAN, MAKH, MAHD)
        VALUES (@DiemDichVu, @MucDoHaiLong, @BinhLuan, @MaKH, @MaHD);

        COMMIT TRAN;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);

        RETURN 1;
    END CATCH
END;
GO

/*Exec (chưa có sample data)
-- Đặt lịch hẹn
DECLARE @KQ INT;

EXEC sp_DatLichHen
    @MaLichHen = 'LH0000000001',
    @NgayHen   = '2025-12-01',
    @ThoiGian  = '09:00',
    @NoiDung   = N'Khám sức khoẻ định kỳ',
    @MaKH      = 'KH0000000001',
    @MaCN      = 'CN001',
    @MaLoaiDV  = 'LDV01',
    @KQ        = @KQ OUTPUT;

SELECT @KQ AS KetQua_DatLich;
-- 1 = thành công, 0 = thất bại (và sẽ có message lỗi trả về)


DECLARE @KQ INT;

EXEC sp_CapNhatLichHen
    @MaLichHen   = 'LH0000000001',
    @NgayHenMoi  = '2025-12-02',
    @ThoiGianMoi = '10:30',
    @NoiDungMoi  = N'Đổi giờ khám sang buổi trưa',
    @MaCNMoi     = 'CN001',   -- có thể giữ nguyên chi nhánh
    @MaLoaiDVMoi = 'LDV01',   -- hoặc đổi sang loại DV khác nếu muốn
    @KQ          = @KQ OUTPUT;

SELECT @KQ AS KetQua_CapNhatLich;
GO

--- Lập hoá đơn và thanh toán
-- Bước 1: Tạo biến bảng chi tiết hoá đơn
DECLARE @CT dbo.HoaDonChiTietType;

INSERT INTO @CT (MASP, SOLUONG)
VALUES ('SP001', 2),      -- mua 2 sản phẩm SP001
       ('SP002', 1);      -- mua 1 sản phẩm SP002

-- Bước 2: Gọi procedure
DECLARE @KQ INT;

EXEC sp_LapHoaDonVaThanhToan
    @MaHD      = 'HD0000000001',
    @NgayLap   = '2025-12-01T10:00:00',
    @MaKH      = 'KH0000000001',
    @MaCN      = 'CN001',
    @MaNV      = 'NV001',
    @KhuyenMai = 0,
    @ChiTiet   = @CT,
    @KQ        = @KQ OUTPUT;

SELECT @KQ AS KetQua_LapHoaDon;

-- Xem lại dữ liệu:
SELECT * FROM HOADON WHERE MAHD = 'HD0000000001';
SELECT * FROM CHITIETHOADON WHERE MAHD = 'HD0000000001';
SELECT * FROM SANPHAM WHERE MASP IN ('SP001', 'SP002'); -- kiểm tra TONKHO đã trừ
SELECT * FROM CHITIEU WHERE MAKH = 'KH0000000001';
GO

-- Thêm đánh giá hoá đơn
DECLARE @KQ INT;

EXEC sp_ThemDanhGiaHoaDon
    @MaDanhGia     = 'DG0000000001',
    @MaHD          = 'HD0000000001',
    @MaKH          = 'KH0000000001',
    @DiemDichVu    = 5,
    @MucDoHaiLong  = 5,
    @BinhLuan      = N'Dịch vụ rất tốt, nhân viên nhiệt tình',
    @KQ            = @KQ OUTPUT;

SELECT @KQ AS KetQua_DanhGia;

SELECT * FROM DANHGIA WHERE MAHD = 'HD0000000001';

*/