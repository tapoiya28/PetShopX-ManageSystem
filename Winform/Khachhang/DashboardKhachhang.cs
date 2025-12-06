using System;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class DashboardKhachhang : Form
    {
        private Panel pnlSidebar;
        private Panel pnlHeader;
        private Panel pnlContent; // Nơi hiển thị nội dung chính

        public DashboardKhachhang()
        {
            this.Size = new Size(1100, 700);
            this.Text = "PetcareX - Khách Hàng";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            TaoGiaoDien();
            
            // Mặc định hiện trang Đặt Lịch
            ChuyenTrang(new UCDatLich());
        }

        private void TaoGiaoDien()
        {
            // 1. HEADER (Trên cùng)
            pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            Label lblLogo = new Label() { 
                Text = "PETCARE X", 
                Font = new Font("Segoe UI", 18, FontStyle.Bold), 
                ForeColor = Color.FromArgb(51, 102, 255),
                Location = new Point(20, 15), 
                AutoSize = true 
            };
            Label lblUser = new Label() {
                Text = $"Xin chào, {UserSession.FullName}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(800, 20),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlHeader.Controls.AddRange(new Control[] { lblLogo, lblUser });

            // 2. SIDEBAR (Bên trái)
            pnlSidebar = new Panel() { Dock = DockStyle.Left, Width = 220, BackColor = Color.FromArgb(40, 50, 70) };
            
            // Tạo các nút menu
            Button btnDatLich = CreateMenuButton("📅  Đặt Lịch Hẹn", 80);
            btnDatLich.Click += (s, e) => ChuyenTrang(new UCDatLich());

            Button btnHoSo = CreateMenuButton("🐶  Hồ Sơ Thú Cưng", 140);
            // btnHoSo.Click += ...

            Button btnLichSu = CreateMenuButton("🕒  Lịch Sử Khám", 200);
            // btnLichSu.Click += ...

            Button btnDangXuat = new Button() { 
                Text = "Đăng Xuất", 
                Dock = DockStyle.Bottom, 
                Height = 50, 
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.IndianRed, 
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnDangXuat.Click += (s, e) => { this.Close(); };

            pnlSidebar.Controls.AddRange(new Control[] { btnDatLich, btnHoSo, btnLichSu, btnDangXuat });

            // 3. MAIN CONTENT (Ở giữa)
            pnlContent = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(20) };

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
        }

        private Button CreateMenuButton(string text, int top)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Top = top;
            btn.Left = 0;
            btn.Width = 220;
            btn.Height = 50;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.WhiteSmoke;
            btn.Font = new Font("Segoe UI", 11);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            
            // Hiệu ứng hover
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(60, 70, 90);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;

            return btn;
        }

        // Hàm chuyển trang mượt mà
        private void ChuyenTrang(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }
    }
}