USE PetcareX;
GO
-- QuanLyHeThong
CREATE PROCEDURE sp_QuanLyHoSoNhanVien
    @MANV CHAR(5) = NULL,           
    @HOTEN NVARCHAR(50) = NULL,
    @NGAYSINH DATE = NULL,
    @GIOITINH NVARCHAR(5) = NULL,
    @SDT CHAR(10) = NULL,
    @LUONGCOBAN DECIMAL(12,2) = NULL,
    @LOAINV NVARCHAR(20) = NULL,     
    @BANGCAP NVARCHAR(50) = NULL,   
    @KINHNGHIEM INT = 0,            
    @MACN_QUANLY CHAR(5) = NULL,      
    @LOAIHANHDONG NVARCHAR(10)    
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF @LOAIHANHDONG = 'XEM'
        BEGIN
            IF @MANV IS NOT NULL
            BEGIN
                IF LEFT(@MANV, 2) = 'BS'
                BEGIN
                    SELECT NV.MANV, NV.HOTEN, NV.SDT, NV.GIOITINH, NV.LUONGCOBAN,
                           BS.BANGCAP, BS.KINHNGHIEM 
                    FROM NHANVIEN NV 
                    LEFT JOIN BACSI BS ON NV.MANV = BS.MANV
                    WHERE NV.MANV = @MANV;
                END
                ELSE IF LEFT(@MANV, 2) = 'QL'
                BEGIN
                    SELECT NV.MANV, NV.HOTEN, NV.SDT, NV.GIOITINH, NV.LUONGCOBAN,
                           QL.NGAYBONHIEM, QL.MACN AS MaChiNhanhQuanLy
                    FROM NHANVIEN NV 
                    LEFT JOIN QUANLY QL ON NV.MANV = QL.MANV
                    WHERE NV.MANV = @MANV;
                END
                ELSE 
                BEGIN
                    SELECT * FROM NHANVIEN WHERE MANV = @MANV;
                END
            END
            ELSE 
            BEGIN
                IF @LOAINV = N'Bác sĩ'
                    SELECT NV.*, BS.BANGCAP, BS.KINHNGHIEM FROM NHANVIEN NV JOIN BACSI BS ON NV.MANV = BS.MANV;
                ELSE IF @LOAINV = N'Quản lý'
                    SELECT NV.*, QL.NGAYBONHIEM, QL.MACN FROM NHANVIEN NV JOIN QUANLY QL ON NV.MANV = QL.MANV;
                ELSE
                    SELECT * FROM NHANVIEN;
            END        
            COMMIT TRANSACTION;
            RETURN;
        END
        IF @LOAIHANHDONG = 'THEM'
        BEGIN
            IF @LOAINV = N'Bác sĩ' AND @BANGCAP IS NULL
            BEGIN ROLLBACK TRANSACTION; PRINT N'Thiếu bằng cấp bác sĩ.'; RETURN; END
            IF @LOAINV = N'Quản lý' AND @MACN_QUANLY IS NULL
            BEGIN ROLLBACK TRANSACTION; PRINT N'Thiếu mã chi nhánh quản lý.'; RETURN; END
            DECLARE @Prefix CHAR(2); DECLARE @MaxMANV CHAR(5); DECLARE @NextNumber INT; DECLARE @NewMANV CHAR(5);
            IF @LOAINV = N'Bác sĩ' SET @Prefix = 'BS';
            ELSE IF @LOAINV = N'Quản lý' SET @Prefix = 'QL';
            ELSE SET @Prefix = 'NV';
            SELECT @MaxMANV = MAX(MANV) FROM NHANVIEN WHERE MANV LIKE @Prefix + '%';
            IF @MaxMANV IS NULL SET @NewMANV = @Prefix + '001';
            ELSE
            BEGIN
                SET @NextNumber = CAST(RIGHT(@MaxMANV, 3) AS INT) + 1;
                IF @NextNumber > 999 
                BEGIN ROLLBACK TRANSACTION; PRINT N'Hết số.'; RETURN; END
                SET @NewMANV = @Prefix + RIGHT('000' + CAST(@NextNumber AS VARCHAR(3)), 3);
            END
            IF EXISTS (SELECT 1 FROM NHANVIEN WHERE SDT = @SDT)
            BEGIN ROLLBACK TRANSACTION; PRINT N'Trùng SĐT.'; RETURN; END
            INSERT INTO NHANVIEN (MANV, HOTEN, NGAYSINH, GIOITINH, SDT, LUONGCOBAN)
            VALUES (@NewMANV, @HOTEN, @NGAYSINH, @GIOITINH, @SDT, @LUONGCOBAN);
            IF @LOAINV = N'Bác sĩ'
                INSERT INTO BACSI (MANV, BANGCAP, KINHNGHIEM) VALUES (@NewMANV, @BANGCAP, @KINHNGHIEM);
            ELSE IF @LOAINV = N'Quản lý'
                INSERT INTO QUANLY (MANV, NGAYBONHIEM, MACN) VALUES (@NewMANV, GETDATE(), @MACN_QUANLY);
            PRINT N'Đã thêm thành công. Mã: ' + @NewMANV;
        END
        ELSE IF @LOAIHANHDONG = 'SUA'
        BEGIN
            IF @MANV IS NULL BEGIN ROLLBACK TRANSACTION; PRINT N'Thiếu mã NV.'; RETURN; END    
            UPDATE NHANVIEN
            SET HOTEN = @HOTEN, NGAYSINH = @NGAYSINH, GIOITINH = @GIOITINH, SDT = @SDT, LUONGCOBAN = @LUONGCOBAN
            WHERE MANV = @MANV;
            IF LEFT(@MANV, 2) = 'BS'
            BEGIN
                UPDATE BACSI SET BANGCAP = @BANGCAP, KINHNGHIEM = @KINHNGHIEM WHERE MANV = @MANV;
            END
            ELSE IF LEFT(@MANV, 2) = 'QL'
            BEGIN
                IF @MACN_QUANLY IS NOT NULL
                    UPDATE QUANLY SET MACN = @MACN_QUANLY WHERE MANV = @MANV;
            END      
            PRINT N'Đã cập nhật thông tin.';
        END
        ELSE IF @LOAIHANHDONG = 'XEM ALL'
        BEGIN
            SELECT NV.MANV, NV.HOTEN, NV.SDT, NV.GIOITINH, NV.LUONGCOBAN,
                   BS.BANGCAP, BS.KINHNGHIEM,
                   QL.NGAYBONHIEM, QL.MACN AS MaChiNhanhQuanLy
            FROM NHANVIEN NV
            LEFT JOIN BACSI BS ON NV.MANV = BS.MANV
            LEFT JOIN QUANLY QL ON NV.MANV = QL.MANV;
        END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        PRINT N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE PROCEDURE sp_QuanLyThongTinChiNhanh
@MACN CHAR(5) = NULL, 
    @TENCN NVARCHAR(50) = NULL,   
    @DIACHI NVARCHAR(100) = NULL, 
    @SDT CHAR(10) = NULL,        
    @GIODM TIME = NULL,           
    @GIODONGCUA TIME = NULL,      
    @LOAIHANHDONG NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF @LOAIHANHDONG = 'XEM ALL'
        BEGIN
            SELECT * FROM CHINHANH;
        END
        ELSE IF @LOAIHANHDONG = 'THEM'
        BEGIN
            DECLARE @MaxMACN CHAR(5);
            DECLARE @NextNumber INT;
            DECLARE @NewMACN CHAR(5);
            SELECT @MaxMACN = MAX(MACN) FROM CHINHANH WHERE MACN LIKE 'CN%';
            IF @MaxMACN IS NULL
            BEGIN
                SET @NewMACN = 'CN001';
            END
            ELSE
            BEGIN
                SET @NextNumber = CAST(RIGHT(@MaxMACN, 3) AS INT) + 1;
                IF @NextNumber > 999
                BEGIN
                    ROLLBACK TRANSACTION;
                    PRINT N'Hệ thống đã đầy mã chi nhánh (Max 999).';
                    RETURN;
                END
                SET @NewMACN = 'CN' + RIGHT('000' + CAST(@NextNumber AS VARCHAR(3)), 3);
            END
            IF EXISTS (SELECT 1 FROM CHINHANH WHERE SDT = @SDT)
            BEGIN
                ROLLBACK TRANSACTION;
                PRINT N'Số điện thoại chi nhánh đã tồn tại.';
                RETURN;
            END

            INSERT INTO CHINHANH (MACN, TENCN, DIACHI, SDT, GIODM, GIODONGCUA)
            VALUES (@NewMACN, @TENCN, @DIACHI, @SDT, @GIODM, @GIODONGCUA);

            PRINT N'Đã thêm chi nhánh thành công. Mã mới: ' + @NewMACN;
        END
        ELSE IF @LOAIHANHDONG = 'SUA'
        BEGIN
            IF @MACN IS NULL
            BEGIN
                ROLLBACK TRANSACTION;
                PRINT N'Vui lòng nhập Mã chi nhánh cần sửa.';
                RETURN;
            END
            IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MACN)
            BEGIN
                ROLLBACK TRANSACTION;
                PRINT N'Chi nhánh không tồn tại.';
                RETURN;
            END
            IF EXISTS (SELECT 1 FROM CHINHANH WHERE SDT = @SDT AND MACN <> @MACN)
            BEGIN
                ROLLBACK TRANSACTION;
                PRINT N'Số điện thoại đã được sử dụng bởi chi nhánh khác.';
                RETURN;
            END
            UPDATE CHINHANH
            SET TENCN = @TENCN,
                DIACHI = @DIACHI,
                SDT = @SDT,
                GIODM = @GIODM,
                GIODONGCUA = @GIODONGCUA
            WHERE MACN = @MACN;
            PRINT N'Đã cập nhật thông tin chi nhánh ' + @MACN;
        END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        PRINT N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE PROCEDURE sp_GhiNhanLichSuLamViec
    @MACN CHAR(5) = NULL,           
    @MANV CHAR(5) = NULL,           
    @NGAYBATDAU DATE = NULL, 
    @NGAYKETTHUC DATE = NULL 
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF @MACN IS NOT NULL AND @MANV IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MACN)
            BEGIN ROLLBACK TRANSACTION; PRINT N'Chi nhánh không tồn tại.'; RETURN; END
            IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MANV)
            BEGIN ROLLBACK TRANSACTION; PRINT N'Nhân viên không tồn tại.'; RETURN; END

            IF @NGAYKETTHUC IS NOT NULL
            BEGIN
                DECLARE @LatestStartDate DATE;
                SELECT TOP 1 @LatestStartDate = NGAYBATDAU FROM LAMVIEC
                WHERE MACN = @MACN AND MANV = @MANV ORDER BY NGAYBATDAU DESC;

                IF @LatestStartDate IS NULL
                BEGIN ROLLBACK TRANSACTION; PRINT N'Không tìm thấy lịch sử để cập nhật.'; RETURN; END

                IF @NGAYKETTHUC < @LatestStartDate
                BEGIN ROLLBACK TRANSACTION; PRINT N'Ngày kết thúc không hợp lệ.'; RETURN; END
                UPDATE LAMVIEC SET NGAYKETTHUC = @NGAYKETTHUC
                WHERE MACN = @MACN AND MANV = @MANV AND NGAYBATDAU = @LatestStartDate;
                PRINT N'Đã cập nhật ngày kết thúc thủ công.';
            END
            ELSE 
            BEGIN
                IF @NGAYBATDAU IS NULL SET @NGAYBATDAU = GETDATE();
                IF EXISTS (SELECT 1 FROM LAMVIEC WHERE MANV = @MANV AND NGAYKETTHUC IS NULL)
                BEGIN
                    UPDATE LAMVIEC
                    SET NGAYKETTHUC = @NGAYBATDAU 
                    WHERE MANV = @MANV AND NGAYKETTHUC IS NULL;

                    PRINT N'Lưu ý: Đã tự động kết thúc công việc tại chi nhánh cũ.';
                END
                INSERT INTO LAMVIEC (MACN, MANV, NGAYBATDAU, NGAYKETTHUC)
                VALUES (@MACN, @MANV, @NGAYBATDAU, NULL);

                PRINT N'Đã phân công nhân viên sang chi nhánh mới thành công.';
            END
        END
        ELSE IF @MANV IS NOT NULL AND @MACN IS NULL
        BEGIN
            SELECT LV.MANV, NV.HOTEN, LV.MACN, CN.TENCN, LV.NGAYBATDAU,
                CASE WHEN LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE() THEN N'Đang làm việc' ELSE N'Đã nghỉ' END AS TRANGTHAI,
                LV.NGAYKETTHUC
            FROM LAMVIEC LV JOIN NHANVIEN NV ON LV.MANV = NV.MANV JOIN CHINHANH CN ON LV.MACN = CN.MACN
            WHERE LV.MANV = @MANV ORDER BY LV.NGAYBATDAU DESC;
        END
        ELSE IF @MACN IS NOT NULL AND @MANV IS NULL 
        BEGIN
            SELECT LV.MACN, CN.TENCN, LV.MANV, NV.HOTEN, NV.SDT, NV.LUONGCOBAN, LV.NGAYBATDAU,
                CASE WHEN LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE() THEN N'Đang làm việc' ELSE N'Đã nghỉ việc' END AS TRANGTHAI
            FROM LAMVIEC LV JOIN CHINHANH CN ON LV.MACN = CN.MACN JOIN NHANVIEN NV ON LV.MANV = NV.MANV
            WHERE LV.MACN = @MACN AND (LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE())
            ORDER BY LV.NGAYBATDAU DESC;
        END
        ELSE BEGIN ROLLBACK TRANSACTION; PRINT N'Thiếu thông tin.'; RETURN; END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION; PRINT N'Lỗi: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

CREATE PROCEDURE sp_ThongKeLuongChiNhanh
    @MACN CHAR(5) = NULL 
AS
BEGIN
    SELECT 
        CN.MACN,
        CN.TENCN,
        COUNT(NV.MANV) AS SoLuongNhanVien,
        ISNULL(SUM(NV.LUONGCOBAN), 0) AS TongLuongPhaiTra
    FROM 
        CHINHANH CN
    LEFT JOIN 
        LAMVIEC LV ON CN.MACN = LV.MACN
    LEFT JOIN 
        NHANVIEN NV ON LV.MANV = NV.MANV
    WHERE 
        (LV.NGAYKETTHUC IS NULL OR LV.NGAYKETTHUC >= GETDATE())
        AND (@MACN IS NULL OR CN.MACN = @MACN)
    GROUP BY 
        CN.MACN, CN.TENCN
    ORDER BY 
        TongLuongPhaiTra DESC;
END;
GO