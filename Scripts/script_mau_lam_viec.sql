USE PetcareX;
GO

-- Xóa dữ liệu cũ trong LAMVIEC để tránh trùng lặp khóa chính khi chạy lại
DELETE FROM LAMVIEC;
GO

-- PHÂN CÔNG BÁC SĨ VÀO CÁC CHI NHÁNH
-- Mỗi chi nhánh sẽ có từ 2-4 bác sĩ để bạn dễ dàng kiểm tra chức năng lọc

-- Chi nhánh 1: Petcare cơ sở 9
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(1, 1, '2023-01-01', NULL, 'BS'),
(1, 21, '2023-01-01', NULL, 'BS'),
(1, 37, '2023-01-01', NULL, 'BS');

-- Chi nhánh 2: Petcare cơ sở 5
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(2, 45, '2023-01-01', NULL, 'BS'),
(2, 52, '2023-01-01', NULL, 'BS'),
(2, 60, '2023-01-01', NULL, 'BS');

-- Chi nhánh 3: Petcare cơ sở 3
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(3, 2, '2023-01-01', NULL, 'BS'),
(3, 31, '2023-01-01', NULL, 'BS');

-- Chi nhánh 4: Petcare cơ sở 10
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(4, 10, '2023-01-01', NULL, 'BS'),
(4, 14, '2023-01-01', NULL, 'BS'),
(4, 16, '2023-01-01', NULL, 'BS'),
(4, 18, '2023-01-01', NULL, 'BS');

-- Chi nhánh 5: Petcare cơ sở 1
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(5, 25, '2023-01-01', NULL, 'BS'),
(5, 55, '2023-01-01', NULL, 'BS');

-- Chi nhánh 6: Petcare cơ sở 8
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(6, 6, '2023-01-01', NULL, 'BS'),
(6, 36, '2023-01-01', NULL, 'BS'),
(6, 39, '2023-01-01', NULL, 'BS');

-- Chi nhánh 7: Petcare cơ sở 2
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(7, 17, '2023-01-01', NULL, 'BS'),
(7, 34, '2023-01-01', NULL, 'BS'),
(7, 56, '2023-01-01', NULL, 'BS');

-- Chi nhánh 8: Petcare cơ sở 4
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(8, 4, '2023-01-01', NULL, 'BS'),
(8, 5, '2023-01-01', NULL, 'BS'),
(8, 22, '2023-01-01', NULL, 'BS'),
(8, 48, '2023-01-01', NULL, 'BS');

-- Chi nhánh 9: Petcare cơ sở 7
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(9, 12, '2023-01-01', NULL, 'BS'),
(9, 30, '2023-01-01', NULL, 'BS'),
(9, 40, '2023-01-01', NULL, 'BS'),
(9, 47, '2023-01-01', NULL, 'BS');

-- Chi nhánh 10: Petcare cơ sở 6
INSERT [dbo].[LAMVIEC] ([MACN], [MANV], [NGAYBATDAU], [NGAYKETTHUC], [VAITRO]) VALUES 
(10, 11, '2023-01-01', NULL, 'BS'),
(10, 28, '2023-01-01', NULL, 'BS'),
(10, 29, '2023-01-01', NULL, 'BS'),
(10, 53, '2023-01-01', NULL, 'BS');

GO
PRINT N'Đã cập nhật bảng LAMVIEC thành công cho Bác sĩ!';