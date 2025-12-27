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
        private Button btnLapHoaDon;    
        private Button btnXemLichSu;     
        private Button btnKhamBenh;     
        private Button btnThongKe;       
        private Button btnTraCuuKhach; 
        private Button btnDangXuat;
        private Label lblUser;

        public DashboardNhanVien()
        {
            // Setup Form chính
            this.Size = new Size(1280, 720);
            this.Text = "Hệ Thống Quản Lý PetcareX - Nhân Viên";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.Font = new Font("Segoe UI", 10);

            // 1. Tạo Sidebar
            TaoSidebar();

            // 2. Tạo Content
            pnlContent = new Panel() { 
                Dock = DockStyle.Fill, 
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlSidebar);

            // 3. LOGIC CHUYỂN TRANG MẶC ĐỊNH
            if (UserSession.IsBacSi())
            {
                ChuyenTrang(new UCKhamBenh()); // Bác sĩ vào thẳng ca khám
            }
            if (UserSession.IsNhanVien())
            {
                ChuyenTrang(new UCLichHen()); // Nhân viên vào quản lý lịch hẹn
            }
            if (UserSession.IsQuanLy())
            {
                ChuyenTrang(new UCThongKe()); // Quản lí vào thống kê
            }
        }

        private void TaoSidebar()
        {
            pnlSidebar = new Panel() { 
                Dock = DockStyle.Left, 
                Width = 240, 
                BackColor = Color.FromArgb(44, 62, 80)
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

            // --- TẠO CÁC NÚT MENU ---

            if (UserSession.IsNhanVien())
            {
                btnQuanLyLichHen = TaoNutMenu("📅  Quản Lý Lịch Hẹn");
                btnQuanLyLichHen.Click += (s, e) => ChuyenTrang(new UCLichHen());
                btnTraCuuKhach = TaoNutMenu("🔍  Tra Cứu Thú Cưng");
                btnTraCuuKhach.Click += (s, e) => ChuyenTrang(new UCTraCuuThuCungToanHeThong());
                btnLapHoaDon = TaoNutMenu("💰  Lập Hóa Đơn");
                btnLapHoaDon.Click += (s, e) => {
                    var frm = new TaoHoaDonForm();
                    frm.ShowDialog();
                };
            }
            
            if (UserSession.IsBacSi())
            {
                btnXemLichSu = TaoNutMenu("📋  Xem Lịch Sử Khám");
                btnXemLichSu.Click += (s, e) => ChuyenTrang(new UCXemLichSuKham());
                btnKhamBenh = TaoNutMenu("🏭  Tạo Ca Khám Mới");
                btnKhamBenh.Click += (s, e) => ChuyenTrang(new UCKhamBenh());
            }
    
            
            if (UserSession.IsQuanLy())
            {
                btnThongKe = TaoNutMenu("📊  Thống Kê");
                btnThongKe.Click += (s, e) => ChuyenTrang(new UCThongKe());
            }

            // Nút Đăng xuất
            btnDangXuat = new Button() { 
                Text = "🚪 Đăng Xuất", 
                Dock = DockStyle.Bottom, 
                Height = 50, 
                FlatStyle = FlatStyle.Flat, 
                ForeColor = Color.White, 
                BackColor = Color.FromArgb(192, 57, 43),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDangXuat.FlatAppearance.BorderSize = 0;
            btnDangXuat.Click += (s, e) => { this.Close(); };

            // --- THÊM NÚT VÀO SIDEBAR ---
            // Lưu ý: Dock = Top nên nút nào add SAU sẽ nằm DƯỚI nút add trước
            // Hoặc add ngược từ dưới lên trên.
            
            // Add các nút nhóm dưới cùng trước
            if (UserSession.IsQuanLy() && btnThongKe != null) 
                pnlSidebar.Controls.Add(btnThongKe);

            if (UserSession.IsBacSi() && btnKhamBenh != null) 
                pnlSidebar.Controls.Add(btnKhamBenh);

            if ((UserSession.IsBacSi() || UserSession.IsQuanLy()) && btnXemLichSu != null) 
                pnlSidebar.Controls.Add(btnXemLichSu);

            // Nhóm chức năng chính của Nhân viên (Add sau để nằm trên)
            if (btnLapHoaDon != null) 
                pnlSidebar.Controls.Add(btnLapHoaDon); // Sẽ nằm dưới Lịch Hẹn

            if (btnQuanLyLichHen != null) 
                pnlSidebar.Controls.Add(btnQuanLyLichHen); // Sẽ nằm trên cùng
            if (btnTraCuuKhach != null) 
                pnlSidebar.Controls.Add(btnTraCuuKhach); 
            // Các thành phần cố định
            pnlSidebar.Controls.Add(btnDangXuat); // Dock Bottom
            pnlSidebar.Controls.Add(pnlUser);     // Dock Top (sẽ đè lên trên cùng của Top)
        }

        private Button TaoNutMenu(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Top;
            btn.Height = 55;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.WhiteSmoke;
            btn.Font = new Font("Segoe UI", 11);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(52, 73, 94);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;

            return btn;
        }

        private void ChuyenTrang(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }
    }
}