using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Winform
{
    public class UCMuaHangOnline : UserControl
    {
        // UI Controls
        private ComboBox cboLoaiHang;
        private TextBox txtTimKiem;
        private DataGridView dgvSanPham;
        private DataGridView dgvGioHang;
        private Label lblTongTien;
        
        // Data
        private DataTable dtGioHang;

        public UCMuaHangOnline()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            
            KhoiTaoGioHang();
            TaoGiaoDien();
            
            // Đảm bảo Control đã load xong mới tải dữ liệu
            this.Load += (s, e) => LoadSanPham();
        }

        private void KhoiTaoGioHang()
        {
            dtGioHang = new DataTable();
            dtGioHang.Columns.Add("MASP", typeof(int));
            dtGioHang.Columns.Add("TENSP", typeof(string));
            dtGioHang.Columns.Add("DONGIA", typeof(decimal));
            dtGioHang.Columns.Add("SOLUONG", typeof(int));
            dtGioHang.Columns.Add("THANHTIEN", typeof(decimal), "DONGIA * SOLUONG");
        }

        private void TaoGiaoDien()
        {
            Label lblTitle = new Label() { Text = "🛒 MUA SẮM TRỰC TUYẾN", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(51, 102, 255), Location = new Point(20, 10), AutoSize = true };

            // === CỘT TRÁI: DANH SÁCH SẢN PHẨM ===
            GroupBox grpSanPham = new GroupBox() { Text = "Danh sách sản phẩm", Location = new Point(20, 60), Size = new Size(650, 600), Font = new Font("Segoe UI", 10) };
            
            cboLoaiHang = new ComboBox() { Location = new Point(10, 30), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoaiHang.Items.AddRange(new string[] { "Tất cả", "Sản phẩm", "Thuốc", "Vacxin", "Gói tiêm" });
            cboLoaiHang.SelectedIndex = 0;
            cboLoaiHang.SelectedIndexChanged += (s, e) => LoadSanPham();

            txtTimKiem = new TextBox() { Location = new Point(140, 30), Width = 380, PlaceholderText = "Tìm tên sản phẩm..." };
            txtTimKiem.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadSanPham(); };

            Button btnTim = new Button() { Text = "Tìm", Location = new Point(530, 28), Width = 100, BackColor = Color.FromArgb(51, 102, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnTim.Click += (s, e) => LoadSanPham();

            dgvSanPham = new DataGridView() { 
                Location = new Point(10, 70), 
                Size = new Size(630, 520), 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
            dgvSanPham.CellDoubleClick += DgvSanPham_CellDoubleClick; 

            grpSanPham.Controls.AddRange(new Control[] { cboLoaiHang, txtTimKiem, btnTim, dgvSanPham });

            // === CỘT PHẢI: GIỎ HÀNG ===
            GroupBox grpGioHang = new GroupBox() { Text = "Giỏ hàng của bạn", Location = new Point(680, 60), Size = new Size(400, 600), Font = new Font("Segoe UI", 10) };
            
            dgvGioHang = new DataGridView() { 
                Location = new Point(10, 30), 
                Size = new Size(380, 420), 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, 
                AllowUserToAddRows = false, 
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
            dgvGioHang.DataSource = dtGioHang;
            
            // Xử lý ẩn cột an toàn sau khi gán DataSource
            dgvGioHang.DataBindingComplete += (s, e) => {
                if (dgvGioHang.Columns.Contains("MASP")) dgvGioHang.Columns["MASP"].Visible = false;
            };

            lblTongTien = new Label() { Text = "Tổng tiền: 0 đ", Location = new Point(10, 465), Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.Red, AutoSize = true };

            Button btnXoa = new Button() { Text = "Xóa món", Location = new Point(290, 460), Width = 100, BackColor = Color.Salmon, FlatStyle = FlatStyle.Flat, ForeColor = Color.White };
            btnXoa.Click += (s, e) => {
                if(dgvGioHang.CurrentRow != null) {
                    dtGioHang.Rows.RemoveAt(dgvGioHang.CurrentRow.Index);
                    TinhTongTien();
                }
            };

            Button btnDatHang = new Button() { 
                Text = "✅ XÁC NHẬN ĐẶT HÀNG", 
                Location = new Point(10, 510), 
                Size = new Size(380, 70), 
                BackColor = Color.FromArgb(46, 204, 113), 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 14, FontStyle.Bold), 
                FlatStyle = FlatStyle.Flat, 
                Cursor = Cursors.Hand 
            };
            btnDatHang.Click += BtnDatHang_Click;

            grpGioHang.Controls.AddRange(new Control[] { dgvGioHang, lblTongTien, btnXoa, btnDatHang });

            this.Controls.AddRange(new Control[] { lblTitle, grpSanPham, grpGioHang });
        }

        private void LoadSanPham()
        {
            try
            {
                string loaiCode = "All";
                string selected = cboLoaiHang.SelectedItem?.ToString() ?? "Tất cả";

                if (selected == "Sản phẩm") loaiCode = "SP";
                else if (selected == "Thuốc") loaiCode = "Th";
                else if (selected == "Vacxin") loaiCode = "VX";
                else if (selected == "Gói tiêm") loaiCode = "GT";

                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_SanPham_TimKiemOnline", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TuKhoa", txtTimKiem.Text.Trim());
                        cmd.Parameters.AddWithValue("@Loai", loaiCode);
                        
                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        dgvSanPham.DataSource = dt;

                        // Ẩn các cột kỹ thuật
                        if(dgvSanPham.Columns.Contains("MASP")) dgvSanPham.Columns["MASP"].Visible = false;
                        if(dgvSanPham.Columns.Contains("LOAI")) dgvSanPham.Columns["LOAI"].Visible = false;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải sản phẩm: " + ex.Message); }
        }

        private void DgvSanPham_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            var row = dgvSanPham.Rows[e.RowIndex];
            int maSP = Convert.ToInt32(row.Cells["MASP"].Value);
            string tenSP = row.Cells["TENSP"].Value.ToString();
            decimal donGia = Convert.ToDecimal(row.Cells["DONGIA"].Value);
            int tonKho = Convert.ToInt32(row.Cells["TONKHO"].Value);

            // Kiểm tra đã có trong giỏ chưa
            DataRow existingRow = dtGioHang.AsEnumerable().FirstOrDefault(r => r.Field<int>("MASP") == maSP);
            
            if (existingRow != null)
            {
                int slHienTai = existingRow.Field<int>("SOLUONG");
                if (slHienTai < tonKho)
                    existingRow["SOLUONG"] = slHienTai + 1;
                else
                    MessageBox.Show("Số lượng trong giỏ đã đạt mức tối đa trong kho!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (tonKho > 0)
                    dtGioHang.Rows.Add(maSP, tenSP, donGia, 1);
                else
                    MessageBox.Show("Sản phẩm này hiện đang hết hàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            TinhTongTien();
        }

        private void TinhTongTien()
        {
            decimal tong = 0;
            foreach (DataRow r in dtGioHang.Rows)
            {
                tong += Convert.ToDecimal(r["THANHTIEN"]);
            }
            lblTongTien.Text = $"Tổng tiền: {tong:N0} đ";
        }

        private void BtnDatHang_Click(object sender, EventArgs e)
        {
            if (dtGioHang.Rows.Count == 0) {
                MessageBox.Show("Giỏ hàng của bạn đang trống!"); return;
            }

            // Kiểm tra UserId từ UserSession (Hãy đảm bảo UserSession được gán khi Đăng nhập)
            if (UserSession.UserId <= 0) {
                MessageBox.Show("Vui lòng đăng nhập để thực hiện chức năng này!"); return;
            }

            DialogResult dr = MessageBox.Show("Bạn có chắc chắn muốn đặt đơn hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No) return;

            SqlTransaction tran = null;
            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    tran = conn.BeginTransaction();

                    decimal tongTien = Convert.ToDecimal(dtGioHang.Compute("SUM(THANHTIEN)", ""));
                    int maHD = 0;

                    // 1. Tạo hóa đơn tổng (sp_DonHangOnline_Tao)
                    using (SqlCommand cmd = new SqlCommand("sp_DonHangOnline_Tao", conn, tran)) {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaKH", UserSession.UserId);
                        cmd.Parameters.AddWithValue("@TongTien", tongTien);
                        
                        SqlParameter outParam = new SqlParameter("@MaHD", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();
                        maHD = (int)outParam.Value;
                    }

                    // 2. Thêm từng chi tiết (sp_DonHangOnline_ThemChiTiet)
                    foreach (DataRow r in dtGioHang.Rows) {
                        using (SqlCommand cmdDetail = new SqlCommand("sp_DonHangOnline_ThemChiTiet", conn, tran)) {
                            cmdDetail.CommandType = CommandType.StoredProcedure;
                            cmdDetail.Parameters.AddWithValue("@MaHD", maHD);
                            cmdDetail.Parameters.AddWithValue("@MaSP", r["MASP"]);
                            cmdDetail.Parameters.AddWithValue("@SoLuong", r["SOLUONG"]);
                            cmdDetail.ExecuteNonQuery();
                        }
                    }

                    tran.Commit();
                    MessageBox.Show($"Đặt hàng thành công!\nMã hóa đơn của bạn là: {maHD}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Reset giao diện
                    dtGioHang.Rows.Clear();
                    TinhTongTien();
                    LoadSanPham(); // Cập nhật lại tồn kho trên Grid
                }
            }
            catch (Exception ex) {
                tran?.Rollback();
                MessageBox.Show("Lỗi hệ thống khi đặt hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}