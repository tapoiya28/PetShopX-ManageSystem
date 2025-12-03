using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    public class LoginForm : Form
    {
        private Panel pnlDangNhap;
        private Panel pnlDangKy;
        private TextBox txtLoginUser;
        private TextBox txtLoginPass;
        private TextBox txtRegHoTen;
        private TextBox txtRegSDT;
        private TextBox txtRegDiaChi;
        private TextBox txtRegPass;

        public LoginForm()
        {
            this.Size = new Size(400, 450);
            this.Text = "Hệ Thống PetcareX";
            this.StartPosition = FormStartPosition.CenterScreen;
            TaoGiaoDienDangNhap();
            TaoGiaoDienDangKy();
            HienThiManHinh(true);
        }

        private void TaoGiaoDienDangNhap()
        {
            pnlDangNhap = new Panel() { Size = this.ClientSize, Location = new Point(0, 0) };

            Label lblTitle = new Label() { Text = "ĐĂNG NHẬP", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(130, 30), AutoSize = true };
            
            Label lblUser = new Label() { Text = "Số điện thoại:", Location = new Point(50, 80) };
            txtLoginUser = new TextBox() { Location = new Point(50, 105), Width = 280 };

            Label lblPass = new Label() { Text = "Mật khẩu:", Location = new Point(50, 140) };
            txtLoginPass = new TextBox() { Location = new Point(50, 165), Width = 280, PasswordChar = '*' };

            Button btnLogin = new Button() { Text = "Đăng Nhập", Location = new Point(50, 210), Width = 280, Height = 35, BackColor = Color.LightBlue };
            btnLogin.Click += (s, e) => XuLyDangNhap();

            Label lblHoi = new Label() { Text = "Chưa có tài khoản?", Location = new Point(80, 260), AutoSize = true };
            Button btnGoToReg = new Button() { Text = "Đăng ký ngay", Location = new Point(200, 255), Width = 100, FlatStyle = FlatStyle.Flat };
            btnGoToReg.FlatAppearance.BorderSize = 0;
            btnGoToReg.ForeColor = Color.Blue;
            btnGoToReg.Click += (s, e) => HienThiManHinh(isLogin: false); 

            pnlDangNhap.Controls.Add(lblTitle);
            pnlDangNhap.Controls.Add(lblUser);
            pnlDangNhap.Controls.Add(txtLoginUser);
            pnlDangNhap.Controls.Add(lblPass);
            pnlDangNhap.Controls.Add(txtLoginPass);
            pnlDangNhap.Controls.Add(btnLogin);
            pnlDangNhap.Controls.Add(lblHoi);
            pnlDangNhap.Controls.Add(btnGoToReg);

            this.Controls.Add(pnlDangNhap);
        }
        private void TaoGiaoDienDangKy()
        {
            pnlDangKy = new Panel() { Size = this.ClientSize, Location = new Point(0, 0), Visible = false };

            Label lblTitle = new Label() { Text = "ĐĂNG KÝ KHÁCH HÀNG", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(80, 30), AutoSize = true };

            Label lblTen = new Label() { Text = "Họ và tên:", Location = new Point(50, 70) };
            txtRegHoTen = new TextBox() { Location = new Point(50, 95), Width = 280 };

            Label lblSDT = new Label() { Text = "Số điện thoại (sẽ là tên đăng nhập):", Location = new Point(50, 130), AutoSize = true };
            txtRegSDT = new TextBox() { Location = new Point(50, 155), Width = 280 };

            Label lblDiaChi = new Label() { Text = "Địa chỉ:", Location = new Point(50, 190) };
            txtRegDiaChi = new TextBox() { Location = new Point(50, 215), Width = 280 };

            Label lblPass = new Label() { Text = "Mật khẩu:", Location = new Point(50, 250) };
            txtRegPass = new TextBox() { Location = new Point(50, 275), Width = 280, PasswordChar = '*' };

            Button btnRegister = new Button() { Text = "Đăng Ký", Location = new Point(50, 320), Width = 280, Height = 35, BackColor = Color.LightGreen };
            btnRegister.Click += (s, e) => XuLyDangKy();

            Button btnBack = new Button() { Text = "<< Quay lại Đăng nhập", Location = new Point(50, 370), Width = 280 };
            btnBack.Click += (s, e) => HienThiManHinh(isLogin: true); 

            pnlDangKy.Controls.Add(lblTitle);
            pnlDangKy.Controls.Add(lblTen);
            pnlDangKy.Controls.Add(txtRegHoTen);
            pnlDangKy.Controls.Add(lblSDT);
            pnlDangKy.Controls.Add(txtRegSDT);
            pnlDangKy.Controls.Add(lblDiaChi);
            pnlDangKy.Controls.Add(txtRegDiaChi);
            pnlDangKy.Controls.Add(lblPass);
            pnlDangKy.Controls.Add(txtRegPass);
            pnlDangKy.Controls.Add(btnRegister);
            pnlDangKy.Controls.Add(btnBack);

            this.Controls.Add(pnlDangKy);
        }
        private void HienThiManHinh(bool isLogin)
        {
            if (isLogin)
            {
                pnlDangNhap.Visible = true;
                pnlDangKy.Visible = false;
                this.Text = "Đăng Nhập";
            }
            else
            {
                pnlDangNhap.Visible = false;
                pnlDangKy.Visible = true;
                this.Text = "Đăng Ký Tài Khoản";
            }
        }
        private void XuLyDangNhap()
        {
            string sdt = txtLoginUser.Text.Trim();
            string matKhau = txtLoginPass.Text;

            if (string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(matKhau)) { MessageBox.Show("Vui lòng nhập đủ thông tin!"); return; }

            string hash = SecurityHelper.HashPassword(matKhau);

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_TaiKhoan_DangNhap", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TenDangNhap", sdt);
                        cmd.Parameters.AddWithValue("@MatKhauHash", hash);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string hoTen = reader["HOTEN"].ToString();
                                string vaiTro = reader["VAITRO"].ToString(); 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        private void XuLyDangKy()
        {
            string hoTen = txtRegHoTen.Text.Trim();
            string sdt = txtRegSDT.Text.Trim();
            string diaChi = txtRegDiaChi.Text.Trim();
            string pass = txtRegPass.Text;
            if (string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(sdt) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin bắt buộc!");
                return;
            }
            string hash = SecurityHelper.HashPassword(pass);
            string salt = Guid.NewGuid().ToString(); 

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
                        cmd.Parameters.AddWithValue("@TenDangNhap", sdt); 
                        cmd.Parameters.AddWithValue("@MatKhauHash", hash);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.");
                        HienThiManHinh(isLogin: true); 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký: " + ex.Message);
            }
        }
    }
}