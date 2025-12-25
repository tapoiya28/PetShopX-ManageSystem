using System;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class DashboardKhachhang : Form
    {
        // Khai báo biến
        private Panel pnlSidebar;
        private Panel pnlHeader;
        private Panel pnlContent;

        public DashboardKhachhang()
        {
            // 1. Cấu hình Form
            this.Size = new Size(1280, 720);
            this.Text = "PetcareX - Khách Hàng";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            // 2. Khởi tạo giao diện (QUAN TRỌNG: Phải chạy xong hàm này mới có pnlContent)
            TaoGiaoDien();
            
            // 3. Chuyển trang mặc định (Chỉ gọi khi pnlContent đã được new)
            ChuyenTrang(new UCDatLich());
        }

        private void TaoGiaoDien()
        {
            // --- BƯỚC 1: KHỞI TẠO CÁC PANEL CHÍNH TRƯỚC (QUAN TRỌNG) ---
            pnlHeader = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
            pnlSidebar = new Panel() { Dock = DockStyle.Left, Width = 240, BackColor = Color.FromArgb(41, 50, 65) };
            pnlContent = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // --- BƯỚC 2: THIẾT LẬP HEADER ---
            Label lblLogo = new Label() { 
                Text = "PETCARE X", 
                Font = new Font("Segoe UI", 18, FontStyle.Bold), 
                ForeColor = Color.FromArgb(51, 102, 255),
                Location = new Point(20, 15), 
                AutoSize = true 
            };
            
            // Xử lý an toàn cho UserSession (tránh lỗi nếu chưa đăng nhập)
            string tenUser = !string.IsNullOrEmpty(UserSession.FullName) ? UserSession.FullName : "Khách hàng";
            Label lblUser = new Label() {
                Text = $"Xin chào, {tenUser}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(800, 20),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlHeader.Controls.AddRange(new Control[] { lblLogo, lblUser });

            // --- BƯỚC 3: TẠO CÁC NÚT MENU ---
            Button btnDatLich = CreateMenuButton("📅  Đặt Lịch Hẹn", 80);
            btnDatLich.Click += (s, e) => ChuyenTrang(new UCDatLich());

            Button btnHoSo = CreateMenuButton("🐶  Thông Tin & Hồ Sơ", 140);
            btnHoSo.Click += (s, e) => ChuyenTrang(new UCThongTinCaNhan()); 

            Button btnTraCuuBS = CreateMenuButton("👨‍⚕️  Tra Cứu Bác Sĩ", 200);
            btnTraCuuBS.Click += (s, e) => ChuyenTrang(new UCTraCuuBacSi());

            Button btnMuaHang = CreateMenuButton("🛒  Mua Hàng Online", 260);
            btnMuaHang.Click += (s, e) => ChuyenTrang(new UCMuaHangOnline());

            Button btnDangXuat = new Button() { 
                Text = "Đăng Xuất", 
                Dock = DockStyle.Bottom, 
                Height = 50, 
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.IndianRed, 
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDangXuat.Click += (s, e) => { this.Close(); };

            // --- BƯỚC 4: ADD CONTROL VÀO PANEL ---
            // Đảm bảo tất cả biến button đã được tạo ở trên
            pnlSidebar.Controls.AddRange(new Control[] { btnDatLich, btnHoSo, btnTraCuuBS, btnMuaHang, btnDangXuat });

            // --- BƯỚC 5: ADD PANEL VÀO FORM ---
            this.Controls.Add(pnlContent); // Content nằm giữa
            this.Controls.Add(pnlSidebar); // Sidebar bên trái
            this.Controls.Add(pnlHeader);  // Header trên cùng
        }

        private Button CreateMenuButton(string text, int top)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Top = top;
            btn.Left = 0;
            btn.Width = 240; // Khớp với chiều rộng Sidebar
            btn.Height = 50;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.WhiteSmoke;
            btn.Font = new Font("Segoe UI", 11);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(60, 70, 90);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;

            return btn;
        }

        private void ChuyenTrang(UserControl uc)
        {
            // Kiểm tra an toàn
            if (pnlContent == null) return; 

            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }
    }
}