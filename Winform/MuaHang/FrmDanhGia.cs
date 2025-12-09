using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class DanhGiaHoaDonForm : Form
    {
        private readonly int _maHD;

        private Label lblMaHDValue;
        private Label[] _starsDichVu;
        private Label[] _starsHaiLong;
        private int _diemDichVu;
        private int _mucDoHaiLong;
        private TextBox txtBinhLuan;
        private Button btnDanhGia;
        private Button btnThoat;

        public DanhGiaHoaDonForm(int maHD)
        {
            _maHD = maHD;

            this.Text = "Đánh giá hóa đơn";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            var lblTitle = new Label
            {
                Text = "ĐÁNH GIÁ HÓA ĐƠN",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = true,
                Location = new Point(150, 15)
            };

            var lblMaHD = new Label
            {
                Text = "Mã hóa đơn:",
                AutoSize = true,
                Location = new Point(20, 55)
            };
            lblMaHDValue = new Label
            {
                Text = _maHD.ToString(),
                AutoSize = true,
                Location = new Point(110, 55),
                ForeColor = Color.DarkGreen
            };

            // Dòng 1: Điểm dịch vụ (sao)
            var lblDichVu = new Label
            {
                Text = "Điểm dịch vụ:",
                AutoSize = true,
                Location = new Point(20, 90)
            };
            _starsDichVu = TaoDaySao(150, 85, StarDichVu_Click);

            // Dòng 2: Mức độ hài lòng (sao)
            var lblHaiLong = new Label
            {
                Text = "Mức độ hài lòng:",
                AutoSize = true,
                Location = new Point(20, 125)
            };
            _starsHaiLong = TaoDaySao(150, 120, StarHaiLong_Click);

            // Ô bình luận
            var lblBinhLuan = new Label
            {
                Text = "Bình luận:",
                AutoSize = true,
                Location = new Point(20, 160)
            };
            txtBinhLuan = new TextBox
            {
                Location = new Point(20, 185),
                Size = new Size(440, 80),
                Multiline = true
            };

            // Nút Đánh giá
            btnDanhGia = new Button
            {
                Text = "Đánh giá",
                Width = 100,
                Height = 35,
                Location = new Point(260, 280),
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDanhGia.FlatAppearance.BorderSize = 0;
            btnDanhGia.Click += BtnDanhGia_Click;

            // Nút Thoát
            btnThoat = new Button
            {
                Text = "Thoát",
                Width = 80,
                Height = 35,
                Location = new Point(380, 280)
            };
            btnThoat.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblMaHD, lblMaHDValue,
                lblDichVu,
                lblHaiLong,
                lblBinhLuan, txtBinhLuan,
                btnDanhGia, btnThoat
            });

            // add sao sau cùng để đè lên đúng vị trí
            foreach (var star in _starsDichVu) this.Controls.Add(star);
            foreach (var star in _starsHaiLong) this.Controls.Add(star);
        }

        // Tạo 1 dãy 5 sao tại (startX, y)
        private Label[] TaoDaySao(int startX, int y, EventHandler onClick)
        {
            var stars = new Label[5];
            for (int i = 0; i < 5; i++)
            {
                var lbl = new Label
                {
                    Text = "☆",
                    Font = new Font("Segoe UI", 18, FontStyle.Regular),
                    AutoSize = true,
                    Location = new Point(startX + i * 30, y),
                    ForeColor = Color.Goldenrod,
                    Cursor = Cursors.Hand,
                    Tag = i + 1  // 1..5
                };
                lbl.Click += onClick;
                stars[i] = lbl;
            }
            return stars;
        }

        private void UpdateStars(Label[] stars, int value)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].Text = (i < value) ? "★" : "☆";
            }
        }

        private void StarDichVu_Click(object? sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.Tag is int v)
            {
                _diemDichVu = v;
                UpdateStars(_starsDichVu, _diemDichVu);
            }
        }

        private void StarHaiLong_Click(object? sender, EventArgs e)
        {
            if (sender is Label lbl && lbl.Tag is int v)
            {
                _mucDoHaiLong = v;
                UpdateStars(_starsHaiLong, _mucDoHaiLong);
            }
        }

        private void BtnDanhGia_Click(object? sender, EventArgs e)
        {
            if (_diemDichVu < 1 || _diemDichVu > 5)
            {
                MessageBox.Show("Vui lòng chọn điểm dịch vụ (1–5 sao).",
                                "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_mucDoHaiLong < 1 || _mucDoHaiLong > 5)
            {
                MessageBox.Show("Vui lòng chọn mức độ hài lòng (1–5 sao).",
                                "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string binhLuan = txtBinhLuan.Text.Trim();
            int maKH;

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    // 1. Lấy MAKH từ HOADON
                    using (SqlCommand cmdGetKH = new SqlCommand(
                        "SELECT MAKH FROM HOADON WHERE MAHD = @MaHD", conn))
                    {
                        cmdGetKH.Parameters.AddWithValue("@MaHD", _maHD);
                        object result = cmdGetKH.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("Không tìm thấy khách hàng cho hóa đơn này.",
                                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        maKH = Convert.ToInt32(result);
                    }

                    // 2. Gọi proc thêm đánh giá
                    using (SqlCommand cmd = new SqlCommand("sp_ThemDanhGiaHoaDon", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaKH", maKH);
                        cmd.Parameters.AddWithValue("@MaHD", _maHD);
                        cmd.Parameters.AddWithValue("@DiemDichVu", _diemDichVu);
                        cmd.Parameters.AddWithValue("@MucDoHaiLong", _mucDoHaiLong);
                        cmd.Parameters.AddWithValue("@BinhLuan",
                            string.IsNullOrEmpty(binhLuan) ? (object)DBNull.Value : binhLuan);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Cảm ơn bạn đã đánh giá!",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Lỗi đánh giá",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message,
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
