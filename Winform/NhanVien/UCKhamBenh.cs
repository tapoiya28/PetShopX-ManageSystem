using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    /// <summary>
    /// UC dành cho Bác sĩ/Nhân viên để thực hiện quy trình khám bệnh cho thú cưng
    /// KỊCH BẢN 3: KHÁM BỆNH
    /// </summary>
    public class UCKhamBenh : UserControl
    {
        // Panel chính
        private Panel pnlTimKiem;
        private Panel pnlThongTinKham;
        private Panel pnlTrieuChung;
        private Panel pnlChanDoan;
        private Panel pnlToaThuoc;

        // Controls Tìm kiếm & Chọn Ca khám
        private Label lblTitle;
        private TextBox txtTimSDT;
        private Button btnTimKiem;
        private DataGridView dgvLichSuKham;
        
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
        private DateTimePicker dtpNgayKham;
        
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
        }

        private void TaoGiaoDien()
        {
            // ==== TIÊU ĐỀ ====
            lblTitle = new Label()
            {
                Text = "🏥 KHÁM BỆNH THÚ CƯNG",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(30, 15),
                AutoSize = true
            };
            
            // ==== PANEL TÌM KIẾM ====
            pnlTimKiem = new Panel()
            {
                Location = new Point(20, 60),
                Size = new Size(1060, 250),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Label lblTimKiem = new Label() 
            { 
                Text = "🔍 Tìm lịch sử khám theo SĐT khách hàng:", 
                Location = new Point(15, 15), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 10, FontStyle.Bold) 
            };
            
            Label lblSDT = new Label()
            {
                Text = "Nhập SĐT:",
                Location = new Point(30, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            
            txtTimSDT = new TextBox() 
            { 
                Location = new Point(120, 47), 
                Width = 200, 
                Font = new Font("Segoe UI", 10) 
            };
            
            btnTimKiem = new Button()
            {
                Text = "Tìm kiếm",
                Location = new Point(330, 45),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTimKiem.FlatAppearance.BorderSize = 0;
            btnTimKiem.Click += BtnTimKiem_Click;
            
            dgvLichSuKham = new DataGridView()
            {
                Location = new Point(15, 85),
                Size = new Size(1030, 150),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvLichSuKham.CellClick += DgvLichSuKham_CellClick;
            
            pnlTimKiem.Controls.AddRange(new Control[] { lblTimKiem, lblSDT, txtTimSDT, btnTimKiem, dgvLichSuKham });
            
            // ==== PANEL THÔNG TIN CA KHÁM ====
            pnlThongTinKham = TaoPanelThongTin();
            
            // ==== PANEL TRIỆU CHỨNG ====
            pnlTrieuChung = TaoPanelTrieuChung();
            
            // ==== PANEL CHẨN ĐOÁN ====
            pnlChanDoan = TaoPanelChanDoan();
            
            // ==== PANEL TOA THUỐC ====
            pnlToaThuoc = TaoPanelToaThuoc();
            
            this.Controls.AddRange(new Control[] { lblTitle, pnlTimKiem, pnlThongTinKham, pnlTrieuChung, pnlChanDoan, pnlToaThuoc });
        }

        private Panel TaoPanelThongTin()
        {
            Panel panel = new Panel()
            {
                Location = new Point(20, 320),
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
            dtpNgayKham = new DateTimePicker() { Location = new Point(450, 77), Width = 150, Enabled = false, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9) };
            
            panel.Controls.AddRange(new Control[] { 
                lblSectionTitle, 
                lblMaKB, txtMaKB, 
                lblTenThuCung, txtTenTC, 
                lblLoaiThuCung, txtLoai,
                lblChuSoHuu, txtChuSoHuu, 
                lblNgayKham, dtpNgayKham 
            });
            
            return panel;
        }

        private Panel TaoPanelTrieuChung()
        {
            Panel panel = new Panel()
            {
                Location = new Point(20, 450),
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
                Location = new Point(560, 450),
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
                Location = new Point(20, 740),
                Size = new Size(1060, 330),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Label lblSection = new Label() { Text = "💊 TOA THUỐC", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(39, 174, 96) };
            
            dgvToaThuoc = new DataGridView()
            {
                Location = new Point(15, 45),
                Size = new Size(1030, 180),
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            // Controls thêm thuốc
            Label lblThuoc = new Label() { Text = "Thuốc:", Location = new Point(15, 240), AutoSize = true, Font = new Font("Segoe UI", 9) };
            cboThuoc = new ComboBox() { Location = new Point(80, 237), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            
            Label lblSoLuong = new Label() { Text = "SL:", Location = new Point(350, 240), AutoSize = true, Font = new Font("Segoe UI", 9) };
            nudSoLuong = new NumericUpDown() { Location = new Point(390, 237), Width = 70, Minimum = 1, Maximum = 999, Value = 1, Font = new Font("Segoe UI", 9) };
            
            btnThemThuoc = new Button()
            {
                Text = "➕ Thêm",
                Location = new Point(480, 235),
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
                Location = new Point(570, 235),
                Size = new Size(70, 28),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnXoaThuoc.FlatAppearance.BorderSize = 0;
            btnXoaThuoc.Click += BtnXoaThuoc_Click;
            
            Label lblGhiChu = new Label() { Text = "Lời dặn BS:", Location = new Point(15, 280), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtGhiChuToa = new TextBox() { Location = new Point(110, 275), Width = 530, Height = 30, Multiline = true, PlaceholderText = "Ghi chú cho toa thuốc...", Font = new Font("Segoe UI", 9) };
            
            btnLuuToaThuoc = new Button()
            {
                Text = "💾 LƯU TOA THUỐC",
                Location = new Point(660, 270),
                Size = new Size(180, 40),
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

        private void BtnTimKiem_Click(object sender, EventArgs e)
        {
            string sdt = txtTimSDT.Text.Trim();
            if (string.IsNullOrEmpty(sdt))
            {
                MessageBox.Show("Vui lòng nhập SĐT khách hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT DISTINCT
                            KB.MAKB,
                            KB.NGAYKHAM,
                            TC.MATC,
                            TC.TENTC AS [Tên Thú Cưng],
                            TC.LOAI AS [Loại],
                            KH.TENKH AS [Chủ Sở Hữu],
                            KH.SDT,
                            NV.HOTEN AS [Bác Sĩ Khám]
                        FROM CAKHAMBENH KB
                        JOIN THUCUNG TC ON KB.MATC = TC.MATC
                        JOIN KHACHHANG KH ON TC.MAKH = KH.MAKH
                        LEFT JOIN NHANVIEN NV ON KB.MANV = NV.MANV
                        WHERE KH.SDT LIKE @SDT
                        ORDER BY KB.NGAYKHAM DESC";
                    
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@SDT", "%" + sdt + "%");
                    
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    
                    dgvLichSuKham.DataSource = dt;
                    
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show($"Không tìm thấy lịch sử khám cho SĐT: {sdt}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Đã tìm thấy {dt.Rows.Count} ca khám!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    
                    // Ẩn cột ID
                    if (dgvLichSuKham.Columns.Contains("MAKB"))
                        dgvLichSuKham.Columns["MAKB"].Visible = false;
                    if (dgvLichSuKham.Columns.Contains("MATC"))
                        dgvLichSuKham.Columns["MATC"].Visible = false;
                    if (dgvLichSuKham.Columns.Contains("SDT"))
                        dgvLichSuKham.Columns["SDT"].Visible = false;
                    if (dgvLichSuKham.Columns.Contains("NGAYKHAM"))
                        dgvLichSuKham.Columns["NGAYKHAM"].HeaderText = "Ngày Khám";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvLichSuKham_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            var row = dgvLichSuKham.Rows[e.RowIndex];
            
            maKBHienTai = Convert.ToInt32(row.Cells["MAKB"].Value);
            maTCHienTai = Convert.ToInt32(row.Cells["MATC"].Value);
            
            txtMaKB.Text = maKBHienTai.ToString();
            txtTenTC.Text = row.Cells["Tên Thú Cưng"].Value.ToString();
            txtLoai.Text = row.Cells["Loại"].Value.ToString();
            txtChuSoHuu.Text = row.Cells["Chủ Sở Hữu"].Value.ToString();
            dtpNgayKham.Value = Convert.ToDateTime(row.Cells["NGAYKHAM"].Value);
            
            // Load dữ liệu triệu chứng, chẩn đoán, toa thuốc
            LoadTrieuChung();
            LoadChanDoan();
            LoadToaThuoc();
        }

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
    }
}
