using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    /// <summary>
    /// UC dành cho Bác sĩ/Nhân viên để XEM lịch sử khám bệnh
    /// </summary>
    public class UCXemLichSuKham : UserControl
    {
        // Panel chính
        private Panel pnlTimKiem;
        private Panel pnlThongTinKham;
        private Panel pnlTrieuChung;
        private Panel pnlChanDoan;
        private Panel pnlToaThuoc;

        // Controls Tìm kiếm
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
        
        // Chẩn đoán
        private DataGridView dgvChanDoan;
        
        // Toa thuốc
        private DataGridView dgvToaThuoc;
        private TextBox txtGhiChuToa;
        
        // Biến lưu trạng thái
        private int maKBHienTai = -1;
        private int maTCHienTai = -1;

        public UCXemLichSuKham()
        {
            this.Size = new Size(1100, 750);
            this.BackColor = Color.FromArgb(236, 240, 245);
            this.AutoScroll = true;
            
            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            // ==== TIÊU ĐỀ ====
            lblTitle = new Label()
            {
                Text = "📋 LỊCH SỬ KHÁM BỆNH",
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
                Size = new Size(490, 220),
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            panel.Controls.AddRange(new Control[] { lblSection, dgvTrieuChung });
            
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
                Size = new Size(490, 220),
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            
            panel.Controls.AddRange(new Control[] { lblSection, dgvChanDoan });
            
            return panel;
        }

        private Panel TaoPanelToaThuoc()
        {
            Panel panel = new Panel()
            {
                Location = new Point(20, 740),
                Size = new Size(1060, 280),
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
            
            Label lblGhiChu = new Label() { Text = "Ghi chú cho toa thuốc:", Location = new Point(15, 210), AutoSize = true, Font = new Font("Segoe UI", 9) };
            txtGhiChuToa = new TextBox() { Location = new Point(160, 207), Width = 600, Multiline = true, Height = 60, ReadOnly = true, BackColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 9) };
            
            panel.Controls.AddRange(new Control[] { lblSection, dgvToaThuoc, lblGhiChu, txtGhiChuToa });
            
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
                    using (SqlCommand cmd = new SqlCommand("sp_LichSuKham_TimTheoSDT", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SDT", sdt);
                        
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
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
                    using (SqlCommand cmd = new SqlCommand("sp_TrieuChung_DanhSach", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvTrieuChung.DataSource = dt;
                        
                        if (dgvTrieuChung.Columns.Contains("STT"))
                            dgvTrieuChung.Columns["STT"].Width = 50;
                    }
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
                    using (SqlCommand cmd = new SqlCommand("sp_ChanDoan_DanhSach", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvChanDoan.DataSource = dt;
                        
                        if (dgvChanDoan.Columns.Contains("STT"))
                            dgvChanDoan.Columns["STT"].Width = 50;
                    }
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
                    using (SqlCommand cmd = new SqlCommand("sp_ToaThuoc_ChiTiet", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MAKB", maKBHienTai);
                        
                        // Đọc result set đầu tiên (danh sách thuốc)
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        
                        if (ds.Tables.Count > 0)
                        {
                            dgvToaThuoc.DataSource = ds.Tables[0];
                            
                            if (dgvToaThuoc.Columns.Contains("MATT"))
                                dgvToaThuoc.Columns["MATT"].Visible = false;
                        }
                        
                        // Đọc result set thứ hai (ghi chú)
                        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                        {
                            object ghiChu = ds.Tables[1].Rows[0][0];
                            txtGhiChuToa.Text = ghiChu != null && ghiChu != DBNull.Value ? ghiChu.ToString() : "";
                        }
                        else
                        {
                            txtGhiChuToa.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load toa thuốc: " + ex.Message);
            }
        }
    }
}
