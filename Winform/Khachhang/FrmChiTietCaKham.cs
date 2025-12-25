using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Winform
{
    public class FrmChiTietCaKham : Form
    {
        private int _maKB;
        
        // Controls
        private Label lblTieuDe;
        private Label lblThongTinChung; // Tên thú cưng, Chủ, Ngày khám
        private Label lblLamSang;       // Triệu chứng, Chẩn đoán
        private Label lblLoiDan;        // Lời dặn bác sĩ
        private DataGridView dgvToaThuoc;
        private Button btnClose;

        public FrmChiTietCaKham(int maKB)
        {
            _maKB = maKB;
            
            // Setup Form
            this.Text = $"Chi Tiết Ca Khám #{maKB}";
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            TaoGiaoDien();
            LoadDuLieu();
        }

        private void TaoGiaoDien()
        {
            // 1. Tiêu đề
            lblTieuDe = new Label() {
                Text = "CHI TIẾT CA KHÁM & TOA THUỐC",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(20, 15),
                AutoSize = true
            };

            // 2. Thông tin chung (Bên Trái)
            lblThongTinChung = new Label() {
                Location = new Point(25, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Black,
                MaximumSize = new Size(350, 0)
            };

            // 3. Thông tin Lâm Sàng (Bên Phải - Triệu chứng/Chẩn đoán)
            lblLamSang = new Label() {
                Location = new Point(400, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.DarkRed,
                MaximumSize = new Size(400, 0)
            };

            // 4. Lời dặn (Nằm dưới thông tin lâm sàng)
            lblLoiDan = new Label() {
                Location = new Point(25, 150), // Vị trí tạm, sẽ chỉnh lại sau
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                ForeColor = Color.DarkBlue,
                MaximumSize = new Size(780, 0)
            };

            // 5. Grid Toa Thuốc
            dgvToaThuoc = new DataGridView() {
                Location = new Point(20, 200), // Vị trí tạm
                Size = new Size(790, 300),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Style Grid
            dgvToaThuoc.EnableHeadersVisualStyles = false;
            dgvToaThuoc.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 204);
            dgvToaThuoc.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvToaThuoc.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvToaThuoc.ColumnHeadersHeight = 35;
            dgvToaThuoc.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            // 6. Nút Đóng
            btnClose = new Button() {
                Text = "Đóng",
                Width = 100,
                Height = 35,
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(710, 510)
            };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTieuDe, lblThongTinChung, lblLamSang, lblLoiDan, dgvToaThuoc, btnClose });
        }

        private void LoadDuLieu()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();

                    // --- 1. Lấy thông tin chung (sp_CaKham_ChiTiet) ---
                    using (SqlCommand cmd = new SqlCommand("sp_CaKham_ChiTiet", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MAKB", _maKB);
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                string ngayKham = Convert.ToDateTime(r["NGAYKHAM"]).ToString("dd/MM/yyyy HH:mm");
                                lblThongTinChung.Text = $"Mã Ca: #{r["MAKB"]}\n" +
                                                        $"Ngày khám: {ngayKham}\n" +
                                                        $"Thú cưng: {r["TENTC"]} ({r["LOAI"]})\n" +
                                                        $"Chủ nuôi: {r["TENKH"]}";
                            }
                        }
                    }

                    // --- 2. Lấy Triệu chứng & Chẩn đoán (sp_TrieuChung_DanhSach & sp_ChanDoan_DanhSach) ---
                    List<string> lstTrieuChung = GetListString(conn, "sp_TrieuChung_DanhSach", "Triệu Chứng");
                    List<string> lstChanDoan = GetListString(conn, "sp_ChanDoan_DanhSach", "Chẩn Đoán");

                    string strTrieuChung = lstTrieuChung.Count > 0 ? string.Join(", ", lstTrieuChung) : "Không ghi nhận";
                    string strChanDoan = lstChanDoan.Count > 0 ? string.Join(", ", lstChanDoan) : "Chưa kết luận";

                    lblLamSang.Text = $"TRIỆU CHỨNG: {strTrieuChung}\n\n" +
                                      $"CHẨN ĐOÁN: {strChanDoan.ToUpper()}";

                    // --- 3. Lấy Toa thuốc & Lời dặn (sp_ToaThuoc_ChiTiet) ---
                    using (SqlCommand cmd = new SqlCommand("sp_ToaThuoc_ChiTiet", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MAKB", _maKB);
                        
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);

                            // Table 0: Danh sách thuốc
                            if (ds.Tables.Count > 0)
                            {
                                dgvToaThuoc.DataSource = ds.Tables[0];
                                
                                // Format cột
                                if(dgvToaThuoc.Columns["MATT"] != null) dgvToaThuoc.Columns["MATT"].Visible = false;
                            }

                            // Table 1: Ghi chú (Lời dặn) - Nếu SP trả về 2 bảng
                            if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                            {
                                string ghiChu = ds.Tables[1].Rows[0]["GHICHU"].ToString();
                                lblLoiDan.Text = string.IsNullOrEmpty(ghiChu) ? "Lời dặn: (Không có)" : $"LỜI DẶN BÁC SĨ: {ghiChu}";
                            }
                            else
                            {
                                lblLoiDan.Text = "Lời dặn: (Không có)";
                            }
                        }
                    }
                    // --- 4. Cập nhật Layout sau khi có dữ liệu ---
                    UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // Hàm phụ trợ lấy danh sách string từ SP
        private List<string> GetListString(SqlConnection conn, string spName, string colName)
        {
            List<string> result = new List<string>();
            using (SqlCommand cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MAKB", _maKB);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        result.Add(r[colName].ToString());
                    }
                }
            }
            return result;
        }

        private void UpdateLayout()
        {
            // Tính toán vị trí Y dựa trên chiều cao của các label phía trên
            int bottomChung = lblThongTinChung.Bottom;
            int bottomLamSang = lblLamSang.Bottom;
            int maxTop = Math.Max(bottomChung, bottomLamSang) + 20;

            // Đặt vị trí Lời dặn
            lblLoiDan.Location = new Point(25, maxTop);
            
            // Đặt vị trí Grid
            int gridY = lblLoiDan.Bottom + 15;
            dgvToaThuoc.Location = new Point(20, gridY);

            // Tính chiều cao Grid
            int availableHeight = btnClose.Top - gridY - 15;
            if (availableHeight < 150) availableHeight = 150; // Chiều cao tối thiểu
            dgvToaThuoc.Height = availableHeight;
        }
    }
}