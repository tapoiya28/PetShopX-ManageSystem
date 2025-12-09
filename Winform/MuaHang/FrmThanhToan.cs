using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class ThanhToanHoaDonForm : Form
    {
        private readonly int _maHD;

        private Label lblMaHDValue;
        private DataGridView dgvChiTiet;
        private Label lblTongTienValue;
        private Button btnThanhToan;
        private Button btnMuaThem;   // nút quay lại thêm chi tiết

        public ThanhToanHoaDonForm(int maHD)
        {
            _maHD = maHD;

            this.Text = "Thanh toán hóa đơn";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            TaoGiaoDien();
            LoadChiTietHoaDon();
        }

        private void TaoGiaoDien()
        {
            var lblTitle = new Label
            {
                Text = "THANH TOÁN HÓA ĐƠN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = true,
                Location = new Point(170, 15)
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

            dgvChiTiet = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(550, 220),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var lblTongTien = new Label
            {
                Text = "Tổng tạm tính:",
                AutoSize = true,
                Location = new Point(20, 315),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            lblTongTienValue = new Label
            {
                Text = "0",
                AutoSize = true,
                Location = new Point(130, 315),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Maroon
            };

            // Nút "Mua thêm" (quay lại form chi tiết) - lệch trái một chút
            btnMuaThem = new Button
            {
                Text = "Mua thêm",
                Width = 120,
                Height = 35,
                Location = new Point(320, 310), // lệch trái so với trước
                FlatStyle = FlatStyle.Standard
            };
            btnMuaThem.Click += (s, e) => this.Close(); // quay lại thêm chi tiết

            // Nút "Thanh toán" – ngoài cùng bên phải, cũng kéo trái nhẹ
            btnThanhToan = new Button
            {
                Text = "Thanh toán",
                Width = 120,
                Height = 35,
                Location = new Point(450, 310), // 450 + 120 = 570 < 600, không tràn
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnThanhToan.FlatAppearance.BorderSize = 0;
            btnThanhToan.Click += BtnThanhToan_Click;

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblMaHD, lblMaHDValue,
                dgvChiTiet,
                lblTongTien, lblTongTienValue,
                btnMuaThem, btnThanhToan
            });
        }

        private void LoadChiTietHoaDon()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT 
                        c.MASP,
                        s.TENSP,
                        c.SOLUONG,
                        c.GIABAN,
                        CAST(c.SOLUONG * c.GIABAN AS DECIMAL(12,2)) AS THANHTIEN
                    FROM CHITIETHOADON c
                    JOIN SANPHAM s ON c.MASP = s.MASP
                    WHERE c.MAHD = @MaHD", conn))
                {
                    cmd.Parameters.AddWithValue("@MaHD", _maHD);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dgvChiTiet.DataSource = dt;

                        decimal tong = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row["THANHTIEN"] != DBNull.Value)
                            {
                                tong += Convert.ToDecimal(row["THANHTIEN"]);
                            }
                        }

                        lblTongTienValue.Text = tong.ToString("N0") + " đ";
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL khi tải chi tiết hóa đơn: " + ex.Message,
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message,
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (dgvChiTiet.Rows.Count == 0)
            {
                MessageBox.Show("Hóa đơn chưa có chi tiết, không thể thanh toán.",
                                "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "Bạn chắc chắn muốn thanh toán hóa đơn này?\n" +
                "Sau khi thanh toán sẽ không thể thêm sản phẩm nữa.",
                "Xác nhận thanh toán",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_ThanhToanHoaDon", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaHD", _maHD);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Thanh toán hóa đơn thành công.",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Sau khi thanh toán xong -> mở form đánh giá
                using (var frmDanhGia = new DanhGiaHoaDonForm(_maHD))
                {
                    frmDanhGia.ShowDialog();
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Lỗi thanh toán",
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
