using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    /// <summary>
    /// Form thêm chi tiết hoá đơn:
    /// - Nhận sẵn MaHD từ form tạo hoá đơn.
    /// - Nhập MASP, Số lượng.
    /// - Nút "Thêm sản phẩm": gọi sp_ThemChiTietHoaDon.
    /// - Nút "Thanh toán...": (nếu đang nhập dở sẽ hỏi có thêm luôn không) rồi mở form ThanhToanHoaDonForm.
    /// </summary>
    public class ThemChiTietHoaDonForm : Form
    {
        private readonly int _maHD;

        // Controls
        private Label lblMaHDValue;
        private TextBox txtMaSP;
        private TextBox txtSoLuong;
        private Button btnThemSanPham;
        private Button btnThanhToan;
        private Button btnDong;

        // ==== Biến phục vụ scale layout ====
        private Size _originalClientSize;
        private readonly Dictionary<Control, Rectangle> _originalBounds = new();
        private readonly Dictionary<Control, float> _originalFontSizes = new();
        private bool _layoutSaved = false;

        public ThemChiTietHoaDonForm(int maHD)
        {
            _maHD = maHD;

            this.Text = "Thêm chi tiết hóa đơn";
            this.Size = new Size(420, 320);     // kích thước thiết kế ban đầu
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = true;

            TaoGiaoDien();

            // Lưu layout ban đầu + phóng to khi load xong
            this.Load += ThemChiTietHoaDonForm_Load;

            // Mỗi lần resize thì scale control
            this.Resize += ThemChiTietHoaDonForm_Resize;
        }

        // ================== SCALE LAYOUT ==================

        private void ThemChiTietHoaDonForm_Load(object? sender, EventArgs e)
        {
            SaveInitialLayout();

            // Sau khi lưu layout ban đầu -> phóng to form
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

        private void ThemChiTietHoaDonForm_Resize(object? sender, EventArgs e)
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

        // ================== GIAO DIỆN GỐC ==================

        private void TaoGiaoDien()
        {
            var lblTitle = new Label
            {
                Text = "CHI TIẾT HÓA ĐƠN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = true,
                Location = new Point(95, 20)
            };

            // Mã hoá đơn (hiển thị, không cho sửa)
            var lblMaHD = new Label
            {
                Text = "Mã hóa đơn:",
                AutoSize = true,
                Location = new Point(40, 80)
            };
            lblMaHDValue = new Label
            {
                Text = _maHD.ToString(),
                AutoSize = true,
                Location = new Point(160, 80),
                ForeColor = Color.DarkGreen
            };

            // Mã sản phẩm
            var lblMaSP = new Label
            {
                Text = "Mã sản phẩm:",
                AutoSize = true,
                Location = new Point(40, 115)
            };
            txtMaSP = new TextBox
            {
                Location = new Point(160, 110),
                Width = 200
            };

            // Số lượng
            var lblSoLuong = new Label
            {
                Text = "Số lượng:",
                AutoSize = true,
                Location = new Point(40, 145)
            };
            txtSoLuong = new TextBox
            {
                Location = new Point(160, 140),
                Width = 200
            };

            // Nút Thêm sản phẩm
            btnThemSanPham = new Button
            {
                Text = "Thêm sản phẩm",
                Width = 150,
                Height = 35,
                Location = new Point(40, 190),
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnThemSanPham.FlatAppearance.BorderSize = 0;
            btnThemSanPham.Click += BtnThemSanPham_Click;

            // Nút Mở form Thanh toán
            btnThanhToan = new Button
            {
                Text = "Thanh toán...",
                Width = 150,
                Height = 35,
                Location = new Point(210, 190),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnThanhToan.FlatAppearance.BorderSize = 0;
            btnThanhToan.Click += BtnThanhToan_Click;

            // Nút Đóng
            btnDong = new Button
            {
                Text = "Đóng",
                Width = 100,
                Height = 30,
                Location = new Point(150, 235)
            };
            btnDong.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblMaHD, lblMaHDValue,
                lblMaSP, txtMaSP,
                lblSoLuong, txtSoLuong,
                btnThemSanPham, btnThanhToan, btnDong
            });
        }

        // ===== Hàm chung: thêm 1 sản phẩm vào hóa đơn (gọi proc) =====
        private bool ThemSanPhamVaoHoaDon(int maSP, int soLuong)
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_ThemChiTietHoaDon", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaHD", _maHD);
                    cmd.Parameters.AddWithValue("@MaSP", maSP);
                    cmd.Parameters.AddWithValue("@SoLuong", soLuong);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Lỗi thêm chi tiết",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message,
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // ===== Nút "Thêm sản phẩm" =====
        private void BtnThemSanPham_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtMaSP.Text.Trim(), out int maSP))
            {
                MessageBox.Show("Mã sản phẩm phải là số nguyên.",
                                "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaSP.Focus();
                return;
            }

            if (!int.TryParse(txtSoLuong.Text.Trim(), out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên dương.",
                                "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoLuong.Focus();
                return;
            }

            if (ThemSanPhamVaoHoaDon(maSP, soLuong))
            {
                MessageBox.Show("Thêm sản phẩm vào hóa đơn thành công.",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear để nhập tiếp
                txtMaSP.Clear();
                txtSoLuong.Clear();
                txtMaSP.Focus();
            }
        }

        // ===== Nút "Thanh toán..." (tự chốt sản phẩm đang nhập nếu cần) =====
        private void BtnThanhToan_Click(object? sender, EventArgs e)
        {
            // Nếu ô nhập còn dữ liệu -> hỏi có muốn thêm luôn không
            bool coDuLieuDangNhap = !string.IsNullOrWhiteSpace(txtMaSP.Text) ||
                                    !string.IsNullOrWhiteSpace(txtSoLuong.Text);

            if (coDuLieuDangNhap)
            {
                var confirm = MessageBox.Show(
                    "Bạn đang nhập một sản phẩm nhưng chưa bấm \"Thêm sản phẩm\".\n" +
                    "Bạn có muốn thêm sản phẩm này vào hóa đơn trước khi sang bước thanh toán không?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    // Validate lại lần nữa
                    if (!int.TryParse(txtMaSP.Text.Trim(), out int maSP))
                    {
                        MessageBox.Show("Mã sản phẩm phải là số nguyên.",
                                        "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtMaSP.Focus();
                        return;
                    }

                    if (!int.TryParse(txtSoLuong.Text.Trim(), out int soLuong) || soLuong <= 0)
                    {
                        MessageBox.Show("Số lượng phải là số nguyên dương.",
                                        "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtSoLuong.Focus();
                        return;
                    }

                    // Gọi proc thêm chi tiết
                    if (!ThemSanPhamVaoHoaDon(maSP, soLuong))
                    {
                        // Có lỗi => không sang bước thanh toán
                        return;
                    }

                    // Thêm ok thì clear ô nhập
                    txtMaSP.Clear();
                    txtSoLuong.Clear();
                }
            }

            // Mở form thanh toán (xem danh sách + quyết định thanh toán / quay lại)
            var frm = new ThanhToanHoaDonForm(_maHD);
            var result = frm.ShowDialog();

            // Nếu thanh toán thành công => đóng luôn form chi tiết
            if (result == DialogResult.OK)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
