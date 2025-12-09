using System;
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

        public TaoHoaDonForm()
        {
            this.Text = "Tạo hóa đơn mới";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            TaoGiaoDien();
            NapThongTinMacDinh();
        }

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

        private void BtnTaoHoaDon_Click(object sender, EventArgs e)
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
