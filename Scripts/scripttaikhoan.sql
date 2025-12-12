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
('taikhoankhachhang', @DefaultPassHash,@DefaultSalt,1,302129)

insert taikhoan (TENDANGNHAP,MATKHAU,salt,trangthai,MANV)
values 
('taikhoanbacsi', @DefaultPassHash,@DefaultSalt,1,924802),
('taikhoannhanvien', @DefaultPassHash,@DefaultSalt,1,924805),
('taikhoanquanli', @DefaultPassHash,@DefaultSalt,1,924803)
-- co so 1
INSERT LAMVIEC(MACN, MANV, NGAYBATDAU, NGAYKETTHUC, VAITRO) VALUES (5112203, 924805, '2020-04-30', '2028-04-26', N'NV')
INSERT LAMVIEC(MACN, MANV, NGAYBATDAU, NGAYKETTHUC, VAITRO) VALUES (5112203, 924802, '2020-04-30', '2028-04-26', N'BS')
INSERT LAMVIEC(MACN, MANV, NGAYBATDAU, NGAYKETTHUC, VAITRO) VALUES (5112203, 924803, '2020-04-30', '2028-04-26', N'QL')
select * from NHANVIEN 
select * from BACSI
select * from QUANLY
select * from KHACHHANG
select * from taikhoan 
select * from CAKHAMBENH


