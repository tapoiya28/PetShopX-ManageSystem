CREATE OR ALTER PROCEDURE sp_KhachHang_Them
    @TENKH NVARCHAR(50),
    @SDT CHAR(10),
    @DIACHI NVARCHAR(100),
    @NewID INT OUTPUT 
AS
BEGIN
    IF EXISTS (SELECT 1 FROM KHACHHANG WHERE SDT = @SDT)
        THROW 50001, N'Số điện thoại này đã được sử dụng bởi khách hàng khác.', 1;

    INSERT INTO KHACHHANG (TENKH, SDT, DIACHI, TENCAPBAC)
    VALUES (@TENKH, @SDT, @DIACHI, N'Đồng');

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
        TC.GIONG,
        TC.TUOI,
        TC.GIOITINH,
        TC.TINHTRANG,
        KH.TENKH AS ChuSoHuu,
        KH.SDT
    FROM THUCUNG TC
    JOIN KHACHHANG KH ON TC.MAKH = KH.MAKH
    ORDER BY TC.MATC DESC;
END;
GO

CREATE OR ALTER PROCEDURE sp_ThuCung_Them
    @MAKH INT,              
   @TENTC NVARCHAR(50),    
    @LOAI NVARCHAR(20),    
    @GIONG NVARCHAR(50),    
    @TUOI TINYINT,         
    @GIOITINH NVARCHAR(5), 
    @NewID INT OUTPUT       
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MAKH)
        THROW 50001, N'Khách hàng không tồn tại trong hệ thống.', 1;

    INSERT INTO THUCUNG (MAKH, TENTC, LOAI, GIONG, TUOI, GIOITINH, TINHTRANG)
    VALUES (@MAKH,@LOAI, @GIONG, @TUOI, @GIOITINH, N'Bình thường');

    SET @NewID = SCOPE_IDENTITY();
END;
GO

CREATE OR ALTER PROCEDURE sp_ThuCung_Sua
    @MATC INT,              
@TENTC NVARCHAR(50),
    @LOAI NVARCHAR(20),
    @GIONG NVARCHAR(50),
    @TUOI TINYINT,
    @GIOITINH NVARCHAR(5),
    @TINHTRANG NVARCHAR(20) 
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM THUCUNG WHERE MATC = @MATC)
        THROW 50002, N'Thú cưng không tồn tại hoặc đã bị xóa.', 1;

    UPDATE THUCUNG
    SET 
        TENTC = @TENTC,
        LOAI = @LOAI,
        GIONG = @GIONG,
        TUOI = @TUOI,
        GIOITINH = @GIOITINH,
        TINHTRANG = @TINHTRANG
    WHERE MATC = @MATC;
END;
GO
CREATE OR ALTER PROCEDURE sp_CapNhatCapBacKhachHang
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        UPDATE KH
        SET TENCAPBAC = (
            SELECT TOP 1 CB.TENCAPBAC
            FROM CAPBAC CB
            WHERE CB.MUCCHITIEU <= ISNULL(KH.TONGCHITIEU, 0)
            ORDER BY CB.MUCCHITIEU DESC 
        )
        FROM KHACHHANG KH;

        PRINT N'Đã cập nhật cấp bậc dựa trên tổng chi tiêu hiện tại.';
    END TRY
    BEGIN CATCH
        PRINT N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
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
        VX.LIEULUONG,
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
        TC.GIONG,
        TC.TUOI,
        TC.GIOITINH,
        TC.TINHTRANG, 
        KH.MAKH,
        KH.TENKH AS ChuSoHuu,
        KH.SDT AS SDTLienHe,
        KH.DIACHI,
        (SELECT MAX(NGAYKHAM) FROM CAKHAMBENH WHERE MATC = TC.MATC) AS LanKhamGanNhat,
        (SELECT MAX(NGAYTIEM) FROM CATIEM WHERE MATC = TC.MATC) AS LanTiemGanNhat,
        (
            (SELECT COUNT(*) FROM CAKHAMBENH WHERE MATC = TC.MATC) + 
            (SELECT COUNT(*) FROM CATIEM WHERE MATC = TC.MATC)
        ) AS TongSoLanGheTham,
        CASE 
            WHEN EXISTS (SELECT 1 FROM DANGKYGOITIEM WHERE MATC = TC.MATC) THEN N'Đang sử dụng gói'
            ELSE N'Tiêm lẻ'
       END AS TrangThaiGoiTiem
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
        SELECT @TienDaTieuNamNgoai = CHITIEU 
        FROM CHITIEU 
        WHERE MAKH = @MAKH AND NAM = @NamXetDuyet;
        
        IF @TienDaTieuNamNgoai IS NULL SET @TienDaTieuNamNgoai = 0;

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
            WHERE MUCLENHANG <= @TienDaTieuNamNgoai
            ORDER BY MUCLENHANG DESC;
            
            IF @CapBacMoi IS NULL SET @CapBacMoi = N'Đồng';
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