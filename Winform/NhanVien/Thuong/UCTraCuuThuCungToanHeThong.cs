using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class UCTraCuuThuCungToanHeThong : UserControl
    {
        private DataGridView dgvTatCaThuCung;
        private TextBox txtTimKiemTongHop;
        private Label lblCount;
        private ComboBox cboLocKhach;
        public UCTraCuuThuCungToanHeThong()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            TaoGiaoDien();
            
            // Tải dữ liệu lần đầu ngay khi mở tab
            TaiDuLieu(""); 
        }

        private void TaoGiaoDien()
        {
            // 1. Tiêu đề và ô tìm kiếm
            Panel pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(52, 73, 94) };
            
            Label lblTitle = new Label() { 
                Text = "TRA CỨU HỆ THỐNG THÚ CƯNG", 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 15), 
                AutoSize = true 
            };

            Label lblHint = new Label() { 
                Text = "🔍 Tìm nhanh (Tên bé, Tên chủ hoặc SĐT):", 
                ForeColor = Color.LightGray, 
                Location = new Point(22, 55), 
                AutoSize = true 
            };

            txtTimKiemTongHop = new TextBox() { 
                Location = new Point(20, 75), 
                Width = 450, 
                Font = new Font("Segoe UI", 12) 
            };
            
            // Sự kiện gõ đến đâu lọc đến đó
            txtTimKiemTongHop.TextChanged += (s, e) => TaiDuLieu(txtTimKiemTongHop.Text.Trim());

            lblCount = new Label() { 
                Text = "Tổng số: 0", 
                ForeColor = Color.Yellow, 
                Location = new Point(500, 78), 
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            Label lblFilter = new Label() { Text = "Lọc loại khách:", ForeColor = Color.White, Location = new Point(500, 55), AutoSize = true };
            cboLocKhach = new ComboBox() { 
                Location = new Point(500, 75), 
                Width = 150, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11) 
            };
            cboLocKhach.Items.Clear();
            cboLocKhach.Items.AddRange(new string[] { "Tất cả", "Khách cũ", "Khách mới", "Đã từng khám" });
            cboLocKhach.SelectedIndex = 0;
            cboLocKhach.SelectedIndex = 0;
            cboLocKhach.SelectedIndexChanged += (s, e) => TaiDuLieu(txtTimKiemTongHop.Text.Trim());

            pnlHeader.Controls.Add(lblFilter);
            pnlHeader.Controls.Add(cboLocKhach);
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblHint, txtTimKiemTongHop, lblCount });

            // 2. DataGridView
            dgvTatCaThuCung = new DataGridView() { 
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10)
            };

            // Chỉnh màu Grid cho chuyên nghiệp
            dgvTatCaThuCung.EnableHeadersVisualStyles = false;
            dgvTatCaThuCung.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80);
            dgvTatCaThuCung.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTatCaThuCung.ColumnHeadersHeight = 35;

            this.Controls.Add(dgvTatCaThuCung);
            this.Controls.Add(pnlHeader);
        }

        private void TaiDuLieu(string tuKhoa)
        {
            string loaiLoc = cboLocKhach.SelectedItem?.ToString() ?? "Tất cả";
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_NhanVien_TraCuuThuCungToanHeThong", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Keyword", (object)tuKhoa ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FilterType", loaiLoc);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvTatCaThuCung.DataSource = dt;
                        lblCount.Text = "Tổng số: " + dt.Rows.Count;
                        
                        // Tô màu dòng dựa trên loại khách
                        ToMauGrid();
                    }
                }
            }
            catch (Exception ex) { /* Xử lý lỗi */ }
        }
        private void ToMauGrid()
        {
            foreach (DataGridViewRow row in dgvTatCaThuCung.Rows)
            {
                if (row.Cells["LoaiKhach"].Value?.ToString() == "Khách mới")
                {
                    row.DefaultCellStyle.ForeColor = Color.DarkGreen; // Khách mới hiện màu xanh lá
                }
                else
                {
                    row.DefaultCellStyle.ForeColor = Color.Navy; // Khách cũ hiện màu xanh đậm
                }
            }
        }
    }
}