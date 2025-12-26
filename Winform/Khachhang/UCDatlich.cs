using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    public class UCDatLich : UserControl
    {
        // Sự kiện để báo cho Form cha (Dashboard) biết muốn quay lại
        public event EventHandler? QuayLaiTrangChu;

        // Các controls giao diện
        private ComboBox cboChiNhanh;
        private ComboBox cboDichVu;
        private DateTimePicker dtpNgay;
        private DateTimePicker dtpGio;
        private TextBox txtGhiChu;
        private Button btnDatLich;
        private Button btnBack;

        // Màu sắc chủ đạo
        private Color primaryColor = Color.FromArgb(51, 102, 255); // Xanh dương
        private Color successColor = Color.FromArgb(40, 167, 69);  // Xanh lá

        public UCDatLich()
        {
            // 1. Cài đặt UserControl
            this.Size = new Size(1000, 700); // Kích thước lớn để thoáng
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill; // Tự động lấp đầy Panel cha

            // 2. Khởi tạo Controls (tránh lỗi null reference)
            cboChiNhanh = new ComboBox();
            cboDichVu = new ComboBox();
            dtpNgay = new DateTimePicker();
            dtpGio = new DateTimePicker();
            txtGhiChu = new TextBox();
            btnDatLich = new Button();
            btnBack = new Button();

            // 3. Vẽ giao diện và Tải dữ liệu
            TaoGiaoDien();
            LoadDuLieuComboBox();
        }

        private void TaoGiaoDien()
        {
            // --- TIÊU ĐỀ ---
            Label lblTitle = new Label() { 
                Text = "ĐẶT LỊCH HẸN MỚI", 
                Font = new Font("Segoe UI", 22, FontStyle.Bold), 
                ForeColor = primaryColor, 
                Location = new Point(50, 30), 
                AutoSize = true 
            };

            Label lblSubTitle = new Label() {
                Text = "Vui lòng điền thông tin bên dưới để chúng tôi chuẩn bị đón tiếp bạn tốt nhất.",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(55, 75),
                AutoSize = true
            };
            btnThemTrucTiep = new Button() { 
                Text = "➕ ĐẶT LỊCH TRỰC TIẾP", 
                Location = new Point(410, 70), // Chỉnh lại tọa độ cho phù hợp
                Width = 200, Height = 30,
                BackColor = Color.Orange, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            // --- GROUP 1: THÔNG TIN DỊCH VỤ ---
            int startY = 130;
            int gap = 80; // Khoảng cách giữa các dòng

            // 1. Chọn Chi Nhánh
            Label lblCN = CreateLabel("Chọn Chi Nhánh:", 50, startY);
            cboChiNhanh.Location = new Point(50, startY + 30);
            cboChiNhanh.Width = 400;
            cboChiNhanh.Height = 35;
            cboChiNhanh.Font = new Font("Segoe UI", 11);
            cboChiNhanh.DropDownStyle = ComboBoxStyle.DropDownList; // Chỉ chọn, không nhập

            // 2. Chọn Dịch Vụ
            Label lblDV = CreateLabel("Loại Dịch Vụ:", 500, startY);
            cboDichVu.Location = new Point(500, startY + 30);
            cboDichVu.Width = 400;
            cboDichVu.Height = 35;
            cboDichVu.Font = new Font("Segoe UI", 11);
            cboDichVu.DropDownStyle = ComboBoxStyle.DropDownList;

            // --- GROUP 2: THỜI GIAN ---
            int line2Y = startY + gap;

            // 3. Ngày Hẹn
            Label lblNgay = CreateLabel("Ngày Hẹn:", 50, line2Y);
            dtpNgay.Location = new Point(50, line2Y + 30);
            dtpNgay.Width = 250;
            dtpNgay.Height = 35;
            dtpNgay.Font = new Font("Segoe UI", 11);
            dtpNgay.Format = DateTimePickerFormat.Short;
            dtpNgay.MinDate = DateTime.Now; // Không cho chọn quá khứ

            // 4. Giờ Hẹn
            Label lblGio = CreateLabel("Giờ Hẹn (08:00 - 20:00):", 350, line2Y);
            dtpGio.Location = new Point(350, line2Y + 30);
            dtpGio.Width = 150;
            dtpGio.Height = 35;
            dtpGio.Font = new Font("Segoe UI", 11);
            dtpGio.Format = DateTimePickerFormat.Custom;
            dtpGio.CustomFormat = "HH:mm";
            dtpGio.ShowUpDown = true; // Dùng nút lên xuống để chỉnh giờ

            // --- GROUP 3: GHI CHÚ ---
            int line3Y = line2Y + gap;
            Label lblNote = CreateLabel("Ghi chú thêm (Triệu chứng, Yêu cầu bác sĩ...):", 50, line3Y);
            txtGhiChu.Location = new Point(50, line3Y + 30);
            txtGhiChu.Width = 850; // Rộng hết form
            txtGhiChu.Height = 100;
            txtGhiChu.Font = new Font("Segoe UI", 11);
            txtGhiChu.Multiline = true; // Cho phép nhập nhiều dòng
            txtGhiChu.BackColor = Color.WhiteSmoke;

            // --- BUTTONS ---
            int btnY = line3Y + 160;

            btnDatLich.Text = "XÁC NHẬN ĐẶT LỊCH";
            btnDatLich.Location = new Point(50, btnY);
            btnDatLich.Size = new Size(250, 50);
            btnDatLich.BackColor = successColor;
            btnDatLich.ForeColor = Color.White;
            btnDatLich.FlatStyle = FlatStyle.Flat;
            btnDatLich.FlatAppearance.BorderSize = 0;
            btnDatLich.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnDatLich.Cursor = Cursors.Hand;
            btnDatLich.Click += (s, e) => XuLyDatLich();

            btnBack.Text = "Hủy / Quay lại";
            btnBack.Location = new Point(330, btnY);
            btnBack.Size = new Size(180, 50);
            btnBack.BackColor = Color.Gainsboro;
            btnBack.ForeColor = Color.Black;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Font = new Font("Segoe UI", 11);
            btnBack.Cursor = Cursors.Hand;
            btnBack.Click += (s, e) => QuayLaiTrangChu?.Invoke(this, EventArgs.Empty);

            // Thêm tất cả vào UserControl
            this.Controls.AddRange(new Control[] { 
                lblTitle, lblSubTitle, 
                lblCN, cboChiNhanh, 
                lblDV, cboDichVu, 
                lblNgay, dtpNgay, 
                lblGio, dtpGio, 
                lblNote, txtGhiChu, 
                btnDatLich, btnBack 
            });
        }

        // Hàm hỗ trợ tạo Label nhanh
        private Label CreateLabel(string text, int x, int y)
        {
            return new Label() {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DimGray
            };
        }

        // Tải dữ liệu vào ComboBox (Branch & Service)
        private void LoadDuLieuComboBox()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    // 1. Load Chi Nhánh
                    SqlDataAdapter daCN = new SqlDataAdapter("SELECT MACN, TENCN FROM CHINHANH", conn);
                    DataTable dtCN = new DataTable();
                    daCN.Fill(dtCN);
                    
                    cboChiNhanh.DataSource = dtCN;
                    cboChiNhanh.DisplayMember = "TENCN"; // Hiển thị tên
                    cboChiNhanh.ValueMember = "MACN";    // Lấy giá trị ID

                    // 2. Load Dịch Vụ
                    SqlDataAdapter daDV = new SqlDataAdapter("SELECT MALOAIDV, TENLOAIDV FROM LOAIDICHVU", conn);
                    DataTable dtDV = new DataTable();
                    daDV.Fill(dtDV);

                    cboDichVu.DataSource = dtDV;
                    cboDichVu.DisplayMember = "TENLOAIDV";
                    cboDichVu.ValueMember = "MALOAIDV";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Xử lý sự kiện bấm nút Đặt lịch
        private void XuLyDatLich()
        {
            // 1. Kiểm tra session
            if (!UserSession.IsLoggedIn || !UserSession.IsKhachHang())
            {
                MessageBox.Show("Phiên đăng nhập hết hạn hoặc bạn không phải Khách hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validate dữ liệu nhập
            if (cboChiNhanh.SelectedValue == null || cboDichVu.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Chi nhánh và Dịch vụ.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Chuẩn bị tham số
            int maKH = UserSession.UserId;
            int maCN = Convert.ToInt32(cboChiNhanh.SelectedValue);
            int maDV = Convert.ToInt32(cboDichVu.SelectedValue);
            DateTime ngay = dtpNgay.Value.Date;
            TimeSpan gio = dtpGio.Value.TimeOfDay;
            string noiDung = txtGhiChu.Text.Trim();

            if (string.IsNullOrEmpty(noiDung)) noiDung = "Đặt lịch qua App";

            // 4. Gọi SQL
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_DatLichHen", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Truyền tham số (Khớp với Procedure trong SQL)
                        cmd.Parameters.AddWithValue("@NgayHen", ngay);
                        cmd.Parameters.AddWithValue("@ThoiGian", gio);
                        cmd.Parameters.AddWithValue("@NoiDung", noiDung);
                        cmd.Parameters.AddWithValue("@MaKH", maKH);
                        cmd.Parameters.AddWithValue("@MaCN", maCN);
                        cmd.Parameters.AddWithValue("@MaLoaiDV", maDV);

                        // Tham số OUTPUT để lấy Mã lịch hẹn
                        SqlParameter outParam = new SqlParameter("@MaLichHen", SqlDbType.Int);
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();

                        // Lấy kết quả
                        string maLichMoi = outParam.Value.ToString() ?? "???";

                        MessageBox.Show($"Đặt lịch thành công!\nMã lịch hẹn của bạn: {maLichMoi}\nVui lòng đến đúng giờ.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Quay lại trang chủ
                        QuayLaiTrangChu?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (SqlException ex)
            {
                // Bắt lỗi logic SQL (Ví dụ: Trùng giờ, Chi nhánh nghỉ...)
                MessageBox.Show("Không thể đặt lịch: " + ex.Message, "Lỗi Nghiệp Vụ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Crash", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}