USE PetcareX;
GO

-- 1. XÁC ĐỊNH KHÁCH HÀNG CẦN BƠM DỮ LIỆU (ID = 1)
DECLARE @MaKH_Test INT = 1;

-- Đảm bảo Khách hàng số 1 tồn tại (Nếu chưa có thì tạo lại)
IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH_Test)
BEGIN
    SET IDENTITY_INSERT KHACHHANG ON;
    INSERT INTO KHACHHANG (MAKH, TENKH, SDT, DIACHI, TENCAPBAC, DIEMTICHLUY)
    VALUES (@MaKH_Test, N'Nguyễn Văn A (Test)', '0912345678', N'123 Lê Lợi, TP.HCM', N'VIP', 100);
    SET IDENTITY_INSERT KHACHHANG OFF;
END

-- 2. TẠO THÚ CƯNG CHO KHÁCH HÀNG NÀY
-- Xóa thú cưng cũ để tránh trùng lặp
DELETE FROM THUCUNG WHERE MAKH = @MaKH_Test;

INSERT INTO THUCUNG (MAKH, TENTC, LOAI, TUOI, GIOITINH, TINHTRANG)
VALUES 
(@MaKH_Test, N'Miu Miu', N'Mèo', 2, N'Cái', N'Bình thường'),
(@MaKH_Test, N'Ni Ni', N'Vẹt', 2, N'Cái', N'Bình thường'),
(@MaKH_Test, N'Lu Lu', N'Chó', 3, N'Đực', N'Đang điều trị');

-- Lấy ID của 2 thú cưng vừa tạo
DECLARE @MaTC1 INT, @MaTC2 INT;
SELECT TOP 1 @MaTC1 = MATC FROM THUCUNG WHERE TENTC = N'Miu Miu' AND MAKH = @MaKH_Test ORDER BY MATC DESC;
SELECT TOP 1 @MaTC2 = MATC FROM THUCUNG WHERE TENTC = N'Lu Lu' AND MAKH = @MaKH_Test ORDER BY MATC DESC;

-- 3. TẠO LỊCH SỬ KHÁM BỆNH (Cho Miu Miu)
INSERT INTO CAKHAMBENH (MATC, MANV, NGAYKHAM)
VALUES (@MaTC1, 2, DATEADD(DAY, -10, GETDATE())); -- Khám 10 ngày trước

DECLARE @MaKB1 INT = SCOPE_IDENTITY();

-- Thêm chẩn đoán và thuốc
INSERT INTO CHANDOAN (MAKB, TENCHANDOAN) VALUES (@MaKB1, N'Rối loạn tiêu hóa nhẹ');
INSERT INTO TRIEUCHUNG (MAKB, TENTRIEUCHUNG) VALUES (@MaKB1, N'Nôn mửa'), (@MaKB1, N'Bỏ ăn');
INSERT INTO TOATHUOC (MAKB, GHICHU) VALUES (@MaKB1, N'Cho ăn cháo loãng, uống thuốc đúng giờ');

-- 4. TẠO LỊCH SỬ TIÊM PHÒNG (Cho Lu Lu)
INSERT INTO CATIEM (MATC, MANV, NGAYTIEM)
VALUES (@MaTC2, 2, DATEADD(MONTH, -1, GETDATE())); -- Tiêm 1 tháng trước

DECLARE @MaTiem1 INT = SCOPE_IDENTITY();
-- Giả sử Vacxin ID 1 tồn tại (nếu chưa có script tạo SP thì có thể lỗi FK ở đây, nhưng dataset1 của bạn đã có SP)
-- Nếu bảng SanPham/Vacxin chưa có dữ liệu, đoạn này có thể cần check. 
-- Tạm thời insert mẫu chi tiết tiêm nếu có dữ liệu Vacxin
IF EXISTS (SELECT 1 FROM VACXIN)
BEGIN
    DECLARE @MaVX INT;
    SELECT TOP 1 @MaVX = MAVACXIN FROM VACXIN;
    INSERT INTO CHITIETCATIEM (MATIEM, MAVACXIN, SOLUONG) VALUES (@MaTiem1, @MaVX, 1);
END

-- 5. TẠO HÓA ĐƠN MUA HÀNG (Để hiện bên Tab Lịch sử hóa đơn)
-- Hóa đơn 1: Đã thanh toán
INSERT INTO HOADON (NGAYLAP, KHUYENMAI, TONGTIEN, MACN, MAKH, MANV, TRANGTHAI)
VALUES (DATEADD(DAY, -5, GETDATE()), 10, 550000, 1, @MaKH_Test, 2, N'Đã thanh toán');

DECLARE @MaHD1 INT = SCOPE_IDENTITY();

-- Thêm chi tiết hóa đơn (Lấy đại 2 sản phẩm đầu tiên trong kho)
INSERT INTO CHITIETHOADON (MAHD, MASP, SOLUONG, GIABAN)
SELECT TOP 2 @MaHD1, MASP, 1, DONGIA FROM SANPHAM;

-- Cập nhật tổng chi tiêu cho khách hàng (Quan trọng để hiện số liệu tổng)
IF EXISTS (SELECT 1 FROM CHITIEU WHERE MAKH = @MaKH_Test AND NAM = YEAR(GETDATE()))
    UPDATE CHITIEU SET CHITIEU = CHITIEU + 550000 WHERE MAKH = @MaKH_Test AND NAM = YEAR(GETDATE());
ELSE
    INSERT INTO CHITIEU (MAKH, NAM, CHITIEU) VALUES (@MaKH_Test, YEAR(GETDATE()), 550000);

PRINT N'=== ĐÃ BƠM DỮ LIỆU MẪU CHO KHÁCH HÀNG ID ' + CAST(@MaKH_Test AS NVARCHAR(20)) + ' THÀNH CÔNG ===';
select * from THUCUNG where makh = 1
EXEC sp_ThuCung_XemDanhSach @MAKH = 1;
GO
select * from nhanvien