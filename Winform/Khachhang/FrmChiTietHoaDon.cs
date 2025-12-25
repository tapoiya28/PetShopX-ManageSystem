using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class FrmChiTietHoaDon : Form
    {
        private int _maHD;
        // Các controls
        private DataGridView dgvChiTiet;
        private Label lblThongTinTrai; // Chứa: Mã, Ngày, Chi nhánh, Thu ngân
        private Label lblThongTinPhai; // Chứa: Tổng tiền, KM, Thực thu
        private Label lblTieuDe;
        private Button btnClose;

        public FrmChiTietHoaDon(int maHD)
        {
            _maHD = maHD;
            // Setup Form
            this.Text = $"Chi Tiết Hóa Đơn #{maHD}";
            this.Size = new Size(750, 600); // Tăng chiều cao lên chút cho thoải mái
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            TaoGiaoDien();
            LoadDuLieu();
        }

        private void TaoGiaoDien()
        {
            // 1. Tiêu đề
            lblTieuDe = new Label() {
                Text = "CHI TIẾT HÓA ĐƠN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 102, 255),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 2. Thông tin bên TRÁI (Thông tin hành chính)
            lblThongTinTrai = new Label() {
                Location = new Point(25, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(64, 64, 64),
                MaximumSize = new Size(350, 0) // Giới hạn chiều rộng để không đè sang phải
            };

            // 3. Thông tin bên PHẢI (Thông tin tiền tệ - Cho nổi bật hơn)
            lblThongTinPhai = new Label() {
                Location = new Point(400, 60), // Đặt sang bên phải
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold), // Chữ to và đậm hơn
                ForeColor = Color.DarkRed,
                TextAlign = ContentAlignment.TopRight
            };

            // 4. Nút Đóng (Tạo trước để lấy tọa độ tính chiều cao Grid)
            btnClose = new Button() {
                Text = "Đóng",
                Width = 100,
                Height = 35,
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                // Neo nút đóng ở góc dưới bên phải form
                Location = new Point(610, 510) 
            };
            btnClose.Click += (s, e) => this.Close();

            // 5. Gridview Chi tiết
            dgvChiTiet = new DataGridView() {
                // Vị trí Y tạm thời, sẽ được tính lại trong LoadDuLieu
                Location = new Point(20, 180), 
                Size = new Size(690, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            
            // Style Header Grid
            dgvChiTiet.EnableHeadersVisualStyles = false;
            dgvChiTiet.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 102, 255);
            dgvChiTiet.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvChiTiet.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvChiTiet.ColumnHeadersHeight = 35;
            
            // Format tiền tệ cho cell
            dgvChiTiet.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            // Thêm button Đánh giá vào TaoGiaoDien()
            Button btnDanhGia = new Button() {
                Text = "⭐ Đánh giá",
                Width = 100,
                Height = 35,
                BackColor = Color.Gold,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(500, 510) // Đặt cạnh nút Đóng
            };

            // Sự kiện click mở form đánh giá
            btnDanhGia.Click += (s, e) => {
                // Mở form đánh giá dưới dạng Dialog
                new DanhGiaHoaDonForm(_maHD).ShowDialog();
            };

            this.Controls.AddRange(new Control[] { lblTieuDe, lblThongTinTrai, lblThongTinPhai, dgvChiTiet, btnClose,btnDanhGia });
        }

        private void LoadDuLieu()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_HoaDon_XemChiTiet", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MAHD", _maHD);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);

                            // --- Bảng 0: Thông tin Header Hóa Đơn ---
                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow r = ds.Tables[0].Rows[0];

                                decimal tongTien = r["TONGTIEN"] != DBNull.Value ? Convert.ToDecimal(r["TONGTIEN"]) : 0;
                                decimal thucThu = r["ThucThu"] != DBNull.Value ? Convert.ToDecimal(r["ThucThu"]) : 0;
                                int khuyenMai = r["KhuyenMai"] != DBNull.Value ? Convert.ToInt32(r["KhuyenMai"]) : 0;
                                string ngayLap = Convert.ToDateTime(r["NGAYLAP"]).ToString("dd/MM/yyyy HH:mm");

                                // Gán thông tin bên TRÁI
                                lblThongTinTrai.Text = $"Mã HĐ: #{_maHD}\n" +
                                                       $"Ngày lập: {ngayLap}\n" +
                                                       $"Chi nhánh: {r["TaiChiNhanh"]}\n" +
                                                       $"Thu ngân: {r["NhanVienLap"]}";

                                // Gán thông tin bên PHẢI (Tiền)
                                lblThongTinPhai.Text = $"Tổng tiền: {tongTien:N0} VNĐ\n" +
                                                       $"Khuyến mãi: {khuyenMai}%\n" +
                                                       $"THỰC THU: {thucThu:N0} VNĐ";
                            }

                            // --- Bảng 1: Chi tiết Dịch vụ/Sản phẩm ---
                            if (ds.Tables.Count > 1)
                            {
                                dgvChiTiet.DataSource = ds.Tables[1];

                                // Format cột tiền
                                if (dgvChiTiet.Columns["DONGIA"] != null) dgvChiTiet.Columns["DONGIA"].DefaultCellStyle.Format = "N0";
                                if (dgvChiTiet.Columns["ThanhTien"] != null) dgvChiTiet.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";

                                // Đặt tên cột tiếng Việt
                                SetHeader(dgvChiTiet, "TENSP", "Tên Dịch Vụ / SP");
                                SetHeader(dgvChiTiet, "LOAI", "Loại");
                                SetHeader(dgvChiTiet, "DONGIA", "Đơn Giá");
                                SetHeader(dgvChiTiet, "SOLUONG", "SL");
                                SetHeader(dgvChiTiet, "ThanhTien", "Thành Tiền");
                            }

                            // --- QUAN TRỌNG: TÍNH TOÁN LẠI VỊ TRÍ GRID ---
                            CapNhatBoCucGiaoDien();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết hóa đơn: " + ex.Message);
            }
        }

        // Hàm này xử lý việc Grid không bị đè lên chữ
        private void CapNhatBoCucGiaoDien()
        {
            // 1. Tìm vị trí Y thấp nhất của 2 label thông tin
            int bottomTrai = lblThongTinTrai.Bottom;
            int bottomPhai = lblThongTinPhai.Bottom;
            int maxBottom = Math.Max(bottomTrai, bottomPhai);

            // 2. Đặt vị trí bắt đầu của Grid cách label thấp nhất 20px
            int gridY = maxBottom + 20;
            dgvChiTiet.Location = new Point(20, gridY);

            // 3. Tính chiều cao còn lại cho Grid để không đè lên nút Đóng
            // (Chiều cao Form - Tiêu đề - Vị trí Y Grid - Khoảng trống dưới cùng)
            int availableHeight = btnClose.Top - gridY - 15;
            
            // Đảm bảo chiều cao không bị âm
            if (availableHeight < 100) availableHeight = 100;
            
            dgvChiTiet.Height = availableHeight;
        }

        private void SetHeader(DataGridView dgv, string colName, string text)
        {
            if (dgv.Columns.Contains(colName)) dgv.Columns[colName].HeaderText = text;
        }
    }
}