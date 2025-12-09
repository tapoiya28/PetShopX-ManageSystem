using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    /// <summary>
    /// UC dành cho Bác sĩ/Nhân viên để TẠO CA KHÁM MỚI và thêm triệu chứng/chẩn đoán/toa thuốc
    /// KỊCH BẢN 3: KHÁM BỆNH
    /// </summary>
    public class UCKhamBenh : UserControl
    {
        // Panel chính
        private Panel pnlTaoMoi;
        private Panel pnlThongTinKham;
        private Panel pnlTrieuChung;
        private Panel pnlChanDoan;
        private Panel pnlToaThuoc;

        // Controls
        private Label lblTitle;
        
        // Controls Tạo ca khám mới
        private ComboBox cboKhachHang;
        private ComboBox cboThuCung;
        private DateTimePicker dtpNgayKhamMoi;
        private Button btnLuuCaKham;
        
        // Thông tin Ca khám hiện tại
        private Label lblMaKB;
        private Label lblTenThuCung;
        private Label lblLoaiThuCung;
        private Label lblChuSoHuu;
        private Label lblNgayKham;
        private TextBox txtMaKB;
        private TextBox txtTenTC;
        private TextBox txtLoai;
        private TextBox txtChuSoHuu;
        private DateTimePicker dtpNgayKhamInfo;
        
        // Triệu chứng
        private DataGridView dgvTrieuChung;
        private TextBox txtTrieuChung;
        private Button btnThemTrieuChung;
        private Button btnXoaTrieuChung;
        
        // Chẩn đoán
        private DataGridView dgvChanDoan;
        private TextBox txtChanDoan;
        private Button btnThemChanDoan;
        private Button btnXoaChanDoan;
        
        // Toa thuốc
        private DataGridView dgvToaThuoc;
        private ComboBox cboThuoc;
        private NumericUpDown nudSoLuong;
        private TextBox txtGhiChuToa;
        private Button btnThemThuoc;
        private Button btnXoaThuoc;
        private Button btnLuuToaThuoc;
        
        // Biến lưu trạng thái
        private int maKBHienTai = -1;
        private int maTCHienTai = -1;

        public UCKhamBenh()
        {
            this.Size = new Size(1100, 750);
            this.BackColor = Color.FromArgb(236, 240, 245);
            this.AutoScroll = true;
            
            TaoGiaoDien();
            TaiDanhSachThuoc();
            LoadKhachHang();
        }

        private void TaoGiaoDien()
        {
            // ==== TIÊU ĐỀ ====
            lblTitle = new Label()
            {
                Text = "🏥 TẠO CA KHÁM MỚI",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Location = new Point(30, 15),
                AutoSize = true
            };
            
            // ==== PANEL TẠO CA KHÁM MỚI ====
            pnlTaoMoi = TaoPanelTaoMoi();
            
            // ==== PANEL THÔNG TIN CA KHÁM ====
            pnlThongTinKham = TaoPanelThongTin();
            
            // ==== PANEL TRIỆU CHỨNG ====
            pnlTrieuChung = TaoPanelTrieuChung();
            
            // ==== PANEL CHẨN ĐOÁN ====
            pnlChanDoan = TaoPanelChanDoan();
            
            // ==== PANEL TOA THUỐC ====
            pnlToaThuoc = TaoPanelToaThuoc();
            
            this.Controls.AddRange(new Control[] { lblTitle, pnlTaoMoi, pnlThongTinKham, pnlTrieuChung, pnlChanDoan, pnlToaThuoc });
            
            // Khóa các panel chỉnh sửa ban đầu
            KhoaChinhSua(true);
        }

        private void KhoaChinhSua(bool khoa)
        {
            // Khóa/mở các nút thêm/xóa
            btnThemTrieuChung.Enabled = !khoa;
            btnXoaTrieuChung.Enabled = !khoa;
            btnThemChanDoan.Enabled = !khoa;
            btnXoaChanDoan.Enabled = !khoa;
            btnThemThuoc.Enabled = !khoa;
            btnXoaThuoc.Enabled = !khoa;
            btnLuuToaThuoc.Enabled = !khoa;
            
            txtTrieuChung.Enabled = !khoa;
            txtChanDoan.Enabled = !khoa;
            cboThuoc.Enabled = !khoa;
            nudSoLuong.Enabled = !khoa;
            txtGhiChuToa.Enabled = !khoa;
        }

        private Panel TaoPanelTaoMoi()
        {
            Panel panel = new Panel()
            {
                Location = new Point(20, 60),
                Size = new Size(1060, 140),
                BackColor = Color.FromArgb(255, 250, 240),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Label lblSection = new Label() 
            { 
                Text = "✨ THÔNG TIN CA KHÁM MỚI", 
                Location = new Point(15, 10), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 11, FontStyle.Bold), 
                ForeColor = Color.FromArgb(230, 126, 34) 
            };
            
            Label lblKH = new Label() 
            { 
                Text = "Chọn khách hàng:", 
                Location = new Point(30, 45), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 9) 
            };
            
            cboKhachHang = new ComboBox() 
            { 
                Location = new Point(160, 42), 
                Width = 300, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9) 
            };
            cboKhachHang.SelectedIndexChanged += CboKhachHang_SelectedIndexChanged;
            
            Label lblTC = new Label() 
            { 
                Text = "Chọn thú cưng:", 
                Location = new Point(520, 45), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 9) 
            };
            
            cboThuCung = new ComboBox() 
            { 
                Location = new Point(640, 42), 
                Width = 300, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9) 
            };
            
            Label lblNgay = new Label()
            {
                Text = "Ngày khám:",
                Location = new Point(30, 85),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            
            dtpNgayKhamMoi = new DateTimePicker()
            {
                Location = new Point(160, 82),
                Width = 150,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9),
                Value = DateTime.Now
            };
            
            btnLuuCaKham = new Button()
            {
                Text = "💾 Lưu Ca Khám & Bắt Đầu Khám",
                Location = new Point(730, 78),
                Size = new Size(210, 32),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLuuCaKham.FlatAppearance.BorderSize = 0;
            btnLuuCaKham.Click += BtnLuuCaKham_Click;
            
            panel.Controls.AddRange(new Control[] { lblSection, lblKH, cboKhachHang, lblTC, cboThuCung, lblNgay, dtpNgayKhamMoi, btnLuuCaKham });
            
            return panel;
        }

        private Panel TaoPanelThongTin()
        {
            Panel panel = new Panel()
            {
                Location = new Point(20, 210),
                Size = new Size(1060, 120),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Label lblSectionTitle = new Label() { Text = "📋 THÔNG TIN CA KHÁM", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80) };
            
            lblMaKB = new Label() { Text = "Mã khám:", Location = new Point(30, 45), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtMaKB = new TextBox() { Location = new Point(120, 42), Width = 120, ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 9) };
            
            lblTenThuCung = new Label() { Text = "Tên thú cưng:", Location = new Point(270, 45), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtTenTC = new TextBox() { Location = new Point(380, 42), Width = 150, ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 9) };
            
            lblLoaiThuCung = new Label() { Text = "Loại:", Location = new Point(560, 45), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtLoai = new TextBox() { Location = new Point(610, 42), Width = 120, ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 9) };
            
            lblChuSoHuu = new Label() { Text = "Chủ sở hữu:", Location = new Point(30, 80), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtChuSoHuu = new TextBox() { Location = new Point(120, 77), Width = 200, ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 9) };
            
            lblNgayKham = new Label() { Text = "Ngày khám:", Location = new Point(350, 80), AutoSize = true, Font = new Font("Segoe UI", 9) };
            dtpNgayKhamInfo = new DateTimePicker() { Location = new Point(450, 77), Width = 150, Enabled = false, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9) };
            
            panel.Controls.AddRange(new Control[] { 
                lblSectionTitle, 
                lblMaKB, txtMaKB, 
                lblTenThuCung, txtTenTC, 
                lblLoaiThuCung, txtLoai,
                lblChuSoHuu, txtChuSoHuu, 
                lblNgayKham, dtpNgayKhamInfo 
            });
            
            return panel;
        }

        private Panel TaoPanelTrieuChung()
        {
            Panel panel = new Panel()
            {
                Location = new Point(20, 340),
                Size = new Size(520, 280),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Label lblSection = new Label() { Text = "🩺 TRIỆU CHỨNG", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(192, 57, 43) };
            
            dgvTrieuChung = new DataGridView()
            {
                Location = new Point(15, 45),
                Size = new Size(490, 150),
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            txtTrieuChung = new TextBox() { Location = new Point(15, 210), Width = 380, PlaceholderText = "Nhập triệu chứng...", Font = new Font("Segoe UI", 9) };
            
            btnThemTrieuChung = new Button()
            {
                Text = "Thêm",
                Location = new Point(405, 208),
                Size = new Size(50, 25),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThemTrieuChung.FlatAppearance.BorderSize = 0;
            btnThemTrieuChung.Click += BtnThemTrieuChung_Click;
            
            btnXoaTrieuChung = new Button()
            {
                Text = "Xóa",
                Location = new Point(460, 208),
                Size = new Size(45, 25),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXoaTrieuChung.FlatAppearance.BorderSize = 0;
            btnXoaTrieuChung.Click += BtnXoaTrieuChung_Click;
            
            panel.Controls.AddRange(new Control[] { lblSection, dgvTrieuChung, txtTrieuChung, btnThemTrieuChung, btnXoaTrieuChung });
            return panel;
        }

        private Panel TaoPanelChanDoan()
        {
            Panel panel = new Panel()
            {
                Location = new Point(560, 340),
                Size = new Size(520, 280),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Label lblSection = new Label() { Text = "📝 CHẨN ĐOÁN", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(142, 68, 173) };
            
            dgvChanDoan = new DataGridView()
            {
                Location = new Point(15, 45),
                Size = new Size(490, 150),
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            txtChanDoan = new TextBox() { Location = new Point(15, 210), Width = 380, PlaceholderText = "Nhập chẩn đoán...", Font = new Font("Segoe UI", 9) };
            
            btnThemChanDoan = new Button()
            {
                Text = "Thêm",
                Location = new Point(405, 208),
                Size = new Size(50, 25),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThemChanDoan.FlatAppearance.BorderSize = 0;
            btnThemChanDoan.Click += BtnThemChanDoan_Click;
            
            btnXoaChanDoan = new Button()
            {
                Text = "Xóa",
                Location = new Point(460, 208),
                Size = new Size(45, 25),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXoaChanDoan.FlatAppearance.BorderSize = 0;
            btnXoaChanDoan.Click += BtnXoaChanDoan_Click;
            
            panel.Controls.AddRange(new Control[] { lblSection, dgvChanDoan, txtChanDoan, btnThemChanDoan, btnXoaChanDoan });
            return panel;
        }

        private Panel TaoPanelToaThuoc()
        {
            Panel panel = new Panel()
            {
                Location = new Point(20, 630),
                Size = new Size(1060, 330),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Label lblSection = new Label() { Text = "💊 TOA THUỐC", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(39, 174, 96) };
            
            dgvToaThuoc = new DataGridView()
            {
                Location = new Point(15, 45),
                Size = new Size(1030, 150),
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            // Controls thêm thuốc
            Label lblThuoc = new Label() { Text = "Thuốc:", Location = new Point(15, 210), AutoSize = true, Font = new Font("Segoe UI", 9) };
            cboThuoc = new ComboBox() { Location = new Point(80, 207), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            
            Label lblSoLuong = new Label() { Text = "SL:", Location = new Point(350, 210), AutoSize = true, Font = new Font("Segoe UI", 9) };
            nudSoLuong = new NumericUpDown() { Location = new Point(390, 207), Width = 70, Minimum = 1, Maximum = 999, Value = 1, Font = new Font("Segoe UI", 9) };
            
            btnThemThuoc = new Button()
            {
                Text = "➕ Thêm",
                Location = new Point(480, 205),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThemThuoc.FlatAppearance.BorderSize = 0;
            btnThemThuoc.Click += BtnThemThuoc_Click;
            
            btnXoaThuoc = new Button()
            {
                Text = "➖ Xóa",
                Location = new Point(570, 205),
                Size = new Size(70, 28),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXoaThuoc.FlatAppearance.BorderSize = 0;
            btnXoaThuoc.Click += BtnXoaThuoc_Click;
            
            Label lblGhiChu = new Label() { Text = "Lời dặn BS:", Location = new Point(15, 250), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtGhiChuToa = new TextBox() { Location = new Point(110, 247), Width = 530, Height = 60, Multiline = true, PlaceholderText = "Ghi chú cho toa thuốc...", Font = new Font("Segoe UI", 9) };
            
            btnLuuToaThuoc = new Button()
            {
                Text = "💾 LƯU TOA THUỐC",
                Location = new Point(660, 255),
                Size = new Size(180, 45),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLuuToaThuoc.FlatAppearance.BorderSize = 0;
            btnLuuToaThuoc.Click += BtnLuuToaThuoc_Click;
            
            panel.Controls.AddRange(new Control[] { 
                lblSection, dgvToaThuoc, 
                lblThuoc, cboThuoc, 
                lblSoLuong, nudSoLuong, 
                btnThemThuoc, btnXoaThuoc,
                lblGhiChu, txtGhiChuToa,
                btnLuuToaThuoc 
            });
            
            return panel;
        }

        // ============ SỰ KIỆN ============

        private void LoadTrieuChung()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT ROW_NUMBER() OVER(ORDER BY TENTRIEUCHUNG) AS [STT], TENTRIEUCHUNG AS [Triệu Chứng] FROM TRIEUCHUNG WHERE MAKB = @MAKB";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@MAKB", maKBHienTai);
                    
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvTrieuChung.DataSource = dt;
                    
                    if (dgvTrieuChung.Columns.Contains("STT"))
                        dgvTrieuChung.Columns["STT"].Width = 50;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load triệu chứng: " + ex.Message);
            }
        }

        private void LoadChanDoan()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT ROW_NUMBER() OVER(ORDER BY TENCHANDOAN) AS [STT], TENCHANDOAN AS [Chẩn Đoán] FROM CHANDOAN WHERE MAKB = @MAKB";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@MAKB", maKBHienTai);
                    
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvChanDoan.DataSource = dt;
                    
                    if (dgvChanDoan.Columns.Contains("STT"))
                        dgvChanDoan.Columns["STT"].Width = 50;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load chẩn đoán: " + ex.Message);
            }
        }

        private void LoadToaThuoc()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            CTT.MATT,
                            SP.TENSP AS [Tên Thuốc],
                            CTT.SOLUONG AS [Số Lượng],
                            T.DONVI AS [Đơn Vị]
                        FROM TOATHUOC TT
                        JOIN CHITIETTOATHUOC CTT ON TT.MATT = CTT.MATT
                        JOIN THUOC T ON CTT.MATHUOC = T.MATHUOC
                        JOIN SANPHAM SP ON T.MATHUOC = SP.MASP
                        WHERE TT.MAKB = @MAKB";
                    
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@MAKB", maKBHienTai);
                    
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvToaThuoc.DataSource = dt;
                    
                    if (dgvToaThuoc.Columns.Contains("MATT"))
                        dgvToaThuoc.Columns["MATT"].Visible = false;
                        
                    // Load ghi chú toa thuốc
                    string queryGhiChu = "SELECT TOP 1 GHICHU FROM TOATHUOC WHERE MAKB = @MAKB";
                    using (SqlCommand cmd = new SqlCommand(queryGhiChu, conn))
                    {
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        object result = cmd.ExecuteScalar();
                        txtGhiChuToa.Text = result != DBNull.Value && result != null ? result.ToString() : "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load toa thuốc: " + ex.Message);
            }
        }

        private void TaiDanhSachThuoc()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT T.MATHUOC, SP.TENSP 
                        FROM THUOC T
                        JOIN SANPHAM SP ON T.MATHUOC = SP.MASP
                        WHERE SP.LOAI = N'Thuốc' AND SP.TONKHO > 0";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    
                    cboThuoc.DataSource = dt;
                    cboThuoc.DisplayMember = "TENSP";
                    cboThuoc.ValueMember = "MATHUOC";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load danh sách thuốc: " + ex.Message);
            }
        }

        private void BtnThemTrieuChung_Click(object sender, EventArgs e)
        {
            if (maKBHienTai == -1)
            {
                MessageBox.Show("Vui lòng chọn ca khám!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string trieuChung = txtTrieuChung.Text.Trim();
            if (string.IsNullOrEmpty(trieuChung))
            {
                MessageBox.Show("Vui lòng nhập triệu chứng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO TRIEUCHUNG (MAKB, TENTRIEUCHUNG) VALUES (@MAKB, @TENTRIEUCHUNG)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        cmd.Parameters.AddWithValue("@TENTRIEUCHUNG", trieuChung);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                txtTrieuChung.Clear();
                LoadTrieuChung();
                MessageBox.Show("Đã thêm triệu chứng!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoaTrieuChung_Click(object sender, EventArgs e)
        {
            if (dgvTrieuChung.SelectedRows.Count == 0) return;
            
            string trieuChung = dgvTrieuChung.SelectedRows[0].Cells["Triệu Chứng"].Value.ToString();
            
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM TRIEUCHUNG WHERE MAKB = @MAKB AND TENTRIEUCHUNG = @TENTRIEUCHUNG";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        cmd.Parameters.AddWithValue("@TENTRIEUCHUNG", trieuChung);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LoadTrieuChung();
                MessageBox.Show("Đã xóa triệu chứng!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThemChanDoan_Click(object sender, EventArgs e)
        {
            if (maKBHienTai == -1)
            {
                MessageBox.Show("Vui lòng chọn ca khám!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string chanDoan = txtChanDoan.Text.Trim();
            if (string.IsNullOrEmpty(chanDoan))
            {
                MessageBox.Show("Vui lòng nhập chẩn đoán!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO CHANDOAN (MAKB, TENCHANDOAN) VALUES (@MAKB, @TENCHANDOAN)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        cmd.Parameters.AddWithValue("@TENCHANDOAN", chanDoan);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                txtChanDoan.Clear();
                LoadChanDoan();
                MessageBox.Show("Đã thêm chẩn đoán!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoaChanDoan_Click(object sender, EventArgs e)
        {
            if (dgvChanDoan.SelectedRows.Count == 0) return;
            
            string chanDoan = dgvChanDoan.SelectedRows[0].Cells["Chẩn Đoán"].Value.ToString();
            
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM CHANDOAN WHERE MAKB = @MAKB AND TENCHANDOAN = @TENCHANDOAN";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        cmd.Parameters.AddWithValue("@TENCHANDOAN", chanDoan);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LoadChanDoan();
                MessageBox.Show("Đã xóa chẩn đoán!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThemThuoc_Click(object sender, EventArgs e)
        {
            if (maKBHienTai == -1)
            {
                MessageBox.Show("Vui lòng chọn ca khám!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (cboThuoc.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn thuốc!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    
                    // Kiểm tra xem đã có toa thuốc chưa
                    string checkQuery = "SELECT MATT FROM TOATHUOC WHERE MAKB = @MAKB";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                    object result = checkCmd.ExecuteScalar();
                    
                    int maToa;
                    if (result == null)
                    {
                        // Tạo toa mới
                        string insertToa = "INSERT INTO TOATHUOC (MAKB, GHICHU) VALUES (@MAKB, @GHICHU); SELECT SCOPE_IDENTITY();";
                        SqlCommand cmdToa = new SqlCommand(insertToa, conn);
                        cmdToa.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        cmdToa.Parameters.AddWithValue("@GHICHU", txtGhiChuToa.Text.Trim());
                        maToa = Convert.ToInt32(cmdToa.ExecuteScalar());
                    }
                    else
                    {
                        maToa = Convert.ToInt32(result);
                    }
                    
                    // Thêm chi tiết toa
                    string insertDetail = "INSERT INTO CHITIETTOATHUOC (MATT, MATHUOC, SOLUONG) VALUES (@MATT, @MATHUOC, @SOLUONG)";
                    using (SqlCommand cmd = new SqlCommand(insertDetail, conn))
                    {
                        cmd.Parameters.AddWithValue("@MATT", maToa);
                        cmd.Parameters.AddWithValue("@MATHUOC", cboThuoc.SelectedValue);
                        cmd.Parameters.AddWithValue("@SOLUONG", nudSoLuong.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LoadToaThuoc();
                MessageBox.Show("Đã thêm thuốc vào toa!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoaThuoc_Click(object sender, EventArgs e)
        {
            if (dgvToaThuoc.SelectedRows.Count == 0) return;
            
            int matt = Convert.ToInt32(dgvToaThuoc.SelectedRows[0].Cells["MATT"].Value);
            string tenThuoc = dgvToaThuoc.SelectedRows[0].Cells["Tên Thuốc"].Value.ToString();
            
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    // Lấy MATHUOC từ tên
                    string getMaThuoc = "SELECT T.MATHUOC FROM THUOC T JOIN SANPHAM SP ON T.MATHUOC = SP.MASP WHERE SP.TENSP = @TENSP";
                    SqlCommand getCmd = new SqlCommand(getMaThuoc, conn);
                    getCmd.Parameters.AddWithValue("@TENSP", tenThuoc);
                    int maThuoc = Convert.ToInt32(getCmd.ExecuteScalar());
                    
                    string query = "DELETE FROM CHITIETTOATHUOC WHERE MATT = @MATT AND MATHUOC = @MATHUOC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MATT", matt);
                        cmd.Parameters.AddWithValue("@MATHUOC", maThuoc);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LoadToaThuoc();
                MessageBox.Show("Đã xóa thuốc khỏi toa!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLuuToaThuoc_Click(object sender, EventArgs e)
        {
            if (maKBHienTai == -1)
            {
                MessageBox.Show("Vui lòng chọn ca khám!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "UPDATE TOATHUOC SET GHICHU = @GHICHU WHERE MAKB = @MAKB";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        cmd.Parameters.AddWithValue("@GHICHU", txtGhiChuToa.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
                
                MessageBox.Show("Đã lưu toa thuốc thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============ TẠO CA KHÁM MỚI ============

        private void LoadKhachHang()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT MAKH, TENKH + ' - ' + SDT AS Display FROM KHACHHANG ORDER BY TENKH";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    
                    cboKhachHang.DataSource = dt;
                    cboKhachHang.DisplayMember = "Display";
                    cboKhachHang.ValueMember = "MAKH";
                    cboKhachHang.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load khách hàng: " + ex.Message);
            }
        }

        private void CboKhachHang_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kiểm tra SelectedIndex trước
            if (cboKhachHang.SelectedIndex == -1) return;
            
            // Kiểm tra SelectedItem
            if (cboKhachHang.SelectedItem == null) return;
            
            try
            {
                // Sử dụng SelectedItem với DataRowView thay vì SelectedValue
                int maKH = 0;
                if (cboKhachHang.SelectedItem is DataRowView drv)
                {
                    maKH = Convert.ToInt32(drv["MAKH"]);
                }
                else
                {
                    return; // Nếu không phải DataRowView thì bỏ qua
                }
                
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT MATC, TENTC + ' (' + LOAI + ')' AS Display FROM THUCUNG WHERE MAKH = @MAKH ORDER BY TENTC";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@MAKH", maKH);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    
                    cboThuCung.DataSource = dt;
                    cboThuCung.DisplayMember = "Display";
                    cboThuCung.ValueMember = "MATC";
                    cboThuCung.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load thú cưng: " + ex.Message);
            }
        }

        private void BtnLuuCaKham_Click(object sender, EventArgs e)
        {
            if (cboKhachHang.SelectedIndex == -1 || cboThuCung.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn khách hàng và thú cưng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Sử dụng SelectedItem với DataRowView
                if (cboThuCung.SelectedItem is DataRowView drvTC)
                {
                    maTCHienTai = Convert.ToInt32(drvTC["MATC"]);
                }
                else
                {
                    MessageBox.Show("Lỗi đọc thông tin thú cưng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                int maNV = UserSession.UserId;
                
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    
                    // Tạo ca khám mới
                    string query = @"
                        INSERT INTO CAKHAMBENH (MATC, MANV, NGAYKHAM) 
                        VALUES (@MATC, @MANV, @NGAYKHAM);
                        SELECT SCOPE_IDENTITY();";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MATC", maTCHienTai);
                    cmd.Parameters.AddWithValue("@MANV", maNV);
                    cmd.Parameters.AddWithValue("@NGAYKHAM", dtpNgayKhamMoi.Value.Date);
                    
                    maKBHienTai = Convert.ToInt32(cmd.ExecuteScalar());
                    
                    // Load thông tin ca khám
                    LoadThongTinCaKham();
                    
                    // Mở khóa chỉnh sửa
                    KhoaChinhSua(false);
                    
                    // Ẩn panel tạo mới
                    pnlTaoMoi.Visible = false;
                    
                    MessageBox.Show("Đã tạo ca khám mới thành công! Bây giờ có thể thêm triệu chứng, chẩn đoán và toa thuốc.", 
                        "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo ca khám: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadThongTinCaKham()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            KB.MAKB,
                            TC.TENTC,
                            TC.LOAI,
                            KH.TENKH,
                            KB.NGAYKHAM
                        FROM CAKHAMBENH KB
                        JOIN THUCUNG TC ON KB.MATC = TC.MATC
                        JOIN KHACHHANG KH ON TC.MAKH = KH.MAKH
                        WHERE KB.MAKB = @MAKB";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                    SqlDataReader reader = cmd.ExecuteReader();
                    
                    if (reader.Read())
                    {
                        txtMaKB.Text = reader["MAKB"].ToString();
                        txtTenTC.Text = reader["TENTC"].ToString();
                        txtLoai.Text = reader["LOAI"].ToString();
                        txtChuSoHuu.Text = reader["TENKH"].ToString();
                        dtpNgayKhamInfo.Value = Convert.ToDateTime(reader["NGAYKHAM"]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load thông tin: " + ex.Message);
            }
        }
    }
}
