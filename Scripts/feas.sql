USE PetcareX;
GO

/* Bảng cho chi tiết hoá đơn (khi dùng tham số bảng)
CREATE TYPE dbo.HoaDonChiTietType AS TABLE
(
    MASP    CHAR(5) NOT NULL,
    SOLUONG INT     NOT NULL
);
GO
*/

-- Procedure đặt lịch hẹn mới
CREATE OR ALTER PROCEDURE sp_DatLichHen
(
    @NgayHen    DATE,
    @ThoiGian   TIME,
    @NoiDung    NVARCHAR(200) = NULL,
    @MaKH       INT,
    @MaCN       INT,
    @MaLoaiDV   INT,
    @MaLichHen  INT OUTPUT     
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        -- Kiểm tra các tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM LOAIDICHVU WHERE MALOAIDV = @MaLoaiDV)
            RAISERROR(N'Loại dịch vụ không tồn tại', 16, 1);

        IF NOT EXISTS (
            SELECT 1 FROM CUNGCAP
            WHERE MACN = @MaCN AND MALOAIDV = @MaLoaiDV
        )
            RAISERROR(N'Dịch vụ không được cung cấp tại chi nhánh này', 16, 1);

        -- Kiểm tra trùng slot
        IF EXISTS (
            SELECT 1
            FROM LICHHEN
            WHERE NGAYHEN = @NgayHen
              AND THOIGIAN = @ThoiGian
              AND MACN = @MaCN
              AND MAKH = @MaKH
        )
            RAISERROR(N'Đã tồn tại lịch hẹn trùng thời gian với khách hàng tại chi nhánh này', 16, 1);

        INSERT INTO LICHHEN (NGAYHEN, THOIGIAN, NOIDUNG, MAKH, MACN, MALOAIDV)
        VALUES (@NgayHen, @ThoiGian, @NoiDung, @MaKH, @MaCN, @MaLoaiDV);

        SET @MaLichHen = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();

        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO

-- Procedure cập nhật lịch hẹn
CREATE OR ALTER PROCEDURE sp_CapNhatLichHen
(
    @MaLichHen   INT,
    @NgayHenMoi  DATE,
    @ThoiGianMoi TIME,
    @NoiDungMoi  NVARCHAR(200) = NULL,
    @MaCNMoi     INT,
    @MaLoaiDVMoi INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @MaKH INT;

    BEGIN TRY
        -- Lấy khách của lịch
        SELECT @MaKH = MAKH
        FROM LICHHEN
        WHERE MALICHHEN = @MaLichHen;

        IF @MaKH IS NULL
            RAISERROR(N'Không tìm thấy lịch hẹn', 16, 1);

        -- Kiểm tra tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCNMoi)
            RAISERROR(N'Chi nhánh mới không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM LOAIDICHVU WHERE MALOAIDV = @MaLoaiDVMoi)
            RAISERROR(N'Loại dịch vụ mới không tồn tại', 16, 1);

        IF NOT EXISTS (
            SELECT 1
            FROM CUNGCAP
            WHERE MACN = @MaCNMoi AND MALOAIDV = @MaLoaiDVMoi
        )
            RAISERROR(N'Dịch vụ mới không được cung cấp tại chi nhánh này', 16, 1);

        -- Kiểm tra trùng slot với lịch khác
        IF EXISTS (
            SELECT 1
            FROM LICHHEN
            WHERE NGAYHEN = @NgayHenMoi
              AND THOIGIAN = @ThoiGianMoi
              AND MACN = @MaCNMoi
              AND MAKH = @MaKH
              AND MALICHHEN <> @MaLichHen
        )
            RAISERROR(N'Đã có lịch hẹn khác trùng slot', 16, 1);

        -- Cập nhật
        UPDATE LICHHEN
        SET NGAYHEN  = @NgayHenMoi,
            THOIGIAN = @ThoiGianMoi,
            NOIDUNG  = @NoiDungMoi,
            MACN     = @MaCNMoi,
            MALOAIDV = @MaLoaiDVMoi
        WHERE MALICHHEN = @MaLichHen;
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();

        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO
-- PRocedure tạo hóa đơn
CREATE OR ALTER PROCEDURE sp_TaoHoaDonMoi
(
    @NgayLap   DATETIME,
    @MaKH      INT,
    @MaCN      INT,
    @MaNV      INT,
    @MaHD      INT OUTPUT
)       
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @KhuyenMai INT;

    BEGIN TRY
        -- Kiểm tra input
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM CHINHANH WHERE MACN = @MaCN)
            RAISERROR(N'Chi nhánh không tồn tại', 16, 1);

        IF NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = @MaNV)
            RAISERROR(N'Nhân viên không tồn tại', 16, 1);

        -- Nhân viên phải làm tại chi nhánh vào ngày lập hóa đơn
        IF NOT EXISTS (
            SELECT 1
            FROM LAMVIEC
            WHERE MANV = @MaNV
              AND MACN = @MaCN
              AND NGAYBATDAU <= CONVERT(DATE, @NgayLap)
              AND (NGAYKETTHUC IS NULL OR NGAYKETTHUC >= CONVERT(DATE, @NgayLap))
        )
            RAISERROR(N'Nhân viên không làm tại chi nhánh này vào ngày lập hóa đơn', 16, 1);

        
        -- Lấy khuyến mãi theo CẤP BẬC khách hàng
        SELECT @KhuyenMai =
            CASE KH.TENCAPBAC
                WHEN N'Cơ bản'    THEN 0
                WHEN N'Thân thiết' THEN 10
                WHEN N'VIP'       THEN 30
                ELSE 0
            END
        FROM KHACHHANG KH
        WHERE KH.MAKH = @MaKH;

        IF @KhuyenMai IS NULL
            RAISERROR(N'Không xác định được cấp bậc khách hàng', 16, 1);

        -- Tao hoa don moi
        INSERT INTO HOADON (NGAYLAP, KHUYENMAI, TONGTIEN, MACN, MAKH, TRANGTHAI, MANV)
        VALUES (@NgayLap, @KhuyenMai, NULL, @MaCN, @MaKH, N'Chưa hoàn thành', @MaNV);

        SET @MaHD = SCOPE_IDENTITY();
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO

-- Procedure thêm chi tiết hoá đơn
CREATE OR ALTER PROCEDURE sp_ThemChiTietHoaDon
(
    @MaHD    INT,
    @MaSP    INT,
    @SoLuong INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        -- Kiểm tra tham số đầu vào
        IF NOT EXISTS (SELECT 1 FROM SANPHAM WHERE MASP = @MaSP)
            RAISERROR(N'Sản phẩm không tồn tại', 16, 1);

        IF @SoLuong <= 0
            RAISERROR(N'Số lượng phải lớn hơn 0', 16, 1);

        -- Hóa đơn phải còn trạng thái 'Chưa hoàn thành'
        IF NOT EXISTS (
            SELECT 1
            FROM HOADON
            WHERE MAHD = @MaHD
              AND TRANGTHAI = N'Chưa hoàn thành'
        )
            RAISERROR(N'Hóa đơn không tồn tại hoặc đã được thanh toán', 16, 1);

        -- Kiểm tra tồn kho + lấy đơn giá
        DECLARE @TonKho INT,
                @GiaBan INT;

        SELECT @TonKho = TONKHO,
               @GiaBan = DONGIA
        FROM SANPHAM
        WHERE MASP = @MaSP;

        IF @TonKho < @SoLuong
            RAISERROR(N'Tồn kho không đủ để đáp ứng yêu cầu', 16, 1);

        -- Thêm chi tiết hoá đơn, lưu GIABAN = DONGIA hiện tại
        INSERT INTO CHITIETHOADON (MAHD, MASP, SOLUONG, GIABAN)
        VALUES (@MaHD, @MaSP, @SoLuong, @GiaBan);

        -- Cập nhật tồn kho
        UPDATE SANPHAM
        SET TONKHO = TONKHO - @SoLuong
        WHERE MASP = @MaSP;

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO
-- Procedure thanh toan
CREATE OR ALTER PROCEDURE sp_ThanhToanHoaDon
(
    @MaHD INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;

        -- Hóa đơn phải tồn tại và còn 'Chưa hoàn thành'
        IF NOT EXISTS (
            SELECT 1
            FROM HOADON
            WHERE MAHD = @MaHD
              AND TRANGTHAI = N'Chưa hoàn thành'
        )
            RAISERROR(N'Hóa đơn không tồn tại hoặc đã được thanh toán', 16, 1);

        DECLARE @TongHang  DECIMAL(12, 2),
                @TongTien  DECIMAL(12, 2),
                @KhuyenMai INT,
                @MaKH      INT,
                @NgayLap   DATE,
                @Nam       SMALLINT;

        -- Tính tổng tiền hàng từ chi tiết hoá đơn
        SELECT @TongHang = SUM(CAST(GIABAN AS DECIMAL(12,2)) * SOLUONG)
        FROM CHITIETHOADON
        WHERE MAHD = @MaHD;

        IF @TongHang IS NULL
            RAISERROR(N'Hóa đơn chưa có chi tiết', 16, 1);

        -- Lấy thông tin hóa đơn
        SELECT  @KhuyenMai = ISNULL(KHUYENMAI, 0),
                @MaKH      = MAKH,
                @NgayLap   = CONVERT(DATE, NGAYLAP)
        FROM HOADON
        WHERE MAHD = @MaHD;

        -- Áp khuyến mãi
        SET @TongTien = @TongHang * (1 - @KhuyenMai / 100.0);

        IF @TongTien <= 0
            RAISERROR(N'Tổng tiền không hợp lệ', 16, 1);

        UPDATE HOADON
        SET TONGTIEN  = @TongTien,
            TRANGTHAI = N'Đã thanh toán'
        WHERE MAHD = @MaHD;

        -- Cập nhật CHITIEU 
        SET @Nam = YEAR(@NgayLap);

        IF EXISTS (SELECT 1 FROM CHITIEU WHERE MAKH = @MaKH AND NAM = @Nam)
        BEGIN
            UPDATE CHITIEU
            SET CHITIEU = CHITIEU + @TongTien
            WHERE MAKH = @MaKH AND NAM = @Nam;
        END
        ELSE
        BEGIN
            INSERT INTO CHITIEU (MAKH, NAM, CHITIEU)
            VALUES (@MaKH, @Nam, @TongTien);
        END

        COMMIT TRAN;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;

        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO

-- Procedure thêm đánh giá hoá đơn
CREATE OR ALTER PROCEDURE sp_ThemDanhGiaHoaDon
(
    @MaKH         INT,
    @MaHD         INT,
    @DiemDichVu   TINYINT,
    @MucDoHaiLong TINYINT,
    @BinhLuan     NVARCHAR(200) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        -- Khách hàng phải tồn tại
        IF NOT EXISTS (SELECT 1 FROM KHACHHANG WHERE MAKH = @MaKH)
            RAISERROR(N'Khách hàng không tồn tại', 16, 1);

        -- Hóa đơn phải tồn tại và đã thanh toán
        IF NOT EXISTS (
            SELECT 1
            FROM HOADON
            WHERE MAHD = @MaHD
              AND TRANGTHAI = N'Đã thanh toán'
        )
            RAISERROR(N'Hóa đơn không tồn tại hoặc chưa được thanh toán', 16, 1);

        -- Mỗi hóa đơn chỉ được đánh giá một lần
        IF EXISTS (SELECT 1 FROM DANHGIA WHERE MAHD = @MaHD)
            RAISERROR(N'Hóa đơn này đã được đánh giá', 16, 1);

        -- Điểm dịch vụ và mức độ hài lòng phải từ 1 đến 5
        IF @DiemDichVu NOT BETWEEN 1 AND 5
            RAISERROR(N'Điểm dịch vụ phải từ 1 đến 5', 16, 1);

        IF @MucDoHaiLong NOT BETWEEN 1 AND 5
            RAISERROR(N'Mức độ hài lòng phải từ 1 đến 5', 16, 1);

        -- Thêm đánh giá
        INSERT INTO DANHGIA (DIEMDICHVU, MUCDOHAILONG, BINHLUAN, MAKH, MAHD)
        VALUES (@DiemDichVu, @MucDoHaiLong, @BinhLuan, @MaKH, @MaHD);
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
                @ErrSeverity INT = ERROR_SEVERITY();

        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;
GO
