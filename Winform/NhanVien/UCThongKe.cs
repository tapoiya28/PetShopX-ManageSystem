using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    /// <summary>
    /// UC dành cho Quản lý để xem các báo cáo thống kê toàn hệ thống
    /// KỊCH BẢN 4: THỐNG KÊ TOÀN HỆ THỐNG
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
            this.BackColor = Color.FromArgb(236, 240, 245);
            this.AutoScroll = true;
            
            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            // Tiêu đề
            Label lblTitle = new Label()
            {
                Text = "📊 THỐNG KÊ TOÀN HỆ THỐNG",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(30, 15),
                AutoSize = true
            };
            
            // Tab Control
            tabMain = new TabControl()
            {
                Location = new Point(20, 60),
                Size = new Size(1060, 670),
                Font = new Font("Segoe UI", 10)
            };
            
            // Tạo các tab
            TaoTabKinhDoanh();
            TaoTabSanPham();
            TaoTabKhachHang();
            TaoTabNhanVien();
            TaoTabDanhGia();
            TaoTabTonKho();
            
            this.Controls.Add(lblTitle);
            this.Controls.Add(tabMain);
        }

        // ============ TAB 1: TÌNH HÌNH KINH DOANH ============
        private void TaoTabKinhDoanh()
        {
            tabKinhDoanh = new TabPage("💰 Kinh Doanh");
            tabKinhDoanh.BackColor = Color.White;
            
            Label lblNam = new Label() { Text = "Năm:", Location = new Point(30, 25), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cboNam1 = new ComboBox() { Location = new Point(90, 22), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            TaiDanhSachNam(cboNam1);
            
            Label lblThang = new Label() { Text = "Tháng:", Location = new Point(220, 25), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cboThang1 = new ComboBox() { Location = new Point(290, 22), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cboThang1.Items.Add("Tất cả");
            for (int i = 1; i <= 12; i++) cboThang1.Items.Add(i);
            cboThang1.SelectedIndex = 0;
            
            btnXemKinhDoanh = new Button()
            {
                Text = "🔍 Xem báo cáo",
                Location = new Point(420, 20),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXemKinhDoanh.FlatAppearance.BorderSize = 0;
            btnXemKinhDoanh.Click += BtnXemKinhDoanh_Click;
            
            dgvKinhDoanh = TaoDataGridView(new Point(30, 70), new Size(1000, 350));
            
            // Tổng kết
            lblTongDoanhThu = new Label()
            {
                Text = "Tổng doanh thu: 0 đ",
                Location = new Point(30, 440),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96)
            };
            
            lblTongDon = new Label()
            {
                Text = "Tổng số đơn: 0",
                Location = new Point(400, 440),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185)
            };
            
            tabKinhDoanh.Controls.AddRange(new Control[] { lblNam, cboNam1, lblThang, cboThang1, btnXemKinhDoanh, dgvKinhDoanh, lblTongDoanhThu, lblTongDon });
            tabMain.TabPages.Add(tabKinhDoanh);
        }

        // ============ TAB 2: THỐNG KÊ SẢN PHẨM ============
        private void TaoTabSanPham()
        {
            tabSanPham = new TabPage("📦 Sản Phẩm");
            tabSanPham.BackColor = Color.White;
            
            // Phần thống kê theo tháng/năm
            Label lblSection1 = new Label() { Text = "THỐNG KÊ SẢN PHẨM BÁN ĐƯỢC", Location = new Point(30, 15), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80) };
            
            Label lblNam = new Label() { Text = "Năm:", Location = new Point(30, 50), AutoSize = true };
            cboNam2 = new ComboBox() { Location = new Point(90, 47), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            TaiDanhSachNam(cboNam2);
            
            Label lblThang = new Label() { Text = "Tháng:", Location = new Point(220, 50), AutoSize = true };
            cboThang2 = new ComboBox() { Location = new Point(290, 47), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cboThang2.Items.Add("Tất cả");
            for (int i = 1; i <= 12; i++) cboThang2.Items.Add(i);
            cboThang2.SelectedIndex = 0;
            
            btnXemSanPham = new Button()
            {
                Text = "📊 Xem thống kê",
                Location = new Point(420, 45),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXemSanPham.FlatAppearance.BorderSize = 0;
            btnXemSanPham.Click += BtnXemSanPham_Click;
            
            dgvSanPham = TaoDataGridView(new Point(30, 95), new Size(1000, 200));
            
            // Phần sản phẩm bán chạy
            Label lblSection2 = new Label() { Text = "SẢN PHẨM BÁN CHẠY", Location = new Point(30, 315), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(230, 126, 34) };
            
            btnSanPhamBanChay = new Button()
            {
                Text = "🔥 Top Sản Phẩm Bán Chạy",
                Location = new Point(30, 350),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSanPhamBanChay.FlatAppearance.BorderSize = 0;
            btnSanPhamBanChay.Click += BtnSanPhamBanChay_Click;
            
            dgvSanPhamBanChay = TaoDataGridView(new Point(30, 400), new Size(1000, 200));
            
            tabSanPham.Controls.AddRange(new Control[] { lblSection1, lblNam, cboNam2, lblThang, cboThang2, btnXemSanPham, dgvSanPham, lblSection2, btnSanPhamBanChay, dgvSanPhamBanChay });
            tabMain.TabPages.Add(tabSanPham);
        }

        // ============ TAB 3: PHÂN TÍCH KHÁCH HÀNG ============
        private void TaoTabKhachHang()
        {
            tabKhachHang = new TabPage("👥 Khách Hàng");
            tabKhachHang.BackColor = Color.White;
            tabKhachHang.AutoScroll = true;
            
            // Thống kê khách hàng cơ bản
            Label lblSection1 = new Label() { Text = "THỐNG KÊ KHÁCH HÀNG", Location = new Point(30, 15), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80) };
            
            Label lblNam = new Label() { Text = "Năm:", Location = new Point(30, 50), AutoSize = true };
            cboNam3 = new ComboBox() { Location = new Point(90, 47), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            TaiDanhSachNam(cboNam3);
            
            btnThongKeKH = new Button()
            {
                Text = "📈 Xem thống kê",
                Location = new Point(220, 45),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThongKeKH.FlatAppearance.BorderSize = 0;
            btnThongKeKH.Click += BtnThongKeKH_Click;
            
            dgvKhachHang = TaoDataGridView(new Point(30, 95), new Size(1000, 150));
            
            // Phân tích khách hàng tiềm năng
            Label lblSection2 = new Label() { Text = "PHÂN TÍCH KHÁCH HÀNG TIỀM NĂNG", Location = new Point(30, 265), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(142, 68, 173) };
            
            btnPhanTichKH = new Button()
            {
                Text = "🎯 Phân tích khách hàng",
                Location = new Point(30, 300),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPhanTichKH.FlatAppearance.BorderSize = 0;
            btnPhanTichKH.Click += BtnPhanTichKH_Click;
            
            dgvPhanTich = TaoDataGridView(new Point(30, 350), new Size(1000, 150));
            
            // Thống kê cấp bậc
            Label lblSection3 = new Label() { Text = "THỐNG KÊ THEO CẤP BẬC", Location = new Point(30, 520), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(39, 174, 96) };
            
            btnThongKeCapBac = new Button()
            {
                Text = "🏆 Xem theo cấp bậc",
                Location = new Point(30, 555),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThongKeCapBac.FlatAppearance.BorderSize = 0;
            btnThongKeCapBac.Click += BtnThongKeCapBac_Click;
            
            dgvCapBac = TaoDataGridView(new Point(30, 605), new Size(1000, 150));
            
            tabKhachHang.Controls.AddRange(new Control[] { lblSection1, lblNam, cboNam3, btnThongKeKH, dgvKhachHang, lblSection2, btnPhanTichKH, dgvPhanTich, lblSection3, btnThongKeCapBac, dgvCapBac });
            tabMain.TabPages.Add(tabKhachHang);
        }

        // ============ TAB 4: HIỆU SUẤT NHÂN VIÊN ============
        private void TaoTabNhanVien()
        {
            tabNhanVien = new TabPage("👨‍💼 Nhân Viên");
            tabNhanVien.BackColor = Color.White;
            
            Label lblSection = new Label() { Text = "THỐNG KÊ HIỆU SUẤT NHÂN VIÊN", Location = new Point(30, 15), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80) };
            
            Label lblNam = new Label() { Text = "Năm:", Location = new Point(30, 50), AutoSize = true };
            cboNam4 = new ComboBox() { Location = new Point(90, 47), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            TaiDanhSachNam(cboNam4);
            
            Label lblThang = new Label() { Text = "Tháng:", Location = new Point(220, 50), AutoSize = true };
            cboThang4 = new ComboBox() { Location = new Point(290, 47), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cboThang4.Items.Add("Tất cả");
            for (int i = 1; i <= 12; i++) cboThang4.Items.Add(i);
            cboThang4.SelectedIndex = 0;
            
            btnThongKeNV = new Button()
            {
                Text = "📊 Xem hiệu suất",
                Location = new Point(420, 45),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThongKeNV.FlatAppearance.BorderSize = 0;
            btnThongKeNV.Click += BtnThongKeNV_Click;
            
            dgvNhanVien = TaoDataGridView(new Point(30, 95), new Size(1000, 250));
            
            // Tính lương
            Label lblSection2 = new Label() { Text = "TÍNH LƯƠNG NHÂN VIÊN", Location = new Point(30, 365), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(39, 174, 96) };
            
            btnTinhLuong = new Button()
            {
                Text = " Tính lương tháng này",
                Location = new Point(30, 400),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTinhLuong.FlatAppearance.BorderSize = 0;
            btnTinhLuong.Click += BtnTinhLuong_Click;
            
            tabNhanVien.Controls.AddRange(new Control[] { lblSection, lblNam, cboNam4, lblThang, cboThang4, btnThongKeNV, dgvNhanVien, lblSection2, btnTinhLuong });
            tabMain.TabPages.Add(tabNhanVien);
        }

        // ============ TAB 5: THỐNG KÊ ĐÁNH GIÁ ============
        private void TaoTabDanhGia()
        {
            tabDanhGia = new TabPage("⭐ Đánh Giá");
            tabDanhGia.BackColor = Color.White;
            
            Label lblSection = new Label() { Text = "THỐNG KÊ ĐÁNH GIÁ DỊCH VỤ", Location = new Point(30, 15), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80) };
            
            Label lblNam = new Label() { Text = "Năm:", Location = new Point(30, 50), AutoSize = true };
            cboNam5 = new ComboBox() { Location = new Point(90, 47), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            TaiDanhSachNam(cboNam5);
            
            Label lblThang = new Label() { Text = "Tháng:", Location = new Point(220, 50), AutoSize = true };
            cboThang5 = new ComboBox() { Location = new Point(290, 47), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cboThang5.Items.Add("Tất cả");
            for (int i = 1; i <= 12; i++) cboThang5.Items.Add(i);
            cboThang5.SelectedIndex = 0;
            
            btnXemDanhGia = new Button()
            {
                Text = " Xem đánh giá",
                Location = new Point(420, 45),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXemDanhGia.FlatAppearance.BorderSize = 0;
            btnXemDanhGia.Click += BtnXemDanhGia_Click;
            
            dgvDanhGia = TaoDataGridView(new Point(30, 95), new Size(1000, 400));
            
            tabDanhGia.Controls.AddRange(new Control[] { lblSection, lblNam, cboNam5, lblThang, cboThang5, btnXemDanhGia, dgvDanhGia });
            tabMain.TabPages.Add(tabDanhGia);
        }

        // ============ TAB 6: TỒN KHO THẤP ============
        private void TaoTabTonKho()
        {
            tabTonKho = new TabPage("📉 Tồn Kho");
            tabTonKho.BackColor = Color.White;
            
            Label lblSection = new Label() { Text = "CẢNH BÁO TỒN KHO THẤP", Location = new Point(30, 15), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(231, 76, 60) };
            
            Label lblNote = new Label() 
            { 
                Text = " Danh sách sản phẩm có tồn kho dưới mức tối thiểu cần nhập hàng", 
                Location = new Point(30, 50), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            
            btnXemTonKho = new Button()
            {
                Text = "🔍 Kiểm tra tồn kho",
                Location = new Point(30, 85),
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXemTonKho.FlatAppearance.BorderSize = 0;
            btnXemTonKho.Click += BtnXemTonKho_Click;
            
            dgvTonKho = TaoDataGridView(new Point(30, 135), new Size(1000, 450));
            
            tabTonKho.Controls.AddRange(new Control[] { lblSection, lblNote, btnXemTonKho, dgvTonKho });
            tabMain.TabPages.Add(tabTonKho);
        }

        // ============ HÀM PHỤ TRỢ ============
        
        private DataGridView TaoDataGridView(Point location, Size size)
        {
            DataGridView dgv = new DataGridView()
            {
                Location = location,
                Size = size,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 35;
            
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgv.RowTemplate.Height = 30;
            
            return dgv;
        }

        private void TaiDanhSachNam(ComboBox cbo)
        {
            int namHienTai = DateTime.Now.Year;
            for (int i = namHienTai; i >= namHienTai - 5; i--)
            {
                cbo.Items.Add(i);
            }
            cbo.SelectedIndex = 0;
        }

        // ============ XỬ LÝ SỰ KIỆN ============

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

                        // Tính tổng
                        decimal tongDoanhThu = 0;
                        int tongDon = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            tongDoanhThu += Convert.ToDecimal(row["Tổng Doanh thu"]);
                            tongDon += Convert.ToInt32(row["Số lượng đơn"]);
                        }
                        
                        lblTongDoanhThu.Text = $"Tổng doanh thu: {tongDoanhThu:N0} đ";
                        lblTongDon.Text = $"Tổng số đơn: {tongDon}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXemSanPham_Click(object sender, EventArgs e)
        {
            try
            {
                int nam = Convert.ToInt32(cboNam2.SelectedItem);
                int? thang = cboThang2.SelectedIndex == 0 ? (int?)null : Convert.ToInt32(cboThang2.SelectedItem);

                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_ThongKeSanPham", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Thang", (object)thang ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Nam", nam);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvSanPham.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSanPhamBanChay_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_SanPhamBanChay", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvSanPhamBanChay.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThongKeKH_Click(object sender, EventArgs e)
        {
            try
            {
                int nam = Convert.ToInt32(cboNam3.SelectedItem);

                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_ThongKeKhachHang", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Nam", nam);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvKhachHang.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPhanTichKH_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_PhanTichKhachHang", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvPhanTich.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThongKeCapBac_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            TENCAPBAC AS [Cấp Bậc],
                            COUNT(*) AS [Số Lượng KH],
                            CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM KHACHHANG) AS DECIMAL(5,2)) AS [Tỷ Lệ %]
                        FROM KHACHHANG
                        GROUP BY TENCAPBAC
                        ORDER BY COUNT(*) DESC";
                    
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvCapBac.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThongKeNV_Click(object sender, EventArgs e)
        {
            try
            {
                int nam = Convert.ToInt32(cboNam4.SelectedItem);
                int? thang = cboThang4.SelectedIndex == 0 ? (int?)null : Convert.ToInt32(cboThang4.SelectedItem);

                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_ThongKeHieuSuatNhanVien", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Nam", nam);
                        cmd.Parameters.AddWithValue("@Thang", (object)thang ?? DBNull.Value);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvNhanVien.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnTinhLuong_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn tính lương cho tất cả nhân viên tháng này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    
                    // Lấy tất cả nhân viên
                    string queryNV = "SELECT MANV FROM NHANVIEN";
                    SqlDataAdapter da = new SqlDataAdapter(queryNV, conn);
                    DataTable dtNV = new DataTable();
                    da.Fill(dtNV);

                    int thanhCong = 0;
                    foreach (DataRow row in dtNV.Rows)
                    {
                        int maNV = Convert.ToInt32(row["MANV"]);
                        
                        using (SqlCommand cmd = new SqlCommand("sp_Sub_TinhLuong", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MANV", maNV);
                            cmd.Parameters.AddWithValue("@Nam", DateTime.Now.Year);
                            cmd.Parameters.AddWithValue("@Thang", DateTime.Now.Month);
                            
                            try
                            {
                                cmd.ExecuteNonQuery();
                                thanhCong++;
                            }
                            catch { }
                        }
                    }

                    MessageBox.Show(
                        $"Đã tính lương thành công cho {thanhCong}/{dtNV.Rows.Count} nhân viên!",
                        "Hoàn thành",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXemDanhGia_Click(object sender, EventArgs e)
        {
            try
            {
                int nam = Convert.ToInt32(cboNam5.SelectedItem);
                int? thang = cboThang5.SelectedIndex == 0 ? (int?)null : Convert.ToInt32(cboThang5.SelectedItem);

                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_ThongKeDanhGia", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Nam", nam);
                        cmd.Parameters.AddWithValue("@Thang", (object)thang ?? DBNull.Value);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvDanhGia.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXemTonKho_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT * FROM f_TonKhoThap()";
                    
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvTonKho.DataSource = dt;

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Tất cả sản phẩm đều có tồn kho đủ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Có {dt.Rows.Count} sản phẩm cần nhập hàng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
