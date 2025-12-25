using System;
using System.Drawing;
using System.Drawing.Drawing2D; // Thêm thư viện vẽ đồ họa
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    /// <summary>
    /// UC Thống Kê - Đã được làm đẹp giao diện (UI)
    /// </summary>
    public class UCThongKe : UserControl
    {
        // Tab control chính
        private TabControl tabMain;

        // Tab 1: Tình hình kinh doanh
        private TabPage tabKinhDoanh;
        private ComboBox cboNam1;
        private ComboBox cboThang1;
        private Button btnXemKinhDoanh;
        private DataGridView dgvKinhDoanh;
        private Label lblTongDoanhThu;
        private Label lblTongDon;

        // Tab 2: Thống kê sản phẩm
        private TabPage tabSanPham;
        private ComboBox cboNam2;
        private ComboBox cboThang2;
        private Button btnXemSanPham;
        private DataGridView dgvSanPham;
        private Button btnSanPhamBanChay;
        private DataGridView dgvSanPhamBanChay;

        // Tab 3: Phân tích khách hàng
        private TabPage tabKhachHang;
        private ComboBox cboNam3;
        private Button btnThongKeKH;
        private DataGridView dgvKhachHang;
        private Button btnPhanTichKH;
        private DataGridView dgvPhanTich;
        private Button btnThongKeCapBac;
        private DataGridView dgvCapBac;

        // Tab 4: Thống kê nhân viên
        private TabPage tabNhanVien;
        private ComboBox cboNam4;
        private ComboBox cboThang4;
        private Button btnThongKeNV;
        private DataGridView dgvNhanVien;
        private Button btnTinhLuong;

        // Tab 5: Thống kê đánh giá
        private TabPage tabDanhGia;
        private ComboBox cboNam5;
        private ComboBox cboThang5;
        private Button btnXemDanhGia;
        private DataGridView dgvDanhGia;

        // Tab 6: Tồn kho thấp
        private TabPage tabTonKho;
        private Button btnXemTonKho;
        private DataGridView dgvTonKho;

        public UCThongKe()
        {
            this.Size = new Size(1100, 750);
            this.BackColor = Color.FromArgb(240, 242, 245); // Màu nền xám nhạt hiện đại
            this.AutoScroll = true;

            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            // --- HEADER ---
            Panel pnlHeader = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(1100, 70), // Tăng chiều cao header
                BackColor = Color.FromArgb(41, 128, 185),
                Dock = DockStyle.Top
            };

            Label lblTitle = new Label()
            {
                Text = "BÁO CÁO & THỐNG KÊ HỆ THỐNG",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            Label lblSubtitle = new Label()
            {
                Text = "Dữ liệu được cập nhật theo thời gian thực",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 230, 255),
                Location = new Point(22, 45),
                AutoSize = true
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            // --- TAB CONTROL (Đã Custom lại cho đẹp) ---
            tabMain = new TabControl()
            {
                Location = new Point(10, 80),
                Size = new Size(1080, 660),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Padding = new Point(15, 8),
                DrawMode = TabDrawMode.OwnerDrawFixed, // QUAN TRỌNG: Để tự vẽ màu
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(140, 40) // Kích thước mỗi tab
            };

            // Sự kiện vẽ lại Tab cho đẹp (Phẳng, không viền lồi lõm)
            tabMain.DrawItem += (s, e) =>
            {
                TabControl tc = (TabControl)s;
                TabPage page = tc.TabPages[e.Index];
                Rectangle r = tc.GetTabRect(e.Index);

                // Tô nền Tab
                bool isSelected = (e.State == DrawItemState.Selected);
                e.Graphics.FillRectangle(new SolidBrush(isSelected ? Color.White : Color.FromArgb(230, 230, 230)), r);

                // Vẽ text
                Color textColor = isSelected ? Color.FromArgb(41, 128, 185) : Color.DimGray;
                Font tabFont = isSelected ? new Font(tc.Font, FontStyle.Bold) : tc.Font;
                TextRenderer.DrawText(e.Graphics, page.Text, tabFont, r, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // Vẽ vạch màu dưới chân tab đang chọn
                if (isSelected)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(41, 128, 185)), r.X, r.Bottom - 3, r.Width, 3);
                }
            };

            // Tạo các tab con
            TaoTabKinhDoanh();
            TaoTabSanPham();
            TaoTabKhachHang();
            TaoTabNhanVien();
            TaoTabDanhGia();
            TaoTabTonKho();

            this.Controls.Add(pnlHeader);
            this.Controls.Add(tabMain);
        }

        // ============ TAB 1: TÌNH HÌNH KINH DOANH (ĐÃ SỬA UI) ============
        private void TaoTabKinhDoanh()
        {
            tabKinhDoanh = new TabPage("💰 Kinh Doanh");
            tabKinhDoanh.BackColor = Color.White;

            // Panel filter
            Panel pnlFilter = CreateFilterPanel(out cboNam1, out cboThang1, out btnXemKinhDoanh, true);
            btnXemKinhDoanh.Click += BtnXemKinhDoanh_Click;

            // Grid
            dgvKinhDoanh = TaoDataGridView(new Point(20, 100), new Size(1030, 310));

            // --- CARD 1: DOANH THU (Dùng RoundedPanel và TableLayout để không mất chữ) ---
            RoundedPanel pnlCardDoanhThu = new RoundedPanel()
            {
                Location = new Point(50, 430),
                Size = new Size(450, 100), // Tăng chiều cao
                BackColor = Color.FromArgb(46, 204, 113),
                Radius = 20
            };
            pnlCardDoanhThu.Controls.Add(CreateCardContent("💰", "TỔNG DOANH THU", out lblTongDoanhThu, "0 đ"));

            // --- CARD 2: ĐƠN HÀNG ---
            RoundedPanel pnlCardDon = new RoundedPanel()
            {
                Location = new Point(550, 430),
                Size = new Size(450, 100),
                BackColor = Color.FromArgb(52, 152, 219),
                Radius = 20
            };
            pnlCardDon.Controls.Add(CreateCardContent("📦", "TỔNG SỐ ĐƠN", out lblTongDon, "0"));

            tabKinhDoanh.Controls.AddRange(new Control[] { pnlFilter, pnlCardDoanhThu, pnlCardDon, dgvKinhDoanh });
            tabMain.TabPages.Add(tabKinhDoanh);
        }

        // ============ CÁC TAB KHÁC (GIỮ NGUYÊN LOGIC, CHỈNH UI) ============

        private void TaoTabSanPham()
        {
            tabSanPham = new TabPage("📦 Sản Phẩm");
            tabSanPham.BackColor = Color.White;

            // Phần 1
            Label lbl1 = CreateSectionTitle("THỐNG KÊ SẢN PHẨM BÁN RA", 20, 15);
            Panel pnlFilter = CreateFilterPanel(out cboNam2, out cboThang2, out btnXemSanPham, true, 20, 45);
            btnXemSanPham.Click += BtnXemSanPham_Click;
            dgvSanPham = TaoDataGridView(new Point(20, 120), new Size(1030, 200));

            // Phần 2
            Label lbl2 = CreateSectionTitle("🔥 TOP SẢN PHẨM BÁN CHẠY", 20, 340);
            btnSanPhamBanChay = CreateButton("Xem Top Bán Chạy", new Point(20, 370), Color.FromArgb(230, 126, 34));
            btnSanPhamBanChay.Click += BtnSanPhamBanChay_Click;
            dgvSanPhamBanChay = TaoDataGridView(new Point(20, 415), new Size(1030, 190));

            tabSanPham.Controls.AddRange(new Control[] { lbl1, pnlFilter, dgvSanPham, lbl2, btnSanPhamBanChay, dgvSanPhamBanChay });
            tabMain.TabPages.Add(tabSanPham);
        }

        private void TaoTabKhachHang()
        {
            tabKhachHang = new TabPage("👥 Khách Hàng");
            tabKhachHang.BackColor = Color.White;
            tabKhachHang.AutoScroll = true;

            // Phần 1
            Label lbl1 = CreateSectionTitle("THỐNG KÊ KHÁCH HÀNG MỚI", 20, 15);
            Panel pnlFilter = CreateFilterPanel(out cboNam3, out _, out btnThongKeKH, false, 20, 45);
            btnThongKeKH.Click += BtnThongKeKH_Click;
            dgvKhachHang = TaoDataGridView(new Point(20, 120), new Size(1030, 150));

            // Phần 2
            Label lbl2 = CreateSectionTitle("PHÂN TÍCH TIỀM NĂNG", 20, 290);
            btnPhanTichKH = CreateButton("Phân tích ngay", new Point(20, 320), Color.FromArgb(142, 68, 173));
            btnPhanTichKH.Click += BtnPhanTichKH_Click;
            dgvPhanTich = TaoDataGridView(new Point(20, 365), new Size(1030, 150));

            // Phần 3
            Label lbl3 = CreateSectionTitle("CƠ CẤU CẤP BẬC (VIP/MEMBER)", 20, 535);
            btnThongKeCapBac = CreateButton("Xem tỷ lệ", new Point(20, 565), Color.FromArgb(39, 174, 96));
            btnThongKeCapBac.Click += BtnThongKeCapBac_Click;
            dgvCapBac = TaoDataGridView(new Point(20, 610), new Size(1030, 150));

            tabKhachHang.Controls.AddRange(new Control[] { lbl1, pnlFilter, dgvKhachHang, lbl2, btnPhanTichKH, dgvPhanTich, lbl3, btnThongKeCapBac, dgvCapBac });
            tabMain.TabPages.Add(tabKhachHang);
        }

        private void TaoTabNhanVien()
        {
            tabNhanVien = new TabPage("👨‍💼 Nhân Viên");
            tabNhanVien.BackColor = Color.White;

            Label lbl1 = CreateSectionTitle("HIỆU SUẤT NHÂN VIÊN", 20, 15);
            Panel pnlFilter = CreateFilterPanel(out cboNam4, out cboThang4, out btnThongKeNV, true, 20, 45);
            btnThongKeNV.Click += BtnThongKeNV_Click;
            dgvNhanVien = TaoDataGridView(new Point(20, 120), new Size(1030, 250));

            Label lbl2 = CreateSectionTitle("TÍNH LƯƠNG & HOA HỒNG", 20, 390);
            btnTinhLuong = CreateButton("💰 Tính lương tháng này", new Point(20, 420), Color.FromArgb(46, 204, 113));
            btnTinhLuong.Size = new Size(200, 40);
            btnTinhLuong.Click += BtnTinhLuong_Click;

            tabNhanVien.Controls.AddRange(new Control[] { lbl1, pnlFilter, dgvNhanVien, lbl2, btnTinhLuong });
            tabMain.TabPages.Add(tabNhanVien);
        }

        private void TaoTabDanhGia()
        {
            tabDanhGia = new TabPage("⭐ Đánh Giá");
            tabDanhGia.BackColor = Color.White;

            Label lbl1 = CreateSectionTitle("PHẢN HỒI KHÁCH HÀNG", 20, 15);
            Panel pnlFilter = CreateFilterPanel(out cboNam5, out cboThang5, out btnXemDanhGia, true, 20, 45);
            btnXemDanhGia.Click += BtnXemDanhGia_Click;
            dgvDanhGia = TaoDataGridView(new Point(20, 120), new Size(1030, 480));

            tabDanhGia.Controls.AddRange(new Control[] { lbl1, pnlFilter, dgvDanhGia });
            tabMain.TabPages.Add(tabDanhGia);
        }

        private void TaoTabTonKho()
        {
            tabTonKho = new TabPage("📉 Tồn Kho");
            tabTonKho.BackColor = Color.White;

            Label lbl1 = CreateSectionTitle("⚠️ CẢNH BÁO TỒN KHO THẤP", 20, 15);
            lbl1.ForeColor = Color.FromArgb(231, 76, 60);

            Label lblNote = new Label()
            {
                Text = "Danh sách các sản phẩm cần nhập thêm hàng ngay lập tức",
                Location = new Point(22, 45),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            btnXemTonKho = CreateButton("🔍 Quét kho hàng", new Point(20, 70), Color.FromArgb(231, 76, 60));
            btnXemTonKho.Click += BtnXemTonKho_Click;

            dgvTonKho = TaoDataGridView(new Point(20, 120), new Size(1030, 480));

            tabTonKho.Controls.AddRange(new Control[] { lbl1, lblNote, btnXemTonKho, dgvTonKho });
            tabMain.TabPages.Add(tabTonKho);
        }

        // ============ HÀM HỖ TRỢ UI (HELPER) ============

        private DataGridView TaoDataGridView(Point location, Size size)
        {
            DataGridView dgv = new DataGridView()
            {
                Location = location,
                Size = size,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, // Bỏ viền xấu
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, // Chỉ kẻ ngang
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false, // Ẩn cột đầu dòng
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 40 } // Tăng chiều cao dòng cho thoáng
            };

            // Header đẹp
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 247);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 246, 247);
            dgv.ColumnHeadersHeight = 45;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Dòng dữ liệu
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(64, 64, 64);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 245, 255); // Xanh rất nhạt
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);

            return dgv;
        }

        // Hàm tạo Panel lọc (Năm/Tháng) dùng chung
        private Panel CreateFilterPanel(out ComboBox cboNam, out ComboBox cboThang, out Button btnAction, bool hasMonth, int x = 20, int y = 20)
        {
            Panel pnl = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(1020, 60),
                BackColor = Color.FromArgb(248, 249, 250), // Xám rất nhạt
            };
            // Tạo đường viền dưới nhẹ cho panel
            pnl.Paint += (s, e) => { ControlPaint.DrawBorder(e.Graphics, pnl.ClientRectangle, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None, Color.LightGray, 1, ButtonBorderStyle.Solid); };

            Label lblN = new Label() { Text = "Năm:", Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cboNam = new ComboBox() { Location = new Point(60, 17), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
            TaiDanhSachNam(cboNam);

            pnl.Controls.Add(lblN);
            pnl.Controls.Add(cboNam);

            cboThang = null;
            int btnX = 180;

            if (hasMonth)
            {
                Label lblT = new Label() { Text = "Tháng:", Location = new Point(180, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
                cboThang = new ComboBox() { Location = new Point(235, 17), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10), FlatStyle = FlatStyle.Flat };
                cboThang.Items.Add("Tất cả");
                for (int i = 1; i <= 12; i++) cboThang.Items.Add(i);
                cboThang.SelectedIndex = 0;
                pnl.Controls.Add(lblT);
                pnl.Controls.Add(cboThang);
                btnX = 360;
            }

            btnAction = CreateButton("Xem báo cáo", new Point(btnX, 15), Color.FromArgb(41, 128, 185));
            pnl.Controls.Add(btnAction);

            return pnl;
        }

        private Button CreateButton(string text, Point loc, Color bg)
        {
            return new Button()
            {
                Text = text,
                Location = loc,
                Size = new Size(140, 32),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private Label CreateSectionTitle(string text, int x, int y)
        {
            return new Label()
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64)
            };
        }

        // Tạo nội dung bên trong Card (Icon + Text + Số liệu)
        private Control CreateCardContent(string icon, string title, out Label lblValue, string defaultVal)
        {
            TableLayoutPanel tbl = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(15, 10, 10, 10)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            Label lblIcon = new Label() { Text = icon, Font = new Font("Segoe UI", 32), ForeColor = Color.White, AutoSize = true, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };

            Label lblTitle = new Label() { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = Color.FromArgb(230, 230, 230), AutoSize = true, Dock = DockStyle.Bottom };

            lblValue = new Label()
            {
                Text = defaultVal,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Dock = DockStyle.Top
            };

            tbl.Controls.Add(lblIcon, 0, 0);
            tbl.SetRowSpan(lblIcon, 2);
            tbl.Controls.Add(lblTitle, 1, 0);
            tbl.Controls.Add(lblValue, 1, 1);

            return tbl;
        }

        private void TaiDanhSachNam(ComboBox cbo)
        {
            int namHienTai = DateTime.Now.Year;
            for (int i = namHienTai; i >= namHienTai - 4; i--)
            {
                cbo.Items.Add(i);
            }
            if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
        }

        // ============ SỰ KIỆN LOGIC (GIỮ NGUYÊN CODE CŨ CỦA BẠN NHƯNG CHUẨN HÓA) ============

        private void BtnXemKinhDoanh_Click(object sender, EventArgs e)
        {
            try
            {
                int nam = Convert.ToInt32(cboNam1.SelectedItem);
                int? thang = cboThang1.SelectedIndex == 0 ? (int?)null : Convert.ToInt32(cboThang1.SelectedItem);

                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_TinhHinhKinhDoanh", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Nam", nam);
                        cmd.Parameters.AddWithValue("@Thang", (object)thang ?? DBNull.Value);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvKinhDoanh.DataSource = dt;

                        decimal tongDoanhThu = 0;
                        int tongDon = 0;
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["Tổng Doanh thu"] != DBNull.Value)
                                    tongDoanhThu += Convert.ToDecimal(row["Tổng Doanh thu"]);
                                if (row["Số lượng đơn"] != DBNull.Value)
                                    tongDon += Convert.ToInt32(row["Số lượng đơn"]);
                            }
                        }
                        lblTongDoanhThu.Text = $"{tongDoanhThu:N0} đ";
                        lblTongDon.Text = $"{tongDon}";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnXemSanPham_Click(object sender, EventArgs e) => LoadData(dgvSanPham, "sp_ThongKeSanPham", cboNam2, cboThang2);
        private void BtnSanPhamBanChay_Click(object sender, EventArgs e) => LoadDataSimple(dgvSanPhamBanChay, "sp_SanPhamBanChay");
        private void BtnThongKeKH_Click(object sender, EventArgs e) => LoadData(dgvKhachHang, "sp_ThongKeKhachHang", cboNam3, null);
        private void BtnPhanTichKH_Click(object sender, EventArgs e) => LoadDataSimple(dgvPhanTich, "sp_PhanTichKhachHang");
        private void BtnThongKeNV_Click(object sender, EventArgs e) => LoadData(dgvNhanVien, "sp_ThongKeHieuSuatNhanVien", cboNam4, cboThang4);
        private void BtnXemDanhGia_Click(object sender, EventArgs e) => LoadData(dgvDanhGia, "sp_ThongKeDanhGia", cboNam5, cboThang5);

        // Helper để load dữ liệu nhanh
        private void LoadData(DataGridView dgv, string spName, ComboBox cbNam, ComboBox cbThang)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(spName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (cbNam != null) cmd.Parameters.AddWithValue("@Nam", Convert.ToInt32(cbNam.SelectedItem));
                        if (cbThang != null)
                        {
                            int? thang = cbThang.SelectedIndex == 0 ? (int?)null : Convert.ToInt32(cbThang.SelectedItem);
                            cmd.Parameters.AddWithValue("@Thang", (object)thang ?? DBNull.Value);
                        }
                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        dgv.DataSource = dt;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void LoadDataSimple(DataGridView dgv, string spName)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    new SqlDataAdapter(spName, conn).Fill(dt);
                    dgv.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnThongKeCapBac_Click(object sender, EventArgs e)
        {
            // Logic SQL trần giữ nguyên như bạn muốn
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT TENCAPBAC AS [Cấp Bậc], COUNT(*) AS [Số Lượng KH], 
                                     CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM KHACHHANG) AS DECIMAL(5,2)) AS [Tỷ Lệ %]
                                     FROM KHACHHANG GROUP BY TENCAPBAC ORDER BY COUNT(*) DESC";
                    DataTable dt = new DataTable();
                    new SqlDataAdapter(query, conn).Fill(dt);
                    dgvCapBac.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnXemTonKho_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    DataTable dt = new DataTable();
                    new SqlDataAdapter("SELECT * FROM f_TonKhoThap()", conn).Fill(dt);
                    dgvTonKho.DataSource = dt;
                    if (dt.Rows.Count > 0) MessageBox.Show($"Cảnh báo: Có {dt.Rows.Count} sản phẩm sắp hết hàng!", "Cảnh báo kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else MessageBox.Show("Kho hàng ổn định.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnTinhLuong_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Xác nhận tính lương cho toàn bộ nhân viên?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                // Cải thiện logic để không bị treo
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    // Lấy danh sách NV
                    DataTable dtNV = new DataTable();
                    new SqlDataAdapter("SELECT MANV FROM NHANVIEN", conn).Fill(dtNV);

                    int count = 0;
                    foreach (DataRow row in dtNV.Rows)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_Sub_TinhLuong", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MANV", row["MANV"]);
                            cmd.Parameters.AddWithValue("@Nam", DateTime.Now.Year);
                            cmd.Parameters.AddWithValue("@Thang", DateTime.Now.Month);
                            cmd.ExecuteNonQuery();
                            count++;
                        }
                    }
                    MessageBox.Show($"Đã tính lương xong cho {count} nhân viên.", "Thành công");
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tính lương: " + ex.Message); }
        }
    }

    /// <summary>
    /// Class hỗ trợ Panel bo tròn góc
    /// </summary>
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 20;

        public RoundedPanel()
        {
            this.DoubleBuffered = true; // Giảm giật hình
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = new GraphicsPath())
            {
                Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                int r = Radius;

                path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                path.AddArc(rect.Right - r, rect.Y, r, r, 270, 90);
                path.AddArc(rect.Right - r, rect.Bottom - r, r, r, 0, 90);
                path.AddArc(rect.X, rect.Bottom - r, r, r, 90, 90);
                path.CloseAllFigures();

                this.Region = new Region(path);

                // Vẽ viền mỏng nếu cần (tùy chọn)
                using (Pen pen = new Pen(this.BackColor, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }
    }
}