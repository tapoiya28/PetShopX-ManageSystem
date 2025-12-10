USE PetcareX;
GO

-- ============ STORED PROCEDURES CHO KHÁM BỆNH ============

-- Tạo ca khám mới
GO
CREATE OR ALTER PROCEDURE sp_CaKham_Tao
    @MATC INT,
    @MANV INT,
    @NGAYKHAM DATE,
    @MAKB INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM THUCUNG WHERE MATC = @MATC)
            THROW 50001, N'Thú cưng không tồn tại', 1;
            
        IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MANV)
            THROW 50002, N'Nhân viên không tồn tại', 1;

        INSERT INTO CAKHAMBENH (MATC, MANV, NGAYKHAM)
        VALUES (@MATC, @MANV, @NGAYKHAM);
        
        SET @MAKB = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Thêm triệu chứng
GO
CREATE OR ALTER PROCEDURE sp_TrieuChung_Them
    @MAKB INT,
    @TENTRIEUCHUNG NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM CAKHAMBENH WHERE MAKB = @MAKB)
            THROW 50001, N'Ca khám không tồn tại', 1;

        INSERT INTO TRIEUCHUNG (MAKB, TENTRIEUCHUNG)
        VALUES (@MAKB, @TENTRIEUCHUNG);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Xóa triệu chứng
GO
CREATE OR ALTER PROCEDURE sp_TrieuChung_Xoa
    @MAKB INT,
    @TENTRIEUCHUNG NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM TRIEUCHUNG 
    WHERE MAKB = @MAKB AND TENTRIEUCHUNG = @TENTRIEUCHUNG;
END;
GO

-- Thêm chẩn đoán
GO
CREATE OR ALTER PROCEDURE sp_ChanDoan_Them
    @MAKB INT,
    @TENCHANDOAN NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM CAKHAMBENH WHERE MAKB = @MAKB)
            THROW 50001, N'Ca khám không tồn tại', 1;

        INSERT INTO CHANDOAN (MAKB, TENCHANDOAN)
        VALUES (@MAKB, @TENCHANDOAN);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Xóa chẩn đoán
GO
CREATE OR ALTER PROCEDURE sp_ChanDoan_Xoa
    @MAKB INT,
    @TENCHANDOAN NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM CHANDOAN 
    WHERE MAKB = @MAKB AND TENCHANDOAN = @TENCHANDOAN;
END;
GO

-- Tạo/Lấy toa thuốc
GO
CREATE OR ALTER PROCEDURE sp_ToaThuoc_TaoHoacLay
    @MAKB INT,
    @GHICHU NVARCHAR(500) = NULL,
    @MATT INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Kiểm tra xem đã có toa chưa
        SELECT @MATT = MATT FROM TOATHUOC WHERE MAKB = @MAKB;
        
        IF @MATT IS NULL
        BEGIN
            -- Tạo toa mới
            INSERT INTO TOATHUOC (MAKB, GHICHU)
            VALUES (@MAKB, @GHICHU);
            
            SET @MATT = SCOPE_IDENTITY();
        END
        ELSE IF @GHICHU IS NOT NULL
        BEGIN
            -- Cập nhật ghi chú nếu có
            UPDATE TOATHUOC SET GHICHU = @GHICHU WHERE MATT = @MATT;
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Thêm thuốc vào toa
GO
CREATE OR ALTER PROCEDURE sp_ToaThuoc_ThemThuoc
    @MATT INT,
    @MATHUOC INT,
    @SOLUONG INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM TOATHUOC WHERE MATT = @MATT)
            THROW 50001, N'Toa thuốc không tồn tại', 1;
            
        IF NOT EXISTS (SELECT 1 FROM THUOC WHERE MATHUOC = @MATHUOC)
            THROW 50002, N'Thuốc không tồn tại', 1;

        -- Kiểm tra xem thuốc đã có trong toa chưa
        IF EXISTS (SELECT 1 FROM CHITIETTOATHUOC WHERE MATT = @MATT AND MATHUOC = @MATHUOC)
        BEGIN
            -- Cập nhật số lượng
            UPDATE CHITIETTOATHUOC 
            SET SOLUONG = SOLUONG + @SOLUONG
            WHERE MATT = @MATT AND MATHUOC = @MATHUOC;
        END
        ELSE
        BEGIN
            -- Thêm mới
            INSERT INTO CHITIETTOATHUOC (MATT, MATHUOC, SOLUONG)
            VALUES (@MATT, @MATHUOC, @SOLUONG);
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Xóa thuốc khỏi toa
GO
CREATE OR ALTER PROCEDURE sp_ToaThuoc_XoaThuoc
    @MATT INT,
    @MATHUOC INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM CHITIETTOATHUOC 
    WHERE MATT = @MATT AND MATHUOC = @MATHUOC;
END;
GO

-- Cập nhật ghi chú toa thuốc
GO
CREATE OR ALTER PROCEDURE sp_ToaThuoc_CapNhatGhiChu
    @MAKB INT,
    @GHICHU NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE TOATHUOC 
    SET GHICHU = @GHICHU 
    WHERE MAKB = @MAKB;
END;
GO

-- Lấy danh sách khách hàng
GO
CREATE OR ALTER PROCEDURE sp_KhachHang_DanhSach
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MAKH, TENKH + ' - ' + SDT AS Display, TENKH, SDT
    FROM KHACHHANG 
    ORDER BY TENKH;
END;
GO

-- Lấy danh sách thú cưng theo khách hàng
GO
CREATE OR ALTER PROCEDURE sp_ThuCung_DanhSachTheoKH
    @MAKH INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MATC, TENTC + ' (' + LOAI + ')' AS Display, TENTC, LOAI
    FROM THUCUNG 
    WHERE MAKH = @MAKH 
    ORDER BY TENTC;
END;
GO

-- Lấy danh sách thuốc còn trong kho
GO
CREATE OR ALTER PROCEDURE sp_Thuoc_DanhSach
AS
BEGIN
    SET NOCOUNT ON;
    SELECT T.MATHUOC, SP.TENSP, SP.TONKHO, T.DONVI
    FROM THUOC T
    JOIN SANPHAM SP ON T.MATHUOC = SP.MASP
    WHERE SP.LOAI = N'Thuốc' AND SP.TONKHO > 0
    ORDER BY SP.TENSP;
END;
GO

-- Lấy thông tin chi tiết ca khám
GO
CREATE OR ALTER PROCEDURE sp_CaKham_ChiTiet
    @MAKB INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        KB.MAKB,
        TC.TENTC,
        TC.LOAI,
        KH.TENKH,
        KB.NGAYKHAM
    FROM CAKHAMBENH KB
    JOIN THUCUNG TC ON KB.MATC = TC.MATC
    JOIN KHACHHANG KH ON TC.MAKH = KH.MAKH
    WHERE KB.MAKB = @MAKB;
END;
GO

-- Tìm lịch sử khám theo SĐT khách hàng
GO
CREATE OR ALTER PROCEDURE sp_LichSuKham_TimTheoSDT
    @SDT NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT
        KB.MAKB,
        KB.NGAYKHAM,
        TC.MATC,
        TC.TENTC AS [Tên Thú Cưng],
        TC.LOAI AS [Loại],
        KH.TENKH AS [Chủ Sở Hữu],
        KH.SDT,
        NV.HOTEN AS [Bác Sĩ Khám]
    FROM CAKHAMBENH KB
    JOIN THUCUNG TC ON KB.MATC = TC.MATC
    JOIN KHACHHANG KH ON TC.MAKH = KH.MAKH
    LEFT JOIN NHANVIEN NV ON KB.MANV = NV.MANV
    WHERE KH.SDT LIKE '%' + @SDT + '%'
    ORDER BY KB.NGAYKHAM DESC;
END;
GO

-- Lấy danh sách triệu chứng của ca khám
GO
CREATE OR ALTER PROCEDURE sp_TrieuChung_DanhSach
    @MAKB INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ROW_NUMBER() OVER(ORDER BY TENTRIEUCHUNG) AS [STT],
        TENTRIEUCHUNG AS [Triệu Chứng]
    FROM TRIEUCHUNG
    WHERE MAKB = @MAKB;
END;
GO

-- Lấy danh sách chẩn đoán của ca khám
GO
CREATE OR ALTER PROCEDURE sp_ChanDoan_DanhSach
    @MAKB INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ROW_NUMBER() OVER(ORDER BY TENCHANDOAN) AS [STT],
        TENCHANDOAN AS [Chẩn Đoán]
    FROM CHANDOAN
    WHERE MAKB = @MAKB;
END;
GO

-- Lấy toa thuốc chi tiết
GO
CREATE OR ALTER PROCEDURE sp_ToaThuoc_ChiTiet
    @MAKB INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Lấy danh sách thuốc
    SELECT 
        CTT.MATT,
        SP.TENSP AS [Tên Thuốc],
        CTT.SOLUONG AS [Số Lượng],
        T.DONVI AS [Đơn Vị]
    FROM TOATHUOC TT
    JOIN CHITIETTOATHUOC CTT ON TT.MATT = CTT.MATT
    JOIN THUOC T ON CTT.MATHUOC = T.MATHUOC
    JOIN SANPHAM SP ON T.MATHUOC = SP.MASP
    WHERE TT.MAKB = @MAKB;
    
    -- Lấy ghi chú
    SELECT TOP 1 GHICHU 
    FROM TOATHUOC 
    WHERE MAKB = @MAKB;
END;
GO
