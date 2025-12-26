USE PetcareX;
GO

-- 1. XÓA DỮ LIỆU CŨ ĐỂ LÀM SẠCH (TRÁNH TRÙNG LẶP KHÓA CHÍNH)
DELETE FROM TAIKHOAN WHERE TENDANGNHAP IN ('taikhoanquanli', 'taikhoanbacsi', 'taikhoannhanvien', 'taikhoankhachhang');
DELETE FROM LAMVIEC WHERE MANV IN (1, 2, 3);
GO

-- 2. CẬP NHẬT LẠI VAI TRÒ TRONG BẢNG NHANVIEN CHO ĐÚNG LOGIC TEST
-- Chúng ta chọn: MANV 1 làm Quản lý, MANV 2 làm Bác sĩ, MANV 3 làm Nhân viên
UPDATE NHANVIEN SET VAITRO = 'QL' WHERE MANV = 1; -- Ấu Nhung Lệ -> Quản lý
UPDATE NHANVIEN SET VAITRO = 'BS' WHERE MANV = 2; -- Đường Chiến Hòa -> Bác sĩ
UPDATE NHANVIEN SET VAITRO = 'NV' WHERE MANV = 3; -- Doãn Tuyết Tuyền -> Nhân viên
GO

-- 3. CẤP TÀI KHOẢN (SCRIPT TAIKHOAN)
DECLARE @PassHash CHAR(32) = 'fb276fb0ed6cdd1639bd678d3ace8614'; -- mật khẩu 123456
DECLARE @Salt CHAR(3) = 'ABC';

INSERT INTO TAIKHOAN (TENDANGNHAP, MATKHAU, SALT, TRANGTHAI, MANV, MAKH)
VALUES 
('taikhoanquanli',   @PassHash, @Salt, 1, 1,    NULL), -- MANV 1
('taikhoanbacsi',    @PassHash, @Salt, 1, 2,    NULL), -- MANV 2
('taikhoannhanvien', @PassHash, @Salt, 1, 3,    NULL), -- MANV 3
('taikhoankhachhang', @PassHash, @Salt, 1, NULL, 1);    -- MAKH 1
GO

-- 4. PHÂN CÔNG CÔNG TÁC (SCRIPT LAMVIEC) - Đảm bảo VAITRO khớp với bảng NHANVIEN và TAIKHOAN
-- Giả sử tất cả làm việc tại Chi nhánh 1 (MACN = 1)
INSERT INTO LAMVIEC (MACN, MANV, NGAYBATDAU, NGAYKETTHUC, VAITRO)
VALUES 
(1, 1, '2023-01-01', NULL, 'QL'), -- Khớp với taikhoanquanli
(1, 2, '2023-01-01', NULL, 'BS'), -- Khớp với taikhoanbacsi
(1, 3, '2023-01-01', NULL, 'NV'); -- Khớp với taikhoannhanvien
GO

-- 5. KIỂM TRA LẠI SỰ ĐỒNG BỘ
PRINT '--- KIỂM TRA ĐỒNG BỘ ---';
SELECT 
    TK.TENDANGNHAP, 
    NV.HOTEN, 
    NV.VAITRO AS VAITRO_GOC, 
    LV.VAITRO AS VAITRO_LAMVIEC,
    LV.MACN
FROM TAIKHOAN TK
JOIN NHANVIEN NV ON TK.MANV = NV.MANV
JOIN LAMVIEC LV ON NV.MANV = LV.MANV
WHERE TK.TENDANGNHAP != 'taikhoankhachhang';