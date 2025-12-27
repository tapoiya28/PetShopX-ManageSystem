using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class FrmThemLichTrucTiep : Form
    {
        private TextBox txtTimKhach;
        private DataGridView dgvKhachHang;
        private ComboBox cboDichVu;
        private DateTimePicker dtpNgay, dtpGio;
        private TextBox txtGhiChu;
        private Button btnLuu, btnHuy;
        
        private int selectedMaKH = -1;

        public FrmThemLichTrucTiep()
        {
            this.Text = "Thêm lịch hẹn cho khách trực tiếp";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            TaoGiaoDien();
            LoadDichVu();
        }

        private void TaoGiaoDien()
        {
            // 1. Phần tìm khách hàng
            Label lblSearch = new Label() { Text = "Tìm khách hàng (Tên/SĐT):", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtTimKhach = new TextBox() { Location = new Point(20, 45), Width = 540, Font = new Font("Segoe UI", 11) };
            txtTimKhach.TextChanged += (s, e) => TimKiemKhachHang(txtTimKhach.Text.Trim());

            dgvKhachHang = new DataGridView() { 
                Location = new Point(20, 80), Width = 540, Height = 150, 
                ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false
            };
            dgvKhachHang.SelectionChanged += (s, e) => {
                if (dgvKhachHang.SelectedRows.Count > 0)
                    selectedMaKH = Convert.ToInt32(dgvKhachHang.SelectedRows[0].Cells["MAKH"].Value);
            };

            // 2. Thông tin lịch hẹn
            int y = 250;
            Label lblDV = new Label() { Text = "Dịch vụ:", Location = new Point(20, y), AutoSize = true };
            cboDichVu = new ComboBox() { Location = new Point(20, y + 25), Width = 540, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };

            y += 70;
            Label lblNgay = new Label() { Text = "Ngày hẹn:", Location = new Point(20, y), AutoSize = true };
            dtpNgay = new DateTimePicker() { Location = new Point(20, y + 25), Width = 250, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };

            Label lblGio = new Label() { Text = "Giờ hẹn:", Location = new Point(310, y), AutoSize = true };
            dtpGio = new DateTimePicker() { Location = new Point(310, y + 25), Width = 250, Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Font = new Font("Segoe UI", 10) };

            y += 70;
            Label lblNote = new Label() { Text = "Ghi chú:", Location = new Point(20, y), AutoSize = true };
            txtGhiChu = new TextBox() { Location = new Point(20, y + 25), Width = 540, Height = 100, Multiline = true, Font = new Font("Segoe UI", 10) };

            // 3. Nút bấm
            btnLuu = new Button() { Text = "TẠO LỊCH HẸN", Location = new Point(150, 580), Width = 150, Height = 45, BackColor = Color.ForestGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnLuu.Click += BtnLuu_Click;

            btnHuy = new Button() { Text = "HỦY", Location = new Point(320, 580), Width = 100, Height = 45, FlatStyle = FlatStyle.Flat };
            btnHuy.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblSearch, txtTimKhach, dgvKhachHang, lblDV, cboDichVu, lblNgay, dtpNgay, lblGio, dtpGio, lblNote, txtGhiChu, btnLuu, btnHuy });
        }

        private void TimKiemKhachHang(string keyword)
        {
            // 1. Chỉ tìm khi từ khóa có ít nhất 1 ký tự (hoặc để trống để hiện tất cả tùy bạn)
            if (string.IsNullOrEmpty(keyword)) {
                dgvKhachHang.DataSource = null;
                return;
            }

            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_NhanVien_TimKhachHang", conn)) {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // 2. ÉP KIỂU NVARCHAR để xử lý đúng tiếng Việt có dấu
                        SqlParameter param = new SqlParameter("@TuKhoa", SqlDbType.NVarChar, 100);
                        param.Value = keyword;
                        cmd.Parameters.Add(param);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // 3. Gán dữ liệu (DataSource = null trước để làm mới lưới)
                        dgvKhachHang.DataSource = null; 
                        dgvKhachHang.DataSource = dt;

                        // 4. Ẩn cột mã khách hàng để giao diện đẹp hơn
                        if (dgvKhachHang.Columns.Contains("MAKH")) 
                            dgvKhachHang.Columns["MAKH"].Visible = false;
                    }
                }
            } catch (Exception ex) {
                // 5. Hiện lỗi lên màn hình để dễ xử lý (Thay vì Console.WriteLine)
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDichVu()
        {
            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT MALOAIDV, TENLOAIDV FROM LOAIDICHVU", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cboDichVu.DataSource = dt;
                    cboDichVu.DisplayMember = "TENLOAIDV";
                    cboDichVu.ValueMember = "MALOAIDV";
                }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (selectedMaKH == -1) {
                MessageBox.Show("Vui lòng chọn một khách hàng từ danh sách!", "Thông báo");
                return;
            }

            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("sp_DatLichHen", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NgayHen", dtpNgay.Value.Date);
                    cmd.Parameters.AddWithValue("@ThoiGian", dtpGio.Value.TimeOfDay);
                    cmd.Parameters.AddWithValue("@NoiDung", "Trực tiếp: " + txtGhiChu.Text.Trim());
                    cmd.Parameters.AddWithValue("@MaKH", selectedMaKH);
                    cmd.Parameters.AddWithValue("@MaCN", UserSession.WorkBranchId); // Lấy chi nhánh từ phiên đăng nhập
                    cmd.Parameters.AddWithValue("@MaLoaiDV", cboDichVu.SelectedValue);

                    SqlParameter outParam = new SqlParameter("@MaLichHen", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(outParam);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Đã thêm lịch hẹn trực tiếp thành công!", "Thành công");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            } catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
    }
}