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
    @Nam INT,
    @Thang INT
AS
BEGIN
    IF @Thang IS NOT NULL AND @Thang BETWEEN 1 AND 12
    BEGIN
        SELECT
            MACN,
            @Nam AS N'Năm',
            @Thang AS N'Tháng',
            COUNT(DISTINCT hd.MAHD) AS N'Số lượng đơn',
            SUM(HD.TONGTIEN) AS N'Tổng Doanh thu',
            SUM((HD.KHUYENMAI / 100) * HD.TONGTIEN) AS N'Tổng tiền chiết khấu'
        FROM HOADON 
        WHERE @Nam = YEAR(NGAYLAP) AND @Thang = MONTH(NGAYLAP)
        GROUP BY MACN
    END
    ELSE IF @Thang IS NULL
    BEGIN
        SELECT 
            MACN,
            @Nam AS N'Năm',
            COUNT(*) AS N'Số lượng đơn',
            SUM(TONGTIEN) AS N'Tổng Doanh thu',
            SUM((HD.KHUYENMAI / 100) * HD.TONGTIEN) AS N'Tổng tiền chiết khấu'
        FROM HOADON
        WHERE @Nam = YEAR(NGAYLAP)
        GROUP BY MACN
    END
END
GO

-- thống kê lượng sản phẩm bán được trong tháng/năm theo từng loại hàng
GO
CREATE OR ALTER PROCEDURE sp_ThongKeSanPham
    @Thang INTEGER, 
    @Nam INTEGER
AS
BEGIN
    IF @Thang IS NOT NULL
    BEGIN
        SELECT 
            @Nam,
            @Thang,
            SP.LOAI,
            SUM(CT.SOLUONG) AS N'Tổng số lượng'
        FROM CHITIETHOADON CT
        JOIN SANPHAM SP ON SP.MASP = CT.MASP
        WHERE YEAR(NGAYLAP) = @Nam AND MONTH(NGAYLAP) = @Thang
        GROUP BY SP.LOAI
    END
    ELSE IF @Thang IS NULL
    BEGIN
        SELECT 
            @Nam,
            @Thang,
            SP.LOAI,
            SUM(CT.SOLUONG) AS N'Tổng số lượng'
        FROM CHITIETHOADON CT
        JOIN SANPHAM SP ON SP.MASP = CT.MASP
        WHERE YEAR(NGAYLAP) = @Nam
        GROUP BY SP.LOAI
    END
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
            DATEDIFF(GETDATE(), NgayMuaHangGanNhat) AS 'SoNgay_NgayMuaHangGanNhat',
            TanSuat,
            TongChi,
            NTILE(5) OVER (ORDER BY DATEDIFF(GETDATE(), NgayMuaHangGanNhat) DESC) AS R_DIEM,
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
            WHEN rfm.R_Diem >= 4 AND rfm.F_Diem >= 4 AND rfm.M_Diem >= 4 THEN N'Khách hàng gần đây',
            WHEN rfm.R_Diem >= 3 AND rfm.F_Diem >= 3 AND rfm.M_Diem >= 3 THEN N'Khách hàng trung thành',
            WHEN rfm.R_Diem >= 3 AND rfm.F_Diem >= 2 AND rfm.M_Diem >= 2 THEN N'Tiềm năng',
            WHEN rfm.R_Diem < 2 AND rfm.F_Diem >= 2 AND rfm.M_Diem >= 2 THEN N'Mua lâu, từng thường xuyên đến',
            WHEN rfm.R_Diem < 2 AND rfm.F_Diem < 2 AND rfm.M_Diem < 2 THEN N'Mua lâu, ít khi mua, chi ít',
    FROM RFM_Diem rfm
    ORDER BY MAKH
END
GO

-- Lượng khách hàng trong tháng (tổng, cũ, mới) (trên toàn hệ thống)
GO
CREATE OR ALTER PROCEDURE sp_ThongKeKhachHang
    @Thang INTEGER = NULL,
    @Nam INTEGER = NULL
AS
BEGIN

    IF @Thang IS NULL SET @Thang = MONTH(GETDATE())
    IF @Nam IS NULL SET @Nam = YEAR(GETDATE())

    DECLARE @DauThangHienTai DATE = DATEFROMPARTS(@Nam, @Thang, 1)

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
END
GO

-- thống kê đánh giá khách hàng 
GO
CREATE OR ALTER PROCEDURE sp_ThongKeDanhGia
AS 
BEGIN
    SET NOCOUNT ON;

    WITH DiemDichVu AS (
        SELECT 
            HD.MACN,
            DG.DIEMDICHVU 'DIEM',
            COUNT((DG.DIEMDICHVU)) 'SOLUONG'
        FROM HOADON HD
        JOIN DANHGIA DG ON HD.MAHD = DG.MAHD -- chỉ quan tâm những hoá đơn có đánh giá => không dùng left join
        WHERE HD.MADANHGIA IS NOT NULL AND DG.DIEMDICHVU IS NOT NULL
        GROUP BY HD.MACN, DG.DIEMDICHVU
    ), DiemHaiLong AS (
        SELECT 
            HD.MACN,
            DG.MUCDOHAILONG 'DIEM',
            COUNT(DG.MUCDOHAILONG) 'SOLUONG'
        FROM HOADON HD
        JOIN DANHGIA DG ON HD.MAHD = DG.MAHD
        WHERE HD.MADANHGIA IS NOT NULL AND DG.MUCDOHAILONG IS NOT NULL
        GROUP BY HD.MACN, DG.MUCDOHAILONG
    )

    SELECT
        COALESCE(DV.MACN, HL.MACN) AS N'Mã chi nhánh'
        DV.DIEM AS N'Điểm dịch vụ',
        DV.SOLUONG AS N'Số lượng',
        HL.DIEM AS N'Điểm hài lòng',
        HL.SOLUONG AS N'Số lượng'
    FROM DiemDichVu DV
    FULL OUTER JOIN DiemHaiLong HL ON DV.MACN = HL.MACN
                                AND DV.DIEM = HL.DIEM

END
GO

-- thống kê sản phẩm bán chạy
GO
CREATE OR ALTER PROCEDURE sp_SanPhamBanChay
    @Thang INTEGER = NULL,
    @Nam INTEGER = YEAR(GETDATE())
AS
BEGIN
    SET NOCOUNT ON

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
END
GO