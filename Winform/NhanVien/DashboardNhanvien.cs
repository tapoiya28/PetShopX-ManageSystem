using System;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class DashboardNhanVien : Form
    {
        private Panel pnlContent;
        private Panel pnlSidebar;
        private Button btnQuanLyLichHen;
        private Button btnDangXuat;
        private Label lblUser;

        public DashboardNhanVien()
        {
            // Setup Form chính
            this.Size = new Size(1280, 720); // Màn hình rộng hơn
            this.Text = "Hệ Thống Quản Lý PetcareX - Nhân Viên";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized; // Tự động phóng to toàn màn hình
            this.Font = new Font("Segoe UI", 10);

            // 1. Tạo Sidebar trước (Dock Left)
            TaoSidebar();

            // 2. Tạo Content (Dock Fill - Tự động lấp đầy phần còn lại)
            pnlContent = new Panel() { 
                Dock = DockStyle.Fill, 
                BackColor = Color.WhiteSmoke, // Màu nền xám nhẹ dịu mắt
                Padding = new Padding(10) // Cách lề một chút cho đẹp
            };
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar); // Add Sidebar sau cùng trong code nhưng Dock Left sẽ ưu tiên

            // Mặc định mở trang lịch hẹn
            ChuyenTrang(new UCLichHen());
        }

        private void TaoSidebar()
        {
            pnlSidebar = new Panel() { 
                Dock = DockStyle.Left, 
                Width = 240, 
                BackColor = Color.FromArgb(44, 62, 80) // Màu xanh đen hiện đại (Midnight Blue)
            };
            
            // Header User info
            Panel pnlUser = new Panel() { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(34, 49, 63) };
            Label lblRole = new Label() { Text = "STAFF PANEL", ForeColor = Color.Gray, Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblUser = new Label() { 
                Text = UserSession.FullName, 
                ForeColor = Color.White, 
                Location = new Point(20, 45), 
                AutoSize = true, 
                Font = new Font("Segoe UI", 12, FontStyle.Bold) 
            };
            pnlUser.Controls.AddRange(new Control[] { lblRole, lblUser });

            // Menu Buttons
            btnQuanLyLichHen = TaoNutMenu("📅  Quản Lý Lịch Hẹn", 110);
            btnQuanLyLichHen.BackColor = Color.FromArgb(52, 73, 94); // Active state giả định

            // Nút Đăng xuất ở dưới cùng
            btnDangXuat = new Button() { 
                Text = "🚪 Đăng Xuất", 
                Dock = DockStyle.Bottom, 
                Height = 50, 
                FlatStyle = FlatStyle.Flat, 
                ForeColor = Color.White, 
                BackColor = Color.FromArgb(192, 57, 43), // Màu đỏ trầm
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDangXuat.FlatAppearance.BorderSize = 0;
            btnDangXuat.Click += (s, e) => { this.Close(); };

            pnlSidebar.Controls.Add(btnQuanLyLichHen);
            pnlSidebar.Controls.Add(btnDangXuat); // Add nút Bottom trước
            pnlSidebar.Controls.Add(pnlUser);     // Add Header sau (Dock Top)
        }

        private Button TaoNutMenu(string text, int top)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Top; // Xếp chồng lên nhau
            btn.Height = 55;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.WhiteSmoke;
            btn.Font = new Font("Segoe UI", 11);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            
            // Hiệu ứng Hover
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(52, 73, 94);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;

            // Mang nút Lịch Hẹn xuống dưới Panel User (Hack nhẹ bằng BringToFront nếu cần, nhưng Dock Top tự xếp)
            return btn;
        }

        private void ChuyenTrang(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill; // Quan trọng: UserControl sẽ tự giãn full Content Panel
            pnlContent.Controls.Add(uc);
        }
    }
}