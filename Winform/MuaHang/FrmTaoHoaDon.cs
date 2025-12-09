using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Winform
{
    public class TaoHoaDonForm : Form
    {
        private TextBox txtMaKH;
        private Label lblMaNVValue;
        private Label lblMaCNValue;
        private Label lblNgayLapValue;
        private Button btnTaoHoaDon;
        private Button btnDong;

        private int _maNV;
        private int _maCN;
        private DateTime _ngayLap;

        // ==== Biến phục vụ scale layout ====
        private Size _originalClientSize;
        private readonly Dictionary<Control, Rectangle> _originalBounds = new();
        private readonly Dictionary<Control, float> _originalFontSizes = new();
        private bool _layoutSaved = false;

        public TaoHoaDonForm()
        {
            this.Text = "Tạo hóa đơn mới";

            // Kích thước thiết kế ban đầu (layout gốc) – cứ để nhỏ như bạn đang design
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = true; // cho phép phóng to

            TaoGiaoDien();
            NapThongTinMacDinh();

            // Khi load xong form thì lưu layout gốc và phóng to
            this.Load += TaoHoaDonForm_Load;

            // Mỗi lần resize thì scale control theo
            this.Resize += TaoHoaDonForm_Resize;
        }

        // ================== SCALE LAYOUT ==================

        private void TaoHoaDonForm_Load(object? sender, EventArgs e)
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

        private void TaoHoaDonForm_Resize(object? sender, EventArgs e)
        {
            if (!_layoutSaved || _originalClientSize.Width == 0 || _originalClientSize.Height == 0)
                return;

            float scaleX = (float)this.ClientSize.Width / _originalClientSize.Width;
            float scaleY = (float)this.ClientSize.Height / _originalClientSize.Height;
            float scale = Math.Min(scaleX, scaleY); // dùng min để font không bị méo

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

                if (_originalFontSizes.TryGetValue(c, out float fontSize))
                {
                    if (fontSize > 0)
                    {
                        c.Font = new Font(c.Font.FontFamily, fontSize * scale, c.Font.Style);
                    }
                }
            }
        }

        // ================== GIAO DIỆN GỐC ==================

        private void TaoGiaoDien()
        {
            var lblTitle = new Label
            {
                Text = "TẠO HÓA ĐƠN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                AutoSize = true,
                Location = new Point(120, 20)
            };

            var lblMaKH = new Label
            {
                Text = "Mã khách hàng:",
                AutoSize = true,
                Location = new Point(40, 80)
            };
            txtMaKH = new TextBox
            {
                Location = new Point(160, 75),
                Width = 200
            };

            var lblMaNV = new Label
            {
                Text = "Mã nhân viên:",
                AutoSize = true,
                Location = new Point(40, 115)
            };
            lblMaNVValue = new Label
            {
                AutoSize = true,
                Location = new Point(160, 115),
                ForeColor = Color.DarkGreen
            };

            var lblMaCN = new Label
            {
                Text = "Mã chi nhánh:",
                AutoSize = true,
                Location = new Point(40, 145)
            };
            lblMaCNValue = new Label
            {
                AutoSize = true,
                Location = new Point(160, 145),
                ForeColor = Color.DarkGreen
            };

            var lblNgayLap = new Label
            {
                Text = "Ngày lập:",
                AutoSize = true,
                Location = new Point(40, 175)
            };
            lblNgayLapValue = new Label
            {
                AutoSize = true,
                Location = new Point(160, 175),
                ForeColor = Color.DarkGreen
            };

            btnTaoHoaDon = new Button
            {
                Text = "TẠO HÓA ĐƠN",
                Width = 150,
                Height = 35,
                Location = new Point(40, 220),
                BackColor = Color.RoyalBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTaoHoaDon.FlatAppearance.BorderSize = 0;
            btnTaoHoaDon.Click += BtnTaoHoaDon_Click;

            btnDong = new Button
            {
                Text = "ĐÓNG",
                Width = 100,
                Height = 35,
                Location = new Point(260, 220)
            };
            btnDong.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblMaKH, txtMaKH,
                lblMaNV, lblMaNVValue,
                lblMaCN, lblMaCNValue,
                lblNgayLap, lblNgayLapValue,
                btnTaoHoaDon, btnDong
            });
        }

        // ================== LOGIC CŨ (giữ nguyên) ==================

        private void NapThongTinMacDinh()
        {
            _maNV = UserSession.UserId;
            if (_maNV <= 0)
            {
                MessageBox.Show("Không xác định được mã nhân viên (UserSession.UserId).",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnTaoHoaDon.Enabled = false;
                return;
            }
            lblMaNVValue.Text = _maNV.ToString();

            _ngayLap = DateTime.Now;
            lblNgayLapValue.Text = _ngayLap.ToString("dd/MM/yyyy HH:mm");

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 1 MACN
                    FROM LAMVIEC
                    WHERE MANV = @MaNV
                      AND NGAYBATDAU <= CONVERT(date, @NgayLap)
                      AND (NGAYKETTHUC IS NULL OR NGAYKETTHUC >= CONVERT(date, @NgayLap))",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@MaNV", _maNV);
                    cmd.Parameters.AddWithValue("@NgayLap", _ngayLap);

                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        MessageBox.Show(
                            "Nhân viên không có ca làm việc ở chi nhánh nào trong ngày hôm nay.",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnTaoHoaDon.Enabled = false;
                        return;
                    }

                    _maCN = Convert.ToInt32(result);
                    lblMaCNValue.Text = _maCN.ToString();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Lỗi SQL khi tải chi nhánh: " + ex.Message,
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnTaoHoaDon.Enabled = false;
            }
        }

        private void BtnTaoHoaDon_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtMaKH.Text.Trim(), out int maKH))
            {
                MessageBox.Show("Mã khách hàng phải là số nguyên.",
                                "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaKH.Focus();
                return;
            }

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_TaoHoaDonMoi", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NgayLap", _ngayLap);
                    cmd.Parameters.AddWithValue("@MaKH", maKH);
                    cmd.Parameters.AddWithValue("@MaCN", _maCN);
                    cmd.Parameters.AddWithValue("@MaNV", _maNV);

                    var pMaHD = new SqlParameter("@MaHD", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pMaHD);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int maHD = (int)pMaHD.Value;

                    MessageBox.Show(
                        $"Tạo hóa đơn thành công.\nMã hóa đơn: {maHD}",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    // Sau khi tạo xong -> nhảy sang form thêm chi tiết
                    var chiTietForm = new ThemChiTietHoaDonForm(maHD);
                    chiTietForm.ShowDialog();

                    // Sau khi xong chi tiết, đóng form tạo hoá đơn
                    this.Close();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Lỗi tạo hóa đơn",
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
