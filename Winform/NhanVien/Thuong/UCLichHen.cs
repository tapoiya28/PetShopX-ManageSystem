using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Winform
{
    public class UCLichHen : UserControl
    {
        private DataGridView dgvLichHen;
        private TextBox txtTimKiem;
        private Button btnTim;
        private Button btnXacNhan;
        private Button btnThemTrucTiep; 

        public struct KetQuaChon { public int MaTC; public int MaBS; }

        public UCLichHen()
        {
            this.BackColor = Color.WhiteSmoke;
            TaoGiaoDien();
            TaiDanhSachLichHen();
        }

        private void TaoGiaoDien()
        {
            Label lblTitle = new Label() { 
                Text = "QUẢN LÝ LỊCH HẸN", 
                Font = new Font("Segoe UI", 18, FontStyle.Bold), 
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(20, 15), 
                AutoSize = true 
            };

            Label lblSdt = new Label() { Text = "SĐT:", Location = new Point(25, 75), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtTimKiem = new TextBox() { Location = new Point(140, 72), Width = 250, Font = new Font("Segoe UI", 10) };
            
            btnTim = new Button() { 
                Text = "🔍 Tìm kiếm", 
                Location = new Point(400, 70), 
                Width = 100, Height = 30,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnTim.Click += (s, e) => TaiDanhSachLichHen();

            // Khởi tạo nút Đặt lịch trực tiếp
            btnThemTrucTiep = new Button() { 
                Text = "➕ ĐẶT LỊCH TRỰC TIẾP", 
                Location = new Point(410, 20), // Chỉnh lại vị trí cho đẹp
                Width = 200, Height = 35,
                BackColor = Color.Orange, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThemTrucTiep.Click += (s, e) => {
                using (var frm = new FrmThemLichTrucTiep()) {
                    if (frm.ShowDialog() == DialogResult.OK) {
                        TaiDanhSachLichHen();
                    }
                }
            };

            btnXacNhan = new Button() { 
                Text = "✅ XÁC NHẬN & TẠO HỒ SƠ", 
                Location = new Point(650, 70), 
                Width = 240, Height = 30,
                BackColor = Color.ForestGreen, ForeColor = Color.White, 
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnXacNhan.Click += (s, e) => XuLyXacNhan();

            dgvLichHen = new DataGridView();
            dgvLichHen.Location = new Point(20, 120);
            dgvLichHen.Size = new Size(900, 500);
            dgvLichHen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvLichHen.BackgroundColor = Color.White;
            dgvLichHen.ReadOnly = true;
            dgvLichHen.AllowUserToAddRows = false;
            dgvLichHen.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            this.Controls.Add(btnThemTrucTiep);
            this.Controls.AddRange(new Control[] { lblTitle, lblSdt, txtTimKiem, btnTim, btnXacNhan, dgvLichHen });
        }

        private void TaiDanhSachLichHen()
        {
            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_NhanVien_XemLichHen", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 60; // Tăng timeout
                        cmd.Parameters.AddWithValue("@MaCN", UserSession.WorkBranchId);
                        
                        string sdt = txtTimKiem.Text.Trim();
                        cmd.Parameters.AddWithValue("@SdtKhachHang", string.IsNullOrEmpty(sdt) ? (object)DBNull.Value : sdt);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvLichHen.DataSource = dt;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải lịch hẹn: " + ex.Message); }
        }

        private void XuLyXacNhan()
        {
            if (dgvLichHen.SelectedRows.Count == 0) return;
            var row = dgvLichHen.SelectedRows[0];
            
            int maLichHen = Convert.ToInt32(row.Cells["MALICHHEN"].Value);
            int maKH = Convert.ToInt32(row.Cells["MAKH"].Value);
            int maCN = UserSession.WorkBranchId; 

            var result = HienFormChonPetVaBacSi(maKH, maCN);
            if (result.MaTC == -1 || result.MaBS == -1) return;

            try
            {
                using (SqlConnection conn = Connection.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_NhanVien_XacNhanLichHen", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MaLichHen", maLichHen);
                        cmd.Parameters.AddWithValue("@MaNV", result.MaBS); 
                        cmd.Parameters.AddWithValue("@MaTC", result.MaTC); 

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Xác nhận thành công!", "Thành công");
                        TaiDanhSachLichHen();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi xác nhận: " + ex.Message); }
        }

        private KetQuaChon HienFormChonPetVaBacSi(int maKH, int maCN)
        {
            KetQuaChon kq = new KetQuaChon { MaTC = -1, MaBS = -1 };
            Form frm = new Form() { Size = new Size(400, 300), Text = "Chọn Pet & Bác sĩ", StartPosition = FormStartPosition.CenterParent };
            
            ComboBox cboPet = new ComboBox() { Location = new Point(30, 45), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            ComboBox cboBS = new ComboBox() { Location = new Point(30, 115), Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
            Button btnOk = new Button() { Text = "Xác Nhận", Location = new Point(140, 180), DialogResult = DialogResult.OK };

            frm.Controls.AddRange(new Control[] { new Label(){Text="Chọn Pet:", Location=new Point(30,20)}, cboPet, new Label(){Text="Chọn Bác sĩ:", Location=new Point(30,90)}, cboBS, btnOk });

            try {
                using (SqlConnection conn = Connection.GetConnection()) {
                    conn.Open();
                    SqlDataAdapter daPet = new SqlDataAdapter("sp_ThuCung_XemDanhSach", conn);
                    daPet.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daPet.SelectCommand.Parameters.AddWithValue("@MAKH", maKH);
                    DataTable dtPet = new DataTable(); daPet.Fill(dtPet);
                    cboPet.DataSource = dtPet; cboPet.DisplayMember = "TENTC"; cboPet.ValueMember = "MATC";

                    SqlDataAdapter daBS = new SqlDataAdapter("sp_ChiNhanh_LayDanhSachBacSi", conn);
                    daBS.SelectCommand.CommandType = CommandType.StoredProcedure;
                    daBS.SelectCommand.Parameters.AddWithValue("@MaCN", maCN);
                    DataTable dtBS = new DataTable(); daBS.Fill(dtBS);
                    cboBS.DataSource = dtBS; cboBS.DisplayMember = "HOTEN"; cboBS.ValueMember = "MANV";
                }
            } catch { }

            if (frm.ShowDialog() == DialogResult.OK) {
                if (cboPet.SelectedValue != null && cboBS.SelectedValue != null) {
                    kq.MaTC = Convert.ToInt32(cboPet.SelectedValue);
                    kq.MaBS = Convert.ToInt32(cboBS.SelectedValue);
                }
            }
            return kq;
        }
    }
}