using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    public class LoginForm : Form
    {
        // Khai báo các Panel container
        private Panel pnlDangNhap;
        private Panel pnlDangKy;

        // Các controls cho phần Đăng Nhập
        private TextBox txtLoginInput; // Nhập SĐT hoặc Tên đăng nhập
        private TextBox txtLoginPass;

        // Các controls cho phần Đăng Ký
        private TextBox txtRegHoTen;
        private TextBox txtRegSDT;
        private TextBox txtRegUser; // Tên đăng nhập
        private TextBox txtRegDiaChi;
        private TextBox txtRegPass;

        public LoginForm()
        {
            // Cài đặt Form
            this.Size = new Size(420, 520);
            this.Text = "Hệ Thống PetcareX";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Khởi tạo giao diện
            TaoGiaoDienDangNhap();
            TaoGiaoDienDangKy();

            // Mặc định hiện màn hình Đăng nhập
            ChuyenManHinh(isLogin: true);
        }

        // --- 1. GIAO DIỆN ĐĂNG NHẬP ---
        private void TaoGiaoDienDangNhap()
        {
            pnlDangNhap = new Panel() { Size = this.ClientSize, Location = new Point(0, 0) };

            Label lblTitle = new Label() { Text = "ĐĂNG NHẬP", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.Navy, Location = new Point(130, 40), AutoSize = true };

            Label lblInput = new Label() { Text = "SĐT hoặc Tên đăng nhập:", Location = new Point(50, 100), AutoSize = true };
            txtLoginInput = new TextBox() { Location = new Point(50, 125), Width = 300, Font = new Font("Segoe UI", 10) };

            Label lblPass = new Label() { Text = "Mật khẩu:", Location = new Point(50, 165), AutoSize = true };
            txtLoginPass = new TextBox() { Location = new Point(50, 190), Width = 300, PasswordChar = '*', Font = new Font("Segoe UI", 10) };

            Button btnLogin = new Button() { Text = "ĐĂNG NHẬP", Location = new Point(50, 240), Width = 300, Height = 40, BackColor = Color.RoyalBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnLogin.Click += (s, e) => XuLyDangNhap();

            Label lblAsk = new Label() { 
                Text = "Bạn chưa có tài khoản?", 
                Location = new Point(85, 345), 
                AutoSize = true 
            };

            Button btnGoToReg = new Button() { 
                Text = "Đăng ký ngay", 
                Location = new Point(lblAsk.Location.X + lblAsk.PreferredWidth - 5, 338), 
                AutoSize = true, 
                FlatStyle = FlatStyle.Flat, 
                ForeColor = Color.RoyalBlue, 
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                Cursor = Cursors.Hand 
            };
            btnGoToReg.FlatAppearance.BorderSize = 0;
            btnGoToReg.FlatAppearance.MouseOverBackColor = Color.Transparent; // Không hiện nền khi di chuột qua
            btnGoToReg.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnGoToReg.Click += (s, e) => ChuyenManHinh(false);

            pnlDangNhap.Controls.AddRange(new Control[] { lblTitle, lblInput, txtLoginInput, lblPass, txtLoginPass, btnLogin, lblAsk, btnGoToReg });
            this.Controls.Add(pnlDangNhap);
        }

        // --- 2. GIAO DIỆN ĐĂNG KÝ ---
        private void TaoGiaoDienDangKy()
        {
            pnlDangKy = new Panel() { Size = this.ClientSize, Location = new Point(0, 0), Visible = false };

            // Tiêu đề căn giữa chuẩn
            Label lblTitle = new Label() { 
                Text = "ĐĂNG KÝ TÀI KHOẢN", 
                Font = new Font("Segoe UI", 16, FontStyle.Bold), 
                ForeColor = Color.ForestGreen, 
                Location = new Point(0, 25), 
                Size = new Size(this.ClientSize.Width, 40),
                TextAlign = ContentAlignment.MiddleCenter 
            };

            // Tăng khoảng cách yPos để các trường không dính nhau
            txtRegHoTen = TaoTextBoxLabel("Họ và tên:", 80, pnlDangKy);
            txtRegSDT = TaoTextBoxLabel("Số điện thoại:", 135, pnlDangKy);
            txtRegDiaChi = TaoTextBoxLabel("Địa chỉ:", 190, pnlDangKy);
            txtRegUser = TaoTextBoxLabel("Tên đăng nhập:", 245, pnlDangKy);
            
            Label lblPass = new Label() { Text = "Mật khẩu:", Location = new Point(50, 300), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.DimGray };
            txtRegPass = new TextBox() { Location = new Point(50, 322), Width = 300, PasswordChar = '*', Font = new Font("Segoe UI", 10) };
            pnlDangKy.Controls.Add(lblPass);
            pnlDangKy.Controls.Add(txtRegPass);

            Button btnRegister = new Button() { 
                Text = "TẠO TÀI KHOẢN", 
                Location = new Point(50, 370), 
                Width = 300, Height = 45, 
                BackColor = Color.ForestGreen, 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                FlatStyle = FlatStyle.Flat 
            };
            btnRegister.Click += (s, e) => XuLyDangKy();

            // --- PHẦN NÚT QUAY LẠI KHÔNG BỊ ĐÈ CHỮ ---
            Label lblAsk = new Label() { 
                Text = "Đã có tài khoản?", 
                Location = new Point(105, 435), 
                AutoSize = true,
                ForeColor = Color.Gray
            };

            Button btnBack = new Button() { 
                Text = "Đăng nhập ngay", 
                Location = new Point(lblAsk.Location.X + lblAsk.PreferredWidth - 5, 428), 
                AutoSize = true,
                FlatStyle = FlatStyle.Flat, 
                ForeColor = Color.ForestGreen, 
                Font = new Font("Segoe UI", 9, FontStyle.Underline),
                Cursor = Cursors.Hand 
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnBack.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnBack.Click += (s, e) => ChuyenManHinh(true);

            pnlDangKy.Controls.Add(lblTitle);
            pnlDangKy.Controls.Add(btnRegister);
            pnlDangKy.Controls.Add(lblAsk);
            pnlDangKy.Controls.Add(btnBack);

            this.Controls.Add(pnlDangKy);
        }

        // Hàm phụ trợ tạo TextBox nhanh cho phần Đăng ký
        private TextBox TaoTextBoxLabel(string labelText, int yPos, Panel pnl)
        {
            Label lbl = new Label() { Text = labelText, Location = new Point(50, yPos), AutoSize = true };
            TextBox txt = new TextBox() { Location = new Point(50, yPos + 25), Width = 300, Font = new Font("Segoe UI", 10) };
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(txt);
            return txt;
        }

        private void ChuyenManHinh(bool isLogin)
        {
            pnlDangNhap.Visible = isLogin;
            pnlDangKy.Visible = !isLogin;
            this.Text = isLogin ? "Đăng Nhập" : "Đăng Ký Tài Khoản";
            
            // Clear các trường mật khẩu khi chuyển màn hình để bảo mật
            txtLoginPass.Text = "";
            txtRegPass.Text = "";
        }

        // --- 3. XỬ LÝ ĐĂNG NHẬP (Logic Salt + Hash) ---
        private void XuLyDangNhap()
        {
            string input = txtLoginInput.Text.Trim();
            string matKhau = txtLoginPass.Text;

            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(matKhau))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    // BƯỚC 1: Lấy SALT từ Database trước
                    string saltFromDB = "";
                    using (SqlCommand cmdGetSalt = new SqlCommand("sp_TaiKhoan_LaySalt", conn))
                    {
                        cmdGetSalt.CommandType = CommandType.StoredProcedure;
                        cmdGetSalt.Parameters.AddWithValue("@InputIdentifier", input);

                        object result = cmdGetSalt.ExecuteScalar();
                        
                        if (result == null)
                        {
                            MessageBox.Show("Tài khoản hoặc số điện thoại không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return; 
                        }
                        saltFromDB = result.ToString();
                    }

                    // BƯỚC 2: Hash mật khẩu nhập vào với Salt vừa lấy được
                    string currentHash = SecurityHelper.HashPassword(matKhau, saltFromDB);

                    // BƯỚC 3: Gọi Procedure Đăng nhập để kiểm tra và lấy thông tin
                    using (SqlCommand cmdLogin = new SqlCommand("sp_TaiKhoan_DangNhap", conn))
                    {
                        cmdLogin.CommandType = CommandType.StoredProcedure;
                        cmdLogin.Parameters.AddWithValue("@InputIdentifier", input);
                        cmdLogin.Parameters.AddWithValue("@MatKhauHash", currentHash);

                        using (SqlDataReader reader = cmdLogin.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader["UserId"] != DBNull.Value)
                                {
                                    UserSession.UserId = Convert.ToInt32(reader["UserId"]);
                                }   
                                UserSession.UserName = reader["UserName"].ToString();
                                UserSession.FullName = reader["FullName"].ToString();
                                if (reader["UserId"] != DBNull.Value)
                                    {
                                        UserSession.WorkBranchId = Convert.ToInt32(reader["WorkBranchId"]);
                                    }
                                UserSession.Role = reader["Role"].ToString(); 
                                MessageBox.Show($"Đăng nhập thành công!\nXin chào: {UserSession.FullName}", "Thông báo");

                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Bắt lỗi RAISERROR từ SQL (Sai mật khẩu, tài khoản khóa...)
                MessageBox.Show(ex.Message, "Lỗi Đăng Nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 4. XỬ LÝ ĐĂNG KÝ ---
        private void XuLyDangKy()
        {
            string hoTen = txtRegHoTen.Text.Trim();
            string sdt = txtRegSDT.Text.Trim();
            string user = txtRegUser.Text.Trim();
            string diaChi = txtRegDiaChi.Text.Trim();
            string pass = txtRegPass.Text;

            if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ các trường bắt buộc!", "Thông báo");
                return;
            }

            // Tạo Salt mới ngẫu nhiên cho user này
            string newSalt = Guid.NewGuid().ToString();
            // Hash mật khẩu với salt mới
            string hash = SecurityHelper.HashPassword(pass, newSalt);

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_KhachHang_DangKy", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@HoTen", hoTen);
                        cmd.Parameters.AddWithValue("@SDT", sdt);
                        cmd.Parameters.AddWithValue("@DiaChi", diaChi);
                        cmd.Parameters.AddWithValue("@TenDangNhap", user);
                        cmd.Parameters.AddWithValue("@MatKhauHash", hash);
                        cmd.Parameters.AddWithValue("@Salt", newSalt); // Gửi Salt xuống để lưu

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        // Chuyển ngay về màn hình đăng nhập
                        ChuyenManHinh(isLogin: true);
                        txtLoginInput.Text = user; // Điền sẵn tên đăng nhập cho tiện
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi đăng ký: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }
    }
}