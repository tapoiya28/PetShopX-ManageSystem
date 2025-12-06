using System;
using System.Windows.Forms;

namespace Winform
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            LoginForm login = new LoginForm();
            if (login.ShowDialog() == DialogResult.OK)
            {
                if (UserSession.IsKhachHang()) 
                {
                    Application.Run(new DashboardKhachhang());
                }
                else if (UserSession.IsBacSi() || UserSession.IsNhanVien() || UserSession.IsQuanLy())
                {
                     Application.Run(new DashboardNhanVien());
                }
                else
                {
                    MessageBox.Show("Lỗi: Không xác định được vai trò người dùng!", "Lỗi Phân Quyền");
                }
            }
            else
            {
                // Người dùng tắt form đăng nhập -> Thoát ứng dụng
                Application.Exit();
            }
        }
    }
}