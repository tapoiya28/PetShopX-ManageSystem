using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    public class UCThongKe : UserControl
    {
        private TabControl tabMain;

        // Tab 1
        private TabPage tabKinhDoanh;
        private ComboBox cboNam1, cboThang1;
        private Button btnXemKinhDoanh;
        private DataGridView dgvKinhDoanh;
        private Label lblTongDoanhThu, lblTongDon;

        // Tab 2
        private TabPage tabSanPham;
        private ComboBox cboNam2, cboThang2;
        private Button btnXemSanPham, btnSanPhamBanChay;
        private DataGridView dgvSanPham, dgvSanPhamBanChay;

        // Tab 3
        private TabPage tabKhachHang;
        private ComboBox cboNam3;
        private Button btnThongKeKH, btnPhanTichKH, btnThongKeCapBac;
        private DataGridView dgvKhachHang, dgvPhanTich, dgvCapBac;

        // Tab 4
        private TabPage tabNhanVien;
        private ComboBox cboNam4, cboThang4;
        private Button btnThongKeNV, btnTinhLuong;
        private DataGridView dgvNhanVien;

        // Tab 5
        private TabPage tabDanhGia;
        private ComboBox cboNam5, cboThang5;
        private Button btnXemDanhGia;
        private DataGridView dgvDanhGia;

        // Tab 6
        private TabPage tabTonKho;
        private Button btnXemTonKho;
        private DataGridView dgvTonKho;

        public UCThongKe()
        {
            this.Size = new Size(1100, 750);
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.AutoScroll = true;
            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            // --- HEADER ---
            // Tăng Height lên 95 để subtitle không bị đè
            Panel pnlHeader = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(1100, 95),
                BackColor = Color.FromArgb(41, 128, 185),
                Dock = DockStyle.Top
            };

            Label lblTitle = new Label()
            {
                Text = "BÁO CÁO & THỐNG KÊ HỆ THỐNG",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            Label lblSubtitle = new Label()
            {
                Text = "Dữ liệu được cập nhật theo thời gian thực",
                Font = new Font("Segoe UI", 9, FontStyle.Italic), // Chỉnh nghiêng cho đẹp
                ForeColor = Color.FromArgb(210, 235, 255),
                Location = new Point(22, 55), // Đẩy xuống so với Title
                AutoSize = true
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle });

            // --- TAB CONTROL ---
            // Đẩy tabMain xuống Y=105 để tách biệt hoàn toàn với Header
            tabMain = new TabControl()
            {
                Location = new Point(10, 105),
                Size = new Size(1080, 630),
                Font = new Font("Segoe UI", 10),
                DrawMode = TabDrawMode.OwnerDrawFixed,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(150, 40)
            };

            tabMain.DrawItem += (s, e) => {
                TabControl tc = (TabControl)s;
                TabPage page = tc.TabPages[e.Index];
                Rectangle r = tc.GetTabRect(e.Index);
                bool isSelected = (e.State == DrawItemState.Selected);
                e.Graphics.FillRectangle(new SolidBrush(isSelected ? Color.White : Color.FromArgb(235, 235, 235)), r);
                Color textColor = isSelected ? Color.FromArgb(41, 128, 185) : Color.FromArgb(100, 100, 100);
                TextRenderer.DrawText(e.Graphics, page.Text, tc.Font, r, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                if (isSelected) e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(41, 128, 185)), r.X, r.Bottom - 3, r.Width, 3);
            };

            TaoTabKinhDoanh();
            TaoTabSanPham();
            TaoTabKhachHang();
            TaoTabNhanVien();
            TaoTabDanhGia();
            TaoTabTonKho();

            this.Controls.Add(tabMain);
            this.Controls.Add(pnlHeader);
        }

        private void TaoTabKinhDoanh()
        {
            tabKinhDoanh = new TabPage("💰 Kinh Doanh");
            tabKinhDoanh.BackColor = Color.White;

            Panel pnlFilter = CreateFilterPanel(out cboNam1, out cboThang1, out btnXemKinhDoanh, true);
            btnXemKinhDoanh.Click += BtnXemKinhDoanh_Click;

            // Thu nhỏ Grid một chút (Height 280) để Card không bị đè lên dữ liệu
            dgvKinhDoanh = TaoDataGridView(new Point(20, 90), new Size(1030, 280));

            // Card Doanh Thu - Tăng chiều ngang (Width 480) để chứa số tiền lớn
            RoundedPanel pnlCardDoanhThu = new RoundedPanel()
            {
                Location = new Point(30, 400),
                Size = new Size(490, 110),
                BackColor = Color.FromArgb(46, 204, 113),
                Radius = 20
            };
            pnlCardDoanhThu.Controls.Add(CreateCardContent("💰", "TỔNG DOANH THU", out lblTongDoanhThu, "0 đ"));

            // Card Đơn Hàng
            RoundedPanel pnlCardDon = new RoundedPanel()
            {
                Location = new Point(540, 400),
                Size = new Size(490, 110),
                BackColor = Color.FromArgb(52, 152, 219),
                Radius = 20
            };
            pnlCardDon.Controls.Add(CreateCardContent("📦", "TỔNG SỐ ĐƠN", out lblTongDon, "0"));

            tabKinhDoanh.Controls.AddRange(new Control[] { pnlFilter, pnlCardDoanhThu, pnlCardDon, dgvKinhDoanh });
            tabMain.TabPages.Add(tabKinhDoanh);
        }

        // ============ HELPER UI (Sửa lỗi khoảng cách và tràn chữ) ============

        private Panel CreateFilterPanel(out ComboBox cboNam, out ComboBox cboThang, out Button btnAction, bool hasMonth, int x = 20, int y = 15)
        {
            Panel pnl = new Panel() { Location = new Point(x, y), Size = new Size(1020, 65), BackColor = Color.FromArgb(248, 249, 250) };
            
            // Căn chỉnh label và combo rộng ra (tăng X)
            Label lblN = new Label() { Text = "Năm:", Location = new Point(15, 23), AutoSize = true };
            cboNam = new ComboBox() { Location = new Point(65, 20), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            TaiDanhSachNam(cboNam);
            pnl.Controls.AddRange(new Control[] { lblN, cboNam });

            cboThang = null;
            int btnX = 180;

            if (hasMonth)
            {
                Label lblT = new Label() { Text = "Tháng:", Location = new Point(190, 23), AutoSize = true };
                cboThang = new ComboBox() { Location = new Point(255, 20), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
                cboThang.Items.Add("Tất cả");
                for (int i = 1; i <= 12; i++) cboThang.Items.Add(i);
                cboThang.SelectedIndex = 0;
                pnl.Controls.AddRange(new Control[] { lblT, cboThang });
                btnX = 380; // Đẩy nút ra xa hơn
            }

            btnAction = CreateButton("Xem báo cáo", new Point(btnX, 17), Color.FromArgb(41, 128, 185));
            pnl.Controls.Add(btnAction);
            return pnl;
        }

        private Control CreateCardContent(string icon, string title, out Label lblValue, string defaultVal)
        {
            TableLayoutPanel tbl = new TableLayoutPanel() { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Color.Transparent, Padding = new Padding(10) };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));

            Label lblIcon = new Label() { Text = icon, Font = new Font("Segoe UI", 35), ForeColor = Color.White, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            Label lblT = new Label() { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(230, 230, 230), Dock = DockStyle.Bottom, AutoSize = true };

            lblValue = new Label()
            {
                Text = defaultVal,
                Font = new Font("Segoe UI", 22, FontStyle.Bold), // Tăng nhẹ font
                ForeColor = Color.White,
                AutoSize = false,       // QUAN TRỌNG: Tắt AutoSize để tránh tràn
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true     // Nếu dài quá hiện dấu ...
            };

            tbl.Controls.Add(lblIcon, 0, 0); tbl.SetRowSpan(lblIcon, 2);
            tbl.Controls.Add(lblT, 1, 0);
            tbl.Controls.Add(lblValue, 1, 1);
            return tbl;
        }

        // ============ LOGIC XỬ LÝ (Format tiền tỷ) ============

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

                        decimal tongTien = 0; int tongDon = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["Tổng Doanh thu"] != DBNull.Value) tongTien += Convert.ToDecimal(row["Tổng Doanh thu"]);
                            if (row["Số lượng đơn"] != DBNull.Value) tongDon += Convert.ToInt32(row["Số lượng đơn"]);
                        }

                        // Format số tiền (Nếu > 1 Tỷ thì ghi rút gọn)
                        if (tongTien >= 1000000000)
                            lblTongDoanhThu.Text = string.Format("{0:0.##} Tỷ VNĐ", (double)tongTien / 1000000000);
                        else
                            lblTongDoanhThu.Text = tongTien.ToString("N0") + " đ";

                        lblTongDon.Text = tongDon.ToString("N0");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // --- CÁC TAB KHÁC (GIỮ LOGIC NHƯNG CẬP NHẬT UI ĐỒNG BỘ) ---

        private void TaoTabSanPham() {
            tabSanPham = new TabPage("📦 Sản Phẩm"); tabSanPham.BackColor = Color.White;
            Panel pnl = CreateFilterPanel(out cboNam2, out cboThang2, out btnXemSanPham, true);
            btnXemSanPham.Click += (s, e) => LoadData(dgvSanPham, "sp_ThongKeSanPham", cboNam2, cboThang2);
            dgvSanPham = TaoDataGridView(new Point(20, 100), new Size(1030, 200));
            Label lbl2 = CreateSectionTitle("🔥 TOP SẢN PHẨM BÁN CHẠY", 20, 320);
            btnSanPhamBanChay = CreateButton("Xem Top Bán Chạy", new Point(20, 355), Color.FromArgb(230, 126, 34));
            btnSanPhamBanChay.Click += (s, e) => LoadDataSimple(dgvSanPhamBanChay, "sp_SanPhamBanChay");
            dgvSanPhamBanChay = TaoDataGridView(new Point(20, 400), new Size(1030, 180));
            tabSanPham.Controls.AddRange(new Control[] { pnl, dgvSanPham, lbl2, btnSanPhamBanChay, dgvSanPhamBanChay });
            tabMain.TabPages.Add(tabSanPham);
        }

        private void TaoTabKhachHang() {
            tabKhachHang = new TabPage("👥 Khách Hàng"); tabKhachHang.BackColor = Color.White; tabKhachHang.AutoScroll = true;
            Panel pnl = CreateFilterPanel(out cboNam3, out _, out btnThongKeKH, false);
            btnThongKeKH.Click += (s, e) => LoadData(dgvKhachHang, "sp_ThongKeKhachHang", cboNam3, null);
            dgvKhachHang = TaoDataGridView(new Point(20, 100), new Size(1030, 150));
            Label lbl2 = CreateSectionTitle("💎 PHÂN TÍCH TIỀM NĂNG", 20, 270);
            btnPhanTichKH = CreateButton("Phân tích ngay", new Point(20, 305), Color.FromArgb(142, 68, 173));
            btnPhanTichKH.Click += (s, e) => LoadDataSimple(dgvPhanTich, "sp_PhanTichKhachHang");
            dgvPhanTich = TaoDataGridView(new Point(20, 350), new Size(1030, 200));
            tabKhachHang.Controls.AddRange(new Control[] { pnl, dgvKhachHang, lbl2, btnPhanTichKH, dgvPhanTich });
            tabMain.TabPages.Add(tabKhachHang);
        }

        private void TaoTabNhanVien() {
            tabNhanVien = new TabPage("👨‍💼 Nhân Viên"); tabNhanVien.BackColor = Color.White;
            Panel pnl = CreateFilterPanel(out cboNam4, out cboThang4, out btnThongKeNV, true);
            btnThongKeNV.Click += (s, e) => LoadData(dgvNhanVien, "sp_ThongKeHieuSuatNhanVien", cboNam4, cboThang4);
            dgvNhanVien = TaoDataGridView(new Point(20, 100), new Size(1030, 300));
            btnTinhLuong = CreateButton("💰 Tính lương tháng này", new Point(20, 420), Color.FromArgb(46, 204, 113));
            btnTinhLuong.Size = new Size(200, 40);
            btnTinhLuong.Click += BtnTinhLuong_Click;
            tabNhanVien.Controls.AddRange(new Control[] { pnl, dgvNhanVien, btnTinhLuong });
            tabMain.TabPages.Add(tabNhanVien);
        }

        private void TaoTabDanhGia() {
            tabDanhGia = new TabPage("⭐ Đánh Giá"); tabDanhGia.BackColor = Color.White;
            Panel pnl = CreateFilterPanel(out cboNam5, out cboThang5, out btnXemDanhGia, true);
            btnXemDanhGia.Click += (s, e) => LoadData(dgvDanhGia, "sp_ThongKeDanhGia", cboNam5, cboThang5);
            dgvDanhGia = TaoDataGridView(new Point(20, 100), new Size(1030, 450));
            tabDanhGia.Controls.AddRange(new Control[] { pnl, dgvDanhGia });
            tabMain.TabPages.Add(tabDanhGia);
        }

        private void TaoTabTonKho() {
            tabTonKho = new TabPage("📉 Tồn Kho"); tabTonKho.BackColor = Color.White;
            btnXemTonKho = CreateButton("🔍 Quét kho hàng", new Point(20, 20), Color.FromArgb(231, 76, 60));
            btnXemTonKho.Click += (s, e) => {
                using (SqlConnection c = Connection.GetConnection()) {
                    DataTable dt = new DataTable(); new SqlDataAdapter("SELECT * FROM f_TonKhoThap()", c).Fill(dt);
                    dgvTonKho.DataSource = dt;
                }
            };
            dgvTonKho = TaoDataGridView(new Point(20, 70), new Size(1030, 480));
            tabTonKho.Controls.AddRange(new Control[] { btnXemTonKho, dgvTonKho });
            tabMain.TabPages.Add(tabTonKho);
        }

        private void BtnTinhLuong_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Xác nhận tính lương cho nhân viên?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    DataTable dtNV = new DataTable(); new SqlDataAdapter("SELECT MANV FROM NHANVIEN", conn).Fill(dtNV);
                    foreach (DataRow r in dtNV.Rows) {
                        using (SqlCommand cmd = new SqlCommand("sp_Sub_TinhLuong", conn)) {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MANV", r["MANV"]);
                            cmd.Parameters.AddWithValue("@Nam", DateTime.Now.Year);
                            cmd.Parameters.AddWithValue("@Thang", DateTime.Now.Month);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Đã tính lương thành công!");
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LoadData(DataGridView dgv, string sp, ComboBox cbN, ComboBox cbT) {
            try {
                using (SqlConnection c = Connection.GetConnection()) {
                    SqlCommand cmd = new SqlCommand(sp, c) { CommandType = CommandType.StoredProcedure };
                    if (cbN != null) cmd.Parameters.AddWithValue("@Nam", Convert.ToInt32(cbN.SelectedItem));
                    if (cbT != null) cmd.Parameters.AddWithValue("@Thang", cbT.SelectedIndex == 0 ? (object)DBNull.Value : Convert.ToInt32(cbT.SelectedItem));
                    DataTable dt = new DataTable(); new SqlDataAdapter(cmd).Fill(dt); dgv.DataSource = dt;
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LoadDataSimple(DataGridView dgv, string sp) {
            try {
                using (SqlConnection c = Connection.GetConnection()) {
                    DataTable dt = new DataTable(); new SqlDataAdapter(sp, c).Fill(dt); dgv.DataSource = dt;
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // --- COMMON HELPERS ---
        private DataGridView TaoDataGridView(Point loc, Size sz) {
            return new DataGridView() {
                Location = loc, Size = sz, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false,
                AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false, RowTemplate = { Height = 35 },
                ColumnHeadersHeight = 40, ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 246, 247), Font = new Font("Segoe UI", 10, FontStyle.Bold) }
            };
        }

        private Button CreateButton(string text, Point loc, Color bg) {
            return new Button() { Text = text, Location = loc, Size = new Size(160, 32), BackColor = bg, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        }

        private Label CreateSectionTitle(string text, int x, int y) {
            return new Label() { Text = text, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64) };
        }

        private void TaiDanhSachNam(ComboBox cbo) {
            for (int i = DateTime.Now.Year; i >= DateTime.Now.Year - 4; i--) cbo.Items.Add(i);
            if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
        }
    }

    public class RoundedPanel : Panel
    {
        public int Radius { get; set; } = 20;
        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = new GraphicsPath()) {
                Rectangle r = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                path.AddArc(r.X, r.Y, Radius, Radius, 180, 90);
                path.AddArc(r.Right - Radius, r.Y, Radius, Radius, 270, 90);
                path.AddArc(r.Right - Radius, r.Bottom - Radius, Radius, Radius, 0, 90);
                path.AddArc(r.X, r.Bottom - Radius, Radius, Radius, 90, 90);
                path.CloseAllFigures();
                this.Region = new Region(path);
                using (Pen p = new Pen(this.BackColor, 1)) e.Graphics.DrawPath(p, path);
            }
        }
    }
}