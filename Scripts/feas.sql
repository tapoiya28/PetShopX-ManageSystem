USE PetcareX;
GO
-- Tạo kiểu bảng cho chi tiết hoá đơn(khi thêm tham số bảng)
CREATE TYPE dbo.HoaDonChiTietType AS TABLE
(
    MASP    CHAR(5) NOT NULL,
    SOLUONG INT     NOT NULL
);
GO
--- Procedure đặt lịch hẹn
CREATE OR ALTER PROCEDURE sp_DatLichHen
(
    @MaLichHen CHAR(12),
    @NgayHen   DATE,
    @ThoiGian  TIME,
    @NoiDung   NVARCHAR(200) = NULL,
    @MaKH      CHAR(12),
    @MaCN      CHAR(5),
    @MaLoaiDV  CHAR(5),
    @KQ        INT OUTPUT  -- 1 = ok, 0 = fail
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Auto rollback nếu có lỗi

    BEGIN TRY
        BEGIN TRAN;
        SET @KQ = 0;

        -- Kiểm tra tồn tại
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM LOAIDICHVU WHERE MALOAIDV = @MaLoaiDV)
            RAISERROR(N'Loại dịch vụ không tồn tại', 16, 1);

        IF NOT EXISTS (
            SELECT 1
            FROM CUNGCAP
            WHERE MACN = @MaCN AND MALOAIDV = @MaLoaiDV
        )
            RAISERROR(N'Dịch vụ không được cung cấp tại chi nhánh này', 16, 1);

        -- Kiểm tra trùng mã lịch
        IF EXISTS (SELECT 1 FROM LICHHEN WHERE MALICHHEN = @MaLichHen)
            RAISERROR(N'Mã lịch hẹn đã tồn tại', 16, 1);

        -- Kiểm tra trùng slot (ngày, giờ, chi nhánh, khách hàng)
        IF EXISTS (
            SELECT 1
            FROM LICHHEN
            WHERE NGAYHEN  = @NgayHen
              AND THOIGIAN = @ThoiGian
              AND MACN     = @MaCN
              AND MAKH     = @MaKH
        )
            RAISERROR(N'Đã tồn tại lịch hẹn trùng thời gian với khách hàng tại chi nhánh này', 16, 1);
        /* comment vì code trên mac chưa sửa lại địa chỉ partition
        IF EXISTS (
            SELECT 1
            FROM LICHHEN_partitioned
            WHERE NGAYHEN  = @NgayHen
              AND THOIGIAN = @ThoiGian
              AND MACN     = @MaCN
              AND MAKH     = @MaKH
        )
            RAISERROR(N'Đã tồn tại lịch hẹn trùng thời gian với khách hàng tại chi nhánh này', 16, 1);
        */
        -- Insert LICHHEN
        INSERT INTO LICHHEN (MALICHHEN, NGAYHEN, THOIGIAN, NOIDUNG, MAKH, MACN, MALOAIDV)
        VALUES (@MaLichHen, @NgayHen, @ThoiGian, @NoiDung, @MaKH, @MaCN, @MaLoaiDV);

        /* comment vì code trên mac chưa sửa lại địa chỉ partition 
        -- Insert LICHHEN_partitioned (trên partition scheme)
        INSERT INTO LICHHEN_partitioned (MALICHHEN, NGAYHEN, THOIGIAN, NOIDUNG, MAKH, MACN, MALOAIDV)
        VALUES (@MaLichHen, @NgayHen, @ThoiGian, @NoiDung, @MaKH, @MaCN, @MaLoaiDV);
        */
        SET @KQ = 1;
        COMMIT TRAN;
    END TRY
    -- Check các lỗi phát sinh
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO
--- Procedure cập nhật lịch hẹn
CREATE PROCEDURE sp_CapNhatLichHen
(
    @MaLichHen   CHAR(12),
    @NgayHenMoi  DATE,
    @ThoiGianMoi TIME,
    @NoiDungMoi  NVARCHAR(200) = NULL,
    @MaCNMoi     CHAR(5),
    @MaLoaiDVMoi CHAR(5),
    @KQ          INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @MaKH CHAR(12);

    BEGIN TRY
        BEGIN TRAN;
        SET @KQ = 0;
        
        -- Lấy thông tin hiện tại
        SELECT TOP 1
            @MaKH = MAKH
        FROM LICHHEN
        WHERE MALICHHEN = @MaLichHen;

        IF @MaKH IS NULL
            RAISERROR(N'Không tìm thấy lịch hẹn', 16, 1);

        -- Kiểm tra các tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCNMoi)
            RAISERROR(N'Chi nhánh mới không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM LOAIDICHVU WHERE MALOAIDV = @MaLoaiDVMoi)
            RAISERROR(N'Loại dịch vụ mới không tồn tại', 16, 1);

        IF NOT EXISTS (
            SELECT 1 FROM CUNGCAP
            WHERE MACN = @MaCNMoi AND MALOAIDV = @MaLoaiDVMoi
        )
            RAISERROR(N'Dịch vụ mới không được cung cấp tại chi nhánh', 16, 1);

        -- Kiểm tra trùng slot với lịch khác
        IF EXISTS (
            SELECT 1
            FROM LICHHEN
            WHERE NGAYHEN  = @NgayHenMoi
              AND THOIGIAN = @ThoiGianMoi
              AND MACN     = @MaCNMoi
              AND MAKH     = @MaKH
              AND MALICHHEN <> @MaLichHen
        )
            RAISERROR(N'Đã có lịch hẹn khác trùng slot', 16, 1);
        /* comment vì code trên mac chưa sửa lại địa chỉ partition
        IF EXISTS (
            SELECT 1
            FROM LICHHEN_partitioned
            WHERE NGAYHEN  = @NgayHenMoi
              AND THOIGIAN = @ThoiGianMoi
              AND MACN     = @MaCNMoi
              AND MAKH     = @MaKH
              AND MALICHHEN <> @MaLichHen
        )
            RAISERROR(N'Đã có lịch hẹn khác trùng slot', 16, 1);
        */

        -- Cập nhật LICHHEN
        UPDATE LICHHEN
        SET NGAYHEN  = @NgayHenMoi,
            THOIGIAN = @ThoiGianMoi,
            NOIDUNG  = @NoiDungMoi,
            MACN     = @MaCNMoi,
            MALOAIDV = @MaLoaiDVMoi
        WHERE MALICHHEN = @MaLichHen;

        /* comment vì code trên mac chưa sửa lại địa chỉ partition
        -- Cập nhật LICHHEN_partitioned
        UPDATE LICHHEN_partitioned
        SET NGAYHEN  = @NgayHenMoi,
            THOIGIAN = @ThoiGianMoi,
            NOIDUNG  = @NoiDungMoi,
            MACN     = @MaCNMoi,
            MALOAIDV = @MaLoaiDVMoi
        WHERE MALICHHEN = @MaLichHen;
        */

        SET @KQ = 1;
        COMMIT TRAN;
    END TRY
    -- Check các lỗi phát sinh
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO
--- Procedure lập hoá đơn và thanh toán
CREATE PROCEDURE sp_LapHoaDonVaThanhToan
(
    @MaHD      CHAR(12),
    @NgayLap   DATETIME,
    @MaKH      CHAR(12),
    @MaCN      CHAR(5),
    @MaNV      CHAR(5),
    @KhuyenMai INT = 0,
    @ChiTiet   dbo.HoaDonChiTietType READONLY, -- Bảng chi tiết hoá đơn
    @KQ        INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @TongTien DECIMAL(12,2);
    DECLARE @Nam SMALLINT, @Thang TINYINT;

    BEGIN TRY
        BEGIN TRAN;
        SET @KQ = 0;

        -- Kiểm tra các tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MaNV)
            RAISERROR(N'Nhân viên không tồn tại', 16, 1);

        -- Kiểm tra nhân viên có làm ở chi nhánh vào ngày lập không
        IF NOT EXISTS (
            SELECT 1 FROM LAMVIEC
            WHERE MANV = @MaNV
              AND MACN = @MaCN
              AND NGAYBATDAU <= CONVERT(DATE, @NgayLap)
              AND (NGAYKETTHUC IS NULL OR NGAYKETTHUC >= CONVERT(DATE, @NgayLap))
        )
            RAISERROR(N'Nhân viên không làm tại chi nhánh này vào ngày lập hóa đơn', 16, 1);

        -- Hoá đơn phải có ít nhất 1 dòng
        IF NOT EXISTS (SELECT 1 FROM @ChiTiet)
            RAISERROR(N'Hoá đơn phải có ít nhất một sản phẩm', 16, 1);

        -- Kiểm tra sản phẩm hợp lệ
        IF EXISTS (
            SELECT 1
            FROM @ChiTiet ct
            LEFT JOIN SANPHAM sp ON sp.MASP = ct.MASP
            WHERE sp.MASP IS NULL
        )
            RAISERROR(N'Tồn tại sản phẩm không hợp lệ trong hoá đơn', 16, 1);

        -- Kiểm tra tồn kho
        IF EXISTS (
            SELECT 1
            FROM @ChiTiet ct
            JOIN SANPHAM sp ON sp.MASP = ct.MASP
            GROUP BY ct.MASP, sp.TONKHO
            HAVING SUM(ct.SOLUONG) > sp.TONKHO
        )
            RAISERROR(N'Số lượng mua vượt quá tồn kho cho một số sản phẩm', 16, 1);

        -- Tính tổng tiền
        SELECT @TongTien = SUM(ct.SOLUONG * sp.DONGIA)
        FROM @ChiTiet ct
        JOIN SANPHAM sp ON sp.MASP = ct.MASP;

        SET @TongTien = @TongTien - ISNULL(@KhuyenMai, 0);

        IF @TongTien <= 0
            RAISERROR(N'Tổng tiền sau khuyến mãi phải > 0', 16, 1);

        -- Insert HOADON
        INSERT INTO HOADON (MAHD, NGAYLAP, KHUYENMAI, TONGTIEN, MACN, MAKH, MANV)
        VALUES (@MaHD, @NgayLap, @KhuyenMai, @TongTien, @MaCN, @MaKH, @MaNV);

        /* comment vì code trên mac chưa sửa lại địa chỉ partition
        -- Insert HOADON_partitioned (trên partition)
        INSERT INTO HOADON_partitioned (MAHD, NGAYLAP, KHUYENMAI, TONGTIEN, MACN, MAKH, MANV)
        VALUES (@MaHD, @NgayLap, @KhuyenMai, @TongTien, @MaCN, @MaKH, @MaNV);
        */

        -- Insert chi tiết hoá đơn
        INSERT INTO CHITIETHOADON (MAHD, MASP, SOLUONG)
        SELECT @MaHD, MASP, SOLUONG
        FROM @ChiTiet;

        -- Cập nhật tồn kho
        UPDATE sp
        SET TONKHO = sp.TONKHO - src.SL
        FROM SANPHAM sp
        JOIN (
            SELECT MASP, SUM(SOLUONG) AS SL
            FROM @ChiTiet
            GROUP BY MASP
        ) src ON sp.MASP = src.MASP;

        -- Cập nhật CHITIEU
        SET @Nam   = YEAR(@NgayLap);
        SET @Thang = MONTH(@NgayLap);
        -- Nếu đã có mục tiêu trong tháng thì cộng dồn, nếu chưa thì thêm mới
        IF EXISTS (SELECT 1 FROM CHITIEU WHERE MAKH = @MaKH AND NAM = @Nam AND THANG = @Thang)
        BEGIN
            UPDATE CHITIEU
            SET CHITIEU = CHITIEU + @TongTien
            WHERE MAKH = @MaKH AND NAM = @Nam AND THANG = @Thang;
        END
        ELSE
        BEGIN
            INSERT INTO CHITIEU (MAKH, NAM, THANG, CHITIEU)
            VALUES (@MaKH, @Nam, @Thang, @TongTien);
        END

        SET @KQ = 1;
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO

--- Procedure thêm đánh giá hoá đơn
CREATE PROCEDURE sp_ThemDanhGiaHoaDon
(
    @MaDanhGia     CHAR(12),
    @MaHD          CHAR(12),
    @MaKH          CHAR(12),
    @DiemDichVu    TINYINT,
    @MucDoHaiLong  TINYINT,
    @BinhLuan      NVARCHAR(200) = NULL,
    @KQ            INT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;
        SET @KQ = 0;

        -- Kiểm tra các tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        -- Hóa đơn phải tồn tại và thuộc về khách này
        IF NOT EXISTS (
            SELECT 1
            FROM HOADON
            WHERE MAHD = @MaHD AND MAKH = @MaKH
        )
            RAISERROR(N'Hóa đơn không tồn tại hoặc không thuộc về khách hàng này.', 16, 1);

        -- Mỗi hóa đơn chỉ được đánh giá 1 lần
        IF EXISTS (SELECT 1 FROM DANHGIA WHERE MAHD = @MaHD)
            RAISERROR(N'Hóa đơn này đã được đánh giá', 16, 1);
        -- Ràng buộc điểm dịch vụ và mức độ hài lòng từ 1 đến 5
        IF @DiemDichVu NOT BETWEEN 1 AND 5
            RAISERROR(N'Điểm dịch vụ phải từ 1 đến 5', 16, 1);

        IF @MucDoHaiLong NOT BETWEEN 1 AND 5
            RAISERROR(N'Mức độ hài lòng phải từ 1 đến 5', 16, 1);

        INSERT INTO DANHGIA (MADANHGIA, DIEMDICHVU, MUCDOHAILONG, BINHLUAN, MAKH, MAHD)
        VALUES (@MaDanhGia, @DiemDichVu, @MucDoHaiLong, @BinhLuan, @MaKH, @MaHD);

        SET @KQ = 1;
        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO

/*Exec (chưa có sample data)
-- Đặt lịch hẹn
DECLARE @KQ INT;

EXEC sp_DatLichHen
    @MaLichHen = 'LH0000000001',
    @NgayHen   = '2025-12-01',
    @ThoiGian  = '09:00',
    @NoiDung   = N'Khám sức khoẻ định kỳ',
    @MaKH      = 'KH0000000001',
    @MaCN      = 'CN001',
    @MaLoaiDV  = 'LDV01',
    @KQ        = @KQ OUTPUT;

SELECT @KQ AS KetQua_DatLich;
-- 1 = thành công, 0 = thất bại (và sẽ có message lỗi trả về)


DECLARE @KQ INT;

EXEC sp_CapNhatLichHen
    @MaLichHen   = 'LH0000000001',
    @NgayHenMoi  = '2025-12-02',
    @ThoiGianMoi = '10:30',
    @NoiDungMoi  = N'Đổi giờ khám sang buổi trưa',
    @MaCNMoi     = 'CN001',   -- có thể giữ nguyên chi nhánh
    @MaLoaiDVMoi = 'LDV01',   -- hoặc đổi sang loại DV khác nếu muốn
    @KQ          = @KQ OUTPUT;

SELECT @KQ AS KetQua_CapNhatLich;
GO

--- Lập hoá đơn và thanh toán
-- Bước 1: Tạo biến bảng chi tiết hoá đơn
DECLARE @CT dbo.HoaDonChiTietType;

INSERT INTO @CT (MASP, SOLUONG)
VALUES ('SP001', 2),      -- mua 2 sản phẩm SP001
       ('SP002', 1);      -- mua 1 sản phẩm SP002

-- Bước 2: Gọi procedure
DECLARE @KQ INT;

EXEC sp_LapHoaDonVaThanhToan
    @MaHD      = 'HD0000000001',
    @NgayLap   = '2025-12-01T10:00:00',
    @MaKH      = 'KH0000000001',
    @MaCN      = 'CN001',
    @MaNV      = 'NV001',
    @KhuyenMai = 0,
    @ChiTiet   = @CT,
    @KQ        = @KQ OUTPUT;

SELECT @KQ AS KetQua_LapHoaDon;

-- Xem lại dữ liệu:
SELECT * FROM HOADON WHERE MAHD = 'HD0000000001';
SELECT * FROM CHITIETHOADON WHERE MAHD = 'HD0000000001';
SELECT * FROM SANPHAM WHERE MASP IN ('SP001', 'SP002'); -- kiểm tra TONKHO đã trừ
SELECT * FROM CHITIEU WHERE MAKH = 'KH0000000001';
GO

-- Thêm đánh giá hoá đơn
DECLARE @KQ INT;

EXEC sp_ThemDanhGiaHoaDon
    @MaDanhGia     = 'DG0000000001',
    @MaHD          = 'HD0000000001',
    @MaKH          = 'KH0000000001',
    @DiemDichVu    = 5,
    @MucDoHaiLong  = 5,
    @BinhLuan      = N'Dịch vụ rất tốt, nhân viên nhiệt tình',
    @KQ            = @KQ OUTPUT;

SELECT @KQ AS KetQua_DanhGia;

SELECT * FROM DANHGIA WHERE MAHD = 'HD0000000001';

*/