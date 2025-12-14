using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    public class UCLichHen : UserControl
    {
        private DataGridView dgvLichHen;
        private TextBox txtTimKiem;
        private Button btnTim;
        private Button btnXacNhan;

        public UCLichHen()
        {
            this.BackColor = Color.WhiteSmoke; // Đổi nền sang xám nhẹ cho dịu mắt
            TaoGiaoDien();
            TaiDanhSachLichHen();
        }

        private void TaoGiaoDien()
        {
            // 1. Tiêu đề
            Label lblTitle = new Label() { 
                Text = "QUẢN LÝ LỊCH HẸN", 
                Font = new Font("Segoe UI", 18, FontStyle.Bold), 
                ForeColor = Color.FromArgb(44, 62, 80), // Màu xanh đen
                Location = new Point(20, 15), 
                AutoSize = true 
            };

            // 2. Khu vực tìm kiếm (Gom vào 1 Panel cho gọn nếu muốn, ở đây giữ nguyên vị trí nhưng chỉnh đẹp hơn)
            Label lblSdt = new Label() { Text = "SĐT:", Location = new Point(25, 75), AutoSize = true, Font = new Font("Segoe UI", 10) };
            
            txtTimKiem = new TextBox() { Location = new Point(140, 72), Width = 250, Font = new Font("Segoe UI", 10) };
            
            btnTim = new Button() { 
                Text = "🔍 Tìm kiếm", 
                Location = new Point(400, 70), 
                Width = 100, 
                Height = 30,
                BackColor = Color.FromArgb(52, 152, 219), // Xanh dương
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTim.FlatAppearance.BorderSize = 0;
            btnTim.Click += (s, e) => TaiDanhSachLichHen();

            // 3. Nút Xác Nhận (Check-in)
            btnXacNhan = new Button() { 
                Text = "✅ XÁC NHẬN & TẠO HỒ SƠ", 
                Location = new Point(650, 70), 
                Width = 240, 
                Height = 30,
                BackColor = Color.ForestGreen, 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnXacNhan.FlatAppearance.BorderSize = 0;
            btnXacNhan.Click += (s, e) => XuLyXacNhan();

            // 4. GridView (PHẦN QUAN TRỌNG NHẤT)
            dgvLichHen = new DataGridView();
            dgvLichHen.Location = new Point(20, 120);
            dgvLichHen.Size = new Size(900, 500);
            dgvLichHen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            
            // --- CẤU HÌNH GIAO DIỆN GRID ---
            dgvLichHen.BackgroundColor = Color.White;
            dgvLichHen.BorderStyle = BorderStyle.None; // Bỏ viền ngoài
            dgvLichHen.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal; // Chỉ kẻ ngang
            dgvLichHen.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            // Header (Tiêu đề cột)
            dgvLichHen.EnableHeadersVisualStyles = false; // Bắt buộc để chỉnh màu header
            dgvLichHen.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80); // Nền header tối
            dgvLichHen.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; // Chữ trắng
            dgvLichHen.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvLichHen.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvLichHen.ColumnHeadersHeight = 40; // Header cao hơn cho thoáng
            
            // Rows (Dòng dữ liệu)
            dgvLichHen.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvLichHen.DefaultCellStyle.ForeColor = Color.Black;
            dgvLichHen.DefaultCellStyle.SelectionBackColor = Color.FromArgb(46, 204, 113); // Màu xanh khi chọn dòng
            dgvLichHen.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvLichHen.DefaultCellStyle.Padding = new Padding(5, 0, 0, 0); // Cách lề chữ 1 chút
            dgvLichHen.RowTemplate.Height = 35; // Dòng cao hơn dễ đọc
            
            // Zebra Striping (Màu so le)
            dgvLichHen.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            
            // Scroll & Size settings
            dgvLichHen.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells; // Tự giãn theo nội dung => Có thanh cuộn ngang nếu dài
            dgvLichHen.ScrollBars = ScrollBars.Both; // Hiện cả 2 thanh cuộn
            dgvLichHen.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLichHen.MultiSelect = false;
            dgvLichHen.ReadOnly = true;
            dgvLichHen.AllowUserToAddRows = false; // Ẩn dòng trống cuối cùng
            dgvLichHen.AllowUserToResizeRows = false;
            
            this.Controls.AddRange(new Control[] { lblTitle, lblSdt, txtTimKiem, btnTim, btnXacNhan, dgvLichHen });
        }

        private void TaiDanhSachLichHen()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_NhanVien_XemLichHen", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaCN", UserSession.WorkBranchId);
                        
                        string sdt = txtTimKiem.Text.Trim();
                        if (!string.IsNullOrEmpty(sdt))
                            cmd.Parameters.AddWithValue("@SdtKhachHang", sdt);
                        else
                            cmd.Parameters.AddWithValue("@SdtKhachHang", DBNull.Value);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvLichHen.DataSource = dt;

                        // --- ĐỔI TÊN CỘT TIẾNG VIỆT SAU KHI LOAD DATA ---
                        if (dgvLichHen.Columns.Count > 0)
                        {
                            // Đặt tên hiển thị (Bạn cần chỉnh lại chính xác theo tên cột trong SQL của bạn)
                            SetColumnHeader("MALICHHEN", "Mã Lịch");
                            SetColumnHeader("NGAYHEN", "Ngày Hẹn");
                            SetColumnHeader("GIOHEN", "Giờ Hẹn");
                            SetColumnHeader("HOTENKH", "Khách Hàng");
                            SetColumnHeader("SDT", "Số ĐT");
                            SetColumnHeader("TENLOAIDV", "Dịch Vụ");
                            SetColumnHeader("TRANGTHAI", "Trạng Thái");
                            SetColumnHeader("GHICHU", "Ghi Chú");
                            
                            // Ẩn các cột ID không cần thiết (nếu có)
                            // SetColumnVisible("MAKH", false); 
                            
                            // Cố định cột quan trọng bên trái khi cuộn ngang
                            if(dgvLichHen.Columns.Contains("HOTENKH")) 
                                dgvLichHen.Columns["HOTENKH"].Frozen = true;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        // Hàm phụ trợ để đặt tên cột cho gọn code
        private void SetColumnHeader(string colName, string headerText)
        {
            if (dgvLichHen.Columns.Contains(colName))
            {
                dgvLichHen.Columns[colName].HeaderText = headerText;
            }
        }
        
        private void SetColumnVisible(string colName, bool isVisible)
        {
             if (dgvLichHen.Columns.Contains(colName))
            {
                dgvLichHen.Columns[colName].Visible = isVisible;
            }
        }

        private void XuLyXacNhan()
        {
            if (dgvLichHen.SelectedRows.Count == 0) return;

            var row = dgvLichHen.SelectedRows[0];
            
            // Logic giữ nguyên, chỉ thêm kiểm tra null an toàn
            int maLichHen = row.Cells["MALICHHEN"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["MALICHHEN"].Value) : 0;
            int maKH = row.Cells["MAKH"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["MAKH"].Value) : 0;
            string tenLoaiDV = row.Cells["TENLOAIDV"].Value != DBNull.Value ? row.Cells["TENLOAIDV"].Value.ToString() : "";

            bool isKhamBenh = tenLoaiDV.Contains("Khám");
            bool isTiemPhong = tenLoaiDV.Contains("Tiêm");

            if (!isKhamBenh && !isTiemPhong)
            {
                MessageBox.Show("Dịch vụ này không cần tạo hồ sơ bệnh án/tiêm chủng (Spa hoặc Mua hàng).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int maTC_DuocChon = HienFormChonThuCung(maKH);
            if (maTC_DuocChon == -1) return;

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_NhanVien_XacNhanLichHen", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaLichHen", maLichHen);
                        cmd.Parameters.AddWithValue("@MaNV", UserSession.UserId);
                        cmd.Parameters.AddWithValue("@MaTC", maTC_DuocChon);

                        cmd.ExecuteNonQuery();
                        
                        MessageBox.Show($"Đã tạo hồ sơ {tenLoaiDV} thành công!\nBác sĩ có thể bắt đầu làm việc.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        TaiDanhSachLichHen();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private int HienFormChonThuCung(int maKH)
        {
    // Tạo Form popup
            Form frm = new Form() { 
                Size = new Size(350, 200), 
                Text = "Chọn Thú Cưng", 
                StartPosition = FormStartPosition.CenterParent, 
                FormBorderStyle = FormBorderStyle.FixedDialog, 
                MaximizeBox = false, 
                MinimizeBox = false 
            };
            
            ComboBox cbo = new ComboBox() { 
                Location = new Point(50, 60), 
                Width = 230, 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Font = new Font("Segoe UI", 10) 
            };
            
            Button btnOk = new Button() { 
                Text = "Xác Nhận", 
                Location = new Point(115, 110), 
                Width = 100, 
                Height = 30, 
                DialogResult = DialogResult.OK, 
                BackColor = Color.ForestGreen, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat 
            };
            
            Label lblHoi = new Label() { 
                Text = "Khách hàng mang bé nào đến?", 
                Location = new Point(50, 25), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 10) 
            };

            frm.Controls.AddRange(new Control[] { lblHoi, cbo, btnOk });

            try 
            {
                using (SqlConnection conn = Connection.GetConnection()) 
                {
                    conn.Open();
                    // --- ĐÂY LÀ CHỖ DỄ GÂY LỖI NHẤT ---
                    // Hãy chắc chắn Store Procedure "sp_ThuCung_XemDanhSach" tồn tại
                    // và tham số đầu vào đúng là @MAKH
                    SqlDataAdapter da = new SqlDataAdapter("sp_ThuCung_XemDanhSach", conn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@MAKH", maKH);
                    
                    DataTable dt = new DataTable(); 
                    da.Fill(dt);
                    
                    // Debug: Kiểm tra xem có lấy được dữ liệu không
                    if (dt.Rows.Count == 0) 
                    { 
                        MessageBox.Show($"Khách hàng (Mã {maKH}) chưa có thú cưng nào trong hệ thống!\nVui lòng thêm thú cưng trước.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                        return -1; 
                    }
                    
                    // Kiểm tra tên cột trong database có đúng là "TENTC" và "MATC" không?
                    // Nếu database bạn đặt là "TenThuCung" thì sửa lại dòng dưới
                    cbo.DataSource = dt; 
                    cbo.DisplayMember = "TENTC"; // <--- Kiểm tra kỹ tên cột này trong SQL
                    cbo.ValueMember = "MATC";    // <--- Kiểm tra kỹ tên cột này trong SQL
                }
            } 
            catch (Exception ex) 
            { 
                // --- HIỆN LỖI LÊN THAY VÌ IM LẶNG ---
                MessageBox.Show("Lỗi khi tải danh sách thú cưng: " + ex.Message); 
                return -1; 
            }

            if (frm.ShowDialog() == DialogResult.OK) 
            {
                if (cbo.SelectedValue != null)
                    return Convert.ToInt32(cbo.SelectedValue);
            }
            
            return -1;
        }
    }
}