using System;
using System.Collections.Generic;
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
        private Button btnMuaThem;

        // ==== Biến phục vụ scale layout ====
        private Size _originalClientSize;
        private readonly Dictionary<Control, Rectangle> _originalBounds = new();
        private readonly Dictionary<Control, float> _originalFontSizes = new();
        private bool _layoutSaved = false;

        public ThanhToanHoaDonForm(int maHD)
        {
            _maHD = maHD;

            this.Text = "Thanh toán hóa đơn";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = true;
            this.BackColor = Color.WhiteSmoke;

            TaoGiaoDien();
            LoadChiTietHoaDon();

            this.Load += ThanhToanHoaDonForm_Load;
            this.Resize += ThanhToanHoaDonForm_Resize;
        }

        // ================== SCALE LAYOUT ==================

        private void ThanhToanHoaDonForm_Load(object? sender, EventArgs e)
        {
            SaveInitialLayout();
            this.WindowState = FormWindowState.Maximized;
        }

        private void SaveInitialLayout()
        {
            if (_layoutSaved) return;

            _originalClientSize = this.ClientSize;
            SaveControlLayout(this);
            _layoutSaved = true;
        }

        private void SaveControlLayout(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                _originalBounds[c] = c.Bounds;
                _originalFontSizes[c] = c.Font.Size;

                if (c.Controls.Count > 0)
                {
                    SaveControlLayout(c);
                }
            }
        }

        private void ThanhToanHoaDonForm_Resize(object? sender, EventArgs e)
        {
            if (!_layoutSaved || _originalClientSize.Width == 0 || _originalClientSize.Height == 0)
                return;

            float scaleX = (float)this.ClientSize.Width / _originalClientSize.Width;
            float scaleY = (float)this.ClientSize.Height / _originalClientSize.Height;
            float scale = Math.Min(scaleX, scaleY);

            foreach (var kvp in _originalBounds)
            {
                Control c = kvp.Key;
                Rectangle rect = kvp.Value;

                c.Bounds = new Rectangle(
                    (int)(rect.X * scaleX),
                    (int)(rect.Y * scaleY),
                    (int)(rect.Width * scaleX),
                    (int)(rect.Height * scaleY)
                );

                if (_originalFontSizes.TryGetValue(c, out float fontSize) && fontSize > 0)
                {
                    c.Font = new Font(c.Font.FontFamily, fontSize * scale, c.Font.Style);
                }
            }
        }

        // ================== GIAO DIỆN ==================

        private void TaoGiaoDien()
        {
            var lblTitle = new Label
            {
                Text = "THANH TOÁN HÓA ĐƠN",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.Navy,
                AutoSize = true,
                Location = new Point(300, 20)
            };

            var lblMaHD = new Label
            {
                Text = "Mã hóa đơn:",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Location = new Point(30, 75)
            };
            lblMaHDValue = new Label
            {
                Text = _maHD.ToString(),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(140, 75),
                ForeColor = Color.Green
            };

            // ===== DataGridView hiển thị sản phẩm =====
            dgvChiTiet = new DataGridView
            {
                Location = new Point(30, 110),
                Size = new Size(830, 380),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Font & kích thước ô cell
            dgvChiTiet.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 14f, FontStyle.Regular),
                Padding = new Padding(6),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            // ===== HEADER: Tăng height để không bị cắt chữ =====
            dgvChiTiet.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(0, 8, 0, 8)   // thêm padding dọc
            };
            dgvChiTiet.EnableHeadersVisualStyles = false;
            dgvChiTiet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvChiTiet.ColumnHeadersHeight = 48;   // header đủ cao cho font 14

            // Chiều cao từng dòng
            dgvChiTiet.RowTemplate.Height = 40;
            dgvChiTiet.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            var lblTongTien = new Label
            {
                Text = "Tổng tạm tính:",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 510)
            };
            lblTongTienValue = new Label
            {
                Text = "0 đ",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Maroon,
                Location = new Point(160, 510)
            };

            btnMuaThem = new Button
            {
                Text = "Mua thêm",
                Width = 150,
                Height = 40,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                Location = new Point(520, 505)
            };
            btnMuaThem.Click += (s, e) => this.Close();

            btnThanhToan = new Button
            {
                Text = "Thanh toán",
                Width = 150,
                Height = 40,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(700, 505),
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

        // ================== DATA ==================

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
