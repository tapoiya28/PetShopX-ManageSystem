USE PetcareX;
GO

-- BÁO CÁO THỐNG KÊ
-- tính tổng sản phẩm trong 1 hoá đơn
GO
CREATE OR ALTER FUNCTION f_TongSanPhamCuaHoaDon(@MAHD VARCHAR(12))
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
        MONTH(NGAYLAP) AS N'Tháng'
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
    @Thang INTEGER NULL, 
    @Nam INTEGER NULL
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
    RETURN SELECT MASP
    FROM SANPHAM
    WHERE LOAI <> N'Dịch vụ' AND TONKHO < 50
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
            END
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

        IF @Thang IS NULL SET @Thang = MONTH(GETDATE())
        IF @Nam IS NULL SET @Nam = YEAR(GETDATE())

        DECLARE @DauThangHienTai DATE = DATEFROMPARTS(@Nam, @Thang, 1);

        WITH MuaHangTrongThang AS (
            SELECT 
                DISTINCT HD.MAKH
            FROM HOADON HD 
            WHERE HD.NGAYLAP >= @DauThangHienTai
                AND HD.NGAYLAP <= EOMONTH(@DauThangHienTai)
        ), KhachHangCTE AS (
            SELECT 
                MAKH, 
                CASE 
                    WHEN EXISTS (SELECT 1 
                                FROM HOADON HD
                                WHERE HD.MAKH = kh.MAKH 
                                AND HD.NGAYLAP < @DauThangHienTai
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
        WITH DiemDichVu AS (
            SELECT 
                HD.MACN,
                DG.DIEMDICHVU 'DIEM',
                COUNT((DG.DIEMDICHVU)) 'SOLUONG'
            FROM HOADON HD
            JOIN DANHGIA DG ON HD.MAHD = DG.MAHD -- chỉ quan tâm những hoá đơn có đánh giá => không dùng left join
            WHERE DG.DIEMDICHVU IS NOT NULL
            GROUP BY HD.MACN, DG.DIEMDICHVU
        ), DiemHaiLong AS (
            SELECT 
                HD.MACN,
                DG.MUCDOHAILONG 'DIEM',
                COUNT(DG.MUCDOHAILONG) 'SOLUONG'
            FROM HOADON HD
            JOIN DANHGIA DG ON HD.MAHD = DG.MAHD
            WHERE DG.MUCDOHAILONG IS NOT NULL
            GROUP BY HD.MACN, DG.MUCDOHAILONG
        )

        SELECT
            COALESCE(DV.MACN, HL.MACN) AS N'Mã chi nhánh',
            DV.DIEM AS N'Điểm dịch vụ',
            DV.SOLUONG AS N'Số lượng',
            HL.DIEM AS N'Điểm hài lòng',
            HL.SOLUONG AS N'Số lượng'
        FROM DiemDichVu AS DV
        FULL OUTER JOIN DiemHaiLong AS HL ON DV.MACN = HL.MACN
                                    AND DV.DIEM = HL.DIEM
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
        HAVING SUM(CT.SOLUONG) > 500
        ORDER BY SOLUONG DESC
    END TRY
    BEGIN CATCH
        PRINT 'Lỗi: ' + ERROR_MESSAGE()
    END CATCH
END
GO

-- tra cứu thông tin sản phẩm
CREATE OR ALTER PROCEDURE sp_TraCuuThongTinSanPham
    @MASP CHAR(5)
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