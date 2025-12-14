USE PetcareX;
GO

IF OBJECT_ID('dbo.TAIKHOAN', 'U') IS NOT NULL DROP TABLE dbo.TAIKHOAN;
GO

CREATE TABLE TAIKHOAN (
    TENDANGNHAP VARCHAR(20) PRIMARY KEY, 
    
    MATKHAU     CHAR(32) NOT NULL,       -- Hash MD5 (Giữ nguyên CHAR vì độ dài cố định)
    
    -- SỬA Ở ĐÂY: Chuyển thành VARCHAR
    SALT        VARCHAR(36) NOT NULL,    -- Salt (Linh hoạt hơn CHAR, dù GUID thường là 36)
    
    TRANGTHAI   BIT DEFAULT 1,           
    
    MANV        INTEGER NULL,
    MAKH        INTEGER NULL,

    CONSTRAINT FK_TAIKHOAN_NHANVIEN FOREIGN KEY (MANV) REFERENCES NHANVIEN(MANV),
    CONSTRAINT FK_TAIKHOAN_KHACHHANG FOREIGN KEY (MAKH) REFERENCES KHACHHANG(MAKH),
    
    CONSTRAINT CHK_ValidOwner CHECK (
        (MANV IS NOT NULL AND MAKH IS NULL) OR 
        (MANV IS NULL AND MAKH IS NOT NULL)
    )
);
GO

--mat khau 123456
DECLARE @DefaultPassHash CHAR(32) = 'fb276fb0ed6cdd1639bd678d3ace8614'; 
DECLARE @DefaultSalt CHAR(3) = 'ABC'; 
insert taikhoan (TENDANGNHAP,MATKHAU,salt,trangthai,MAKH)
values 
('taikhoankhachhang', @DefaultPassHash,@DefaultSalt,1,1)

insert taikhoan (TENDANGNHAP,MATKHAU,salt,trangthai,MANV)
values 
('taikhoanbacsi', @DefaultPassHash,@DefaultSalt,1,2),
('taikhoannhanvien', @DefaultPassHash,@DefaultSalt,1,3),
('taikhoanquanli', @DefaultPassHash,@DefaultSalt,1,1)
select * from taikhoan
IF NOT EXISTS (SELECT 1 FROM LAMVIEC WHERE MANV = 1)
BEGIN
    INSERT INTO LAMVIEC (MACN, MANV, NGAYBATDAU, NGAYKETTHUC, VAITRO)
    VALUES (1, 1, '2023-01-01', NULL, 'QL'); -- NULL nghĩa là làm vô thời hạn
END

-- Cho Bác sĩ (MANV = 2)
IF NOT EXISTS (SELECT 1 FROM LAMVIEC WHERE MANV = 2)
BEGIN
    INSERT INTO LAMVIEC (MACN, MANV, NGAYBATDAU, NGAYKETTHUC, VAITRO)
    VALUES (1, 2, '2023-01-01', NULL, 'BS');
END

-- Cho Nhân viên (MANV = 3) -> Tài khoản bạn đang test có thể là cái này
IF NOT EXISTS (SELECT 1 FROM LAMVIEC WHERE MANV = 3)
BEGIN
    INSERT INTO LAMVIEC (MACN, MANV, NGAYBATDAU, NGAYKETTHUC, VAITRO)
    VALUES (1, 3, '2023-01-01', NULL, 'NV');
END
GO

-- 3. Kiểm tra lại dữ liệu đã vào chưa
SELECT * FROM LAMVIEC;
-- co so 1
select * from NHANVIEN 
select * from BACSI
select * from QUANLY
select * from KHACHHANG where makh = 1
select * from taikhoan 
select * from CAKHAMBENH
select * from LAMVIEC where manv = 1
select * from LAMVIEC where manv = 5
select * from LAMVIEC where manv = 4


