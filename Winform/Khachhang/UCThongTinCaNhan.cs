using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    public class UCThongTinCaNhan : UserControl
    {
        // --- Các thành phần giao diện ---
        private Label lblTenKhach, lblSDT, lblDiaChi, lblCapBac;
        private Label lblTongChiTieu, lblLanGheGanNhat, lblAvatar;
        
        private TabControl tabMain;
        private Label lblCurrentPetKham; 
        private Label lblCurrentPetTiem; 

        private DataGridView dgvThuCung;
        private DataGridView dgvLichSuKham;
        private DataGridView dgvLichSuTiem;
        private DataGridView dgvHoaDon;

        // --- Cấu hình màu sắc ---
        private Color primaryColor = Color.FromArgb(51, 102, 255); 
        private Color headerBg = Color.WhiteSmoke;

        public UCThongTinCaNhan()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // 1. Khởi tạo giao diện
            TaoGiaoDienHienDai();

            // 2. Load dữ liệu
            if (!this.DesignMode)
            {
                LoadThongTinChung();
                LoadDanhSachThuCung();
                LoadLichSuHoaDon();
            }
        }

        #region 1. THIẾT KẾ GIAO DIỆN (UI) - ĐÃ FIX LỖI ĐÈ NHAU

        private void TaoGiaoDienHienDai()
        {
            // --- 1. PANEL HEADER (Thông tin khách hàng) ---
            Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 140, BackColor = headerBg, Padding = new Padding(15) };
            
            lblAvatar = new Label() { Text = "👤", Font = new Font("Segoe UI", 45), Size = new Size(80, 80), Location = new Point(20, 20), ForeColor = primaryColor };
            lblTenKhach = CreateLabel("Đang tải tên...", 110, 20, 16, true, primaryColor);
            lblSDT = CreateLabel("SĐT: --", 110, 55, 11);
            lblDiaChi = CreateLabel("Địa chỉ: --", 110, 80, 11);
            
            Panel pnlStats = new Panel() { Dock = DockStyle.Right, Width = 400, BackColor = Color.Transparent };
            lblCapBac = CreateLabel("Hạng: --", 20, 25, 12, true, Color.OrangeRed);
            lblTongChiTieu = CreateLabel("Chi tiêu: 0 đ", 20, 55, 11);
            lblLanGheGanNhat = CreateLabel("Ghé gần nhất: --", 20, 80, 11);
            
            pnlStats.Controls.AddRange(new Control[] { lblCapBac, lblTongChiTieu, lblLanGheGanNhat });
            pnlHeader.Controls.AddRange(new Control[] { lblAvatar, lblTenKhach, lblSDT, lblDiaChi, pnlStats });

            // --- 2. TAB CONTROL (Nội dung chính) ---
            tabMain = new TabControl() 
            { 
                Dock = DockStyle.Fill, 
                Font = new Font("Segoe UI", 10),
                // FIX: Chuyển sang Normal để Tab tự giãn theo độ dài chữ, tránh bị đè/cắt chữ
                SizeMode = TabSizeMode.Normal, 
                Padding = new Point(15, 10) 
            };

            // TAB 1: DANH SÁCH THÚ CƯNG
            TabPage tabPet = new TabPage("🐾 Danh Sách Thú Cưng") { BackColor = Color.White, Padding = new Padding(10) };
            dgvThuCung = CreateModernGrid();
            dgvThuCung.SelectionChanged += DgvThuCung_SelectionChanged;
            tabPet.Controls.Add(dgvThuCung); // Grid Fill toàn bộ

            // TAB 2: LỊCH SỬ KHÁM BỆNH
            TabPage tabKham = new TabPage("🏥 Lịch Sử Khám Bệnh") { BackColor = Color.White, Padding = new Padding(10) };
            lblCurrentPetKham = new Label() { Text = "Vui lòng chọn thú cưng ở tab Danh Sách", Dock = DockStyle.Top, Height = 40, ForeColor = Color.DarkGreen, Font = new Font("Segoe UI", 10, FontStyle.Italic), TextAlign = ContentAlignment.MiddleLeft };
            dgvLichSuKham = CreateModernGrid();
            dgvLichSuKham.CellDoubleClick += DgvLichSuKham_CellDoubleClick;
            
            // FIX THỨ TỰ: Thêm Grid trước, Label sau rồi mang Label lên trước (BringToFront)
            tabKham.Controls.Add(dgvLichSuKham);
            tabKham.Controls.Add(lblCurrentPetKham);
            lblCurrentPetKham.BringToFront(); 

            // TAB 3: LỊCH SỬ TIÊM PHÒNG
            TabPage tabTiem = new TabPage("💉 Lịch Sử Tiêm Phòng") { BackColor = Color.White, Padding = new Padding(10) };
            lblCurrentPetTiem = new Label() { Text = "Vui lòng chọn thú cưng ở tab Danh Sách", Dock = DockStyle.Top, Height = 40, ForeColor = Color.DarkOrange, Font = new Font("Segoe UI", 10, FontStyle.Italic), TextAlign = ContentAlignment.MiddleLeft };
            dgvLichSuTiem = CreateModernGrid();
            
            tabTiem.Controls.Add(dgvLichSuTiem);
            tabTiem.Controls.Add(lblCurrentPetTiem);
            lblCurrentPetTiem.BringToFront();

            // TAB 4: LỊCH SỬ HÓA ĐƠN
            TabPage tabHoaDon = new TabPage("📜 Lịch Sử Hóa Đơn") { BackColor = Color.White, Padding = new Padding(10) };
            dgvHoaDon = CreateModernGrid();
            dgvHoaDon.CellDoubleClick += DgvHoaDon_CellDoubleClick;
            tabHoaDon.Controls.Add(dgvHoaDon);

            // Thêm các tab vào TabControl
            tabMain.TabPages.AddRange(new TabPage[] { tabPet, tabKham, tabTiem, tabHoaDon });

            // Thêm Control vào UserControl (Thứ tự quan quan trọng để Dock không đè nhau)
            this.Controls.Add(tabMain);   // Sẽ Fill phần còn lại
            this.Controls.Add(pnlHeader); // Sẽ nằm trên cùng (Top)
        }

        private DataGridView CreateModernGrid()
        {
            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = Color.WhiteSmoke;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = primaryColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 40;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowTemplate.Height = 35;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            return dgv;
        }

        private Label CreateLabel(string text, int x, int y, float size, bool bold = false, Color? color = null)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            lbl.Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular);
            lbl.ForeColor = color ?? Color.FromArgb(64, 64, 64);
            return lbl;
        }

        #endregion

        #region 2. LOGIC XỬ LÝ DỮ LIỆU

        private void LoadThongTinChung()
        {
            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_KhachHang_XemChiTiet", conn)) {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MAKH", UserSession.UserId);
                        using (SqlDataReader r = cmd.ExecuteReader()) {
                            if (r.Read()) {
                                lblTenKhach.Text = r["TENKH"].ToString();
                                lblSDT.Text = "SĐT: " + r["SDT"].ToString();
                                lblDiaChi.Text = "Địa chỉ: " + r["DIACHI"].ToString();
                                lblCapBac.Text = "Hạng thành viên: " + r["TENCAPBAC"].ToString();
                                decimal chiTieu = r["TongChiTieu"] != DBNull.Value ? Convert.ToDecimal(r["TongChiTieu"]) : 0;
                                lblTongChiTieu.Text = $"Tổng chi tiêu: {chiTieu:N0} VNĐ";
                                if (r["LanGheGanNhat"] != DBNull.Value)
                                    lblLanGheGanNhat.Text = $"Ghé gần nhất: {Convert.ToDateTime(r["LanGheGanNhat"]):dd/MM/yyyy}";
                            }
                        }
                    }
                }
            } catch { }
        }

        private void LoadDanhSachThuCung()
        {
            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("sp_ThuCung_XemDanhSach", conn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@MAKH", UserSession.UserId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvThuCung.DataSource = dt;
                    
                    string[] hideCols = { "MaKH", "ChuSoHuu", "SDT", "HinhAnh" };
                    foreach (string col in hideCols) if (dgvThuCung.Columns.Contains(col)) dgvThuCung.Columns[col].Visible = false;
                    
                    SetHeader(dgvThuCung, "MATC", "Mã TC"); SetHeader(dgvThuCung, "TENTC", "Tên Thú Cưng");
                    SetHeader(dgvThuCung, "LOAI", "Loài"); SetHeader(dgvThuCung, "GIONG", "Giống");
                    SetHeader(dgvThuCung, "CANNANG", "Cân nặng"); SetHeader(dgvThuCung, "MAUSAC", "Màu sắc");
                    SetHeader(dgvThuCung, "TINHTRANG", "Tình trạng");
                }
            } catch { }
        }

        private void DgvThuCung_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvThuCung.SelectedRows.Count > 0)
            {
                var row = dgvThuCung.SelectedRows[0];
                if (dgvThuCung.Columns.Contains("MATC") && row.Cells["MATC"].Value != DBNull.Value)
                {
                    int maTC = Convert.ToInt32(row.Cells["MATC"].Value);
                    string tenTC = dgvThuCung.Columns.Contains("TENTC") ? row.Cells["TENTC"].Value.ToString() : "Thú cưng";
                    
                    lblCurrentPetKham.Text = $"📍 Đang xem lịch sử khám của: {tenTC}";
                    lblCurrentPetTiem.Text = $"📍 Đang xem lịch sử tiêm của: {tenTC}";

                    LoadLichSuYTe(maTC);
                }
            }
        }

        private void LoadLichSuYTe(int maTC)
        {
            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    // Khám bệnh
                    SqlDataAdapter daKham = new SqlDataAdapter("sp_ThuCung_LichSuKham", conn);
                    daKham.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daKham.SelectCommand.Parameters.AddWithValue("@MATC", maTC);
                    DataTable dtKham = new DataTable(); daKham.Fill(dtKham);
                    dgvLichSuKham.DataSource = dtKham;
                    SetHeader(dgvLichSuKham, "MAKB", "Mã Ca"); 
                    SetHeader(dgvLichSuKham, "NGAYKHAM", "Ngày Khám"); 
                    SetHeader(dgvLichSuKham, "CHANDOAN", "Chẩn Đoán"); 

                    // Tiêm phòng
                    SqlDataAdapter daTiem = new SqlDataAdapter("sp_ThuCung_LichSuTiem", conn);
                    daTiem.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daTiem.SelectCommand.Parameters.AddWithValue("@MATC", maTC);
                    DataTable dtTiem = new DataTable(); daTiem.Fill(dtTiem);
                    dgvLichSuTiem.DataSource = dtTiem;
                    SetHeader(dgvLichSuTiem, "NGAYTIEM", "Ngày Tiêm"); 
                    SetHeader(dgvLichSuTiem, "TENWACCINE", "Vaccine"); 
                }
            } catch { }
        }

        private void LoadLichSuHoaDon()
        {
            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("sp_HoaDon_DanhSach", conn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@MAKH", UserSession.UserId);
                    DataTable dt = new DataTable(); da.Fill(dt);
                    dgvHoaDon.DataSource = dt;
                    if (dgvHoaDon.Columns.Contains("TONGTIEN")) dgvHoaDon.Columns["TONGTIEN"].DefaultCellStyle.Format = "N0";
                }
            } catch { }
        }

        private void SetHeader(DataGridView dgv, string colName, string text)
        {
            if (dgv != null && dgv.Columns.Contains(colName)) dgv.Columns[colName].HeaderText = text;
        }

        private void DgvLichSuKham_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvLichSuKham.Columns.Contains("MAKB"))
            {
                int maKB = Convert.ToInt32(dgvLichSuKham.Rows[e.RowIndex].Cells["MAKB"].Value);
                new FrmChiTietCaKham(maKB).ShowDialog();
            }
        }

        private void DgvHoaDon_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvHoaDon.Columns.Contains("MAHD"))
            {
                int maHD = Convert.ToInt32(dgvHoaDon.Rows[e.RowIndex].Cells["MAHD"].Value);
                new FrmChiTietHoaDon(maHD).ShowDialog();
            }
        }

        #endregion
    }
}