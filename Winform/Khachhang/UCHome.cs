using System;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class UCHome : UserControl
    {
        public event EventHandler YeuCauDatKham;
        public event EventHandler XemThuCung;
        public UCHome()
        {
            this.Dock = DockStyle.Fill; 
            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            Button nutDatKham = new Button() { Text = "Đặt lịch", Location = new Point(50, 50), Size = new Size(150, 50) };
            nutDatKham.Click += (s, e) => YeuCauDatKham?.Invoke(this, EventArgs.Empty);

            Button nutXemThuCung = new Button() { Text = "Danh sách thú cưng", Location = new Point(50, 100), Size = new Size(150, 50) };
            nutDatKham.Click += (s, e) => XemThuCung?.Invoke(this, EventArgs.Empty);
            this.Controls.Add(nutDatKham);    
            this.Controls.Add(nutXemThuCung);    
        }
    }
}