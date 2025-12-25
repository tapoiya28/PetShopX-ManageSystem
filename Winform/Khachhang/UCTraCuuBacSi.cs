using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class UCTraCuuBacSi : UserControl
    {
        private ComboBox cboChiNhanh;
        private TextBox txtTenBS;
        private Button btnTim;
        private DataGridView dgvBacSi;

        public UCTraCuuBacSi()
        {
            this.Size = new Size(1100, 700);
            this.BackColor = Color.White;
            TaoGiaoDien();
            LoadChiNhanh();
            LoadDanhSachBacSi(); // Load mặc định tất cả
        }

        private void TaoGiaoDien()
        {
            Label lblTitle = new Label() { 
                Text = "👨‍⚕️ TRA CỨU ĐỘI NGŨ BÁC SĨ", 
                Font = new Font("Segoe UI", 20, FontStyle.Bold), 
                ForeColor = Color.FromArgb(51, 102, 255), 
                Location = new Point(30, 20), 
                AutoSize = true 
            };

            // Panel bộ lọc
            Panel pnlFilter = new Panel() { Location = new Point(30, 80), Size = new Size(1040, 70), BackColor = Color.WhiteSmoke };
            
            Label lblCN = new Label() { Text = "Chi nhánh:", Location = new Point(20, 25), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cboChiNhanh = new ComboBox() { Location = new Point(100, 22), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            
            Label lblTen = new Label() { Text = "Tên Bác sĩ:", Location = new Point(400, 25), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtTenBS = new TextBox() { Location = new Point(490, 22), Width = 250, Font = new Font("Segoe UI", 10) };

            btnTim = new Button() { 
                Text = "🔍 Tìm kiếm", 
                Location = new Point(780, 20), 
                Size = new Size(120, 32), 
                BackColor = Color.FromArgb(51, 102, 255), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnTim.Click += (s, e) => LoadDanhSachBacSi();

            pnlFilter.Controls.AddRange(new Control[] { lblCN, cboChiNhanh, lblTen, txtTenBS, btnTim });

            // Grid hiển thị
            dgvBacSi = new DataGridView()
            {
                Location = new Point(30, 170),
                Size = new Size(1040, 480),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                RowTemplate = { Height = 35 }
            };

            this.Controls.AddRange(new Control[] { lblTitle, pnlFilter, dgvBacSi });
        }

        private void LoadChiNhanh()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("sp_ChiNhanh_DanhSach", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    
                    // Thêm tùy chọn "Tất cả"
                    DataRow dr = dt.NewRow();
                    dr["MACN"] = DBNull.Value;
                    dr["TENCN"] = "--- Tất cả chi nhánh ---";
                    dt.Rows.InsertAt(dr, 0);

                    cboChiNhanh.DataSource = dt;
                    cboChiNhanh.DisplayMember = "TENCN";
                    cboChiNhanh.ValueMember = "MACN";
                }
            }
            catch { }
        }

        private void LoadDanhSachBacSi()
        {
            try
            {
                // Kiểm tra nếu ComboBox chưa có dữ liệu thì thoát, tránh lỗi Null
                if (cboChiNhanh.DataSource == null || cboChiNhanh.SelectedValue == null) return;

                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_TraCuu_BacSi", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        // Kiểm tra giá trị an toàn
                        var selectedVal = cboChiNhanh.SelectedValue;
                        if (selectedVal != null && selectedVal != DBNull.Value && selectedVal.ToString() != "")
                            cmd.Parameters.AddWithValue("@MaCN", selectedVal);
                        else
                            cmd.Parameters.AddWithValue("@MaCN", DBNull.Value);

                        cmd.Parameters.AddWithValue("@TenBS", txtTenBS.Text.Trim());

                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        dgvBacSi.DataSource = dt;
                        
                        if(dgvBacSi.Columns.Contains("MANV")) dgvBacSi.Columns["MANV"].Visible = false;
                    }
                }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show("Lỗi tải danh sách bác sĩ: " + ex.Message, "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
        }
    }
}