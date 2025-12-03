using System;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Winform
{
    public class ThongTinCaNhan : Form
    {
  //    String connectionString =@"Data Source=DESKTOP-EIS4KER\SQLEXPRESS;Initial Catalog=PetcareX;Integrated Security=True;TrustServerCertificate=True";
        private DataGridView dgvThongTinKhachHang ;
        private Button nutTimKiem; 
        private TextBox truongNhapMa; 

        public ThongTinCaNhan(string maKH = "") 
        {
            TaoGiaoDien(); 
            if (!string.IsNullOrEmpty(maKH))
            {
                truongNhapMa.Text = maKH; 
                TaiThongTinCaNhan();      
            }
        }
private void TaoGiaoDien()
        {
            this.Size = new Size(600, 400);
            this.Text = "Thông tin cá nhân";

            truongNhapMa = new TextBox();
            truongNhapMa.Location = new Point(20, 20);
            truongNhapMa.Size = new Size(150, 25);
            this.Controls.Add(truongNhapMa);

            nutTimKiem = new Button();
            nutTimKiem.Text = "Tìm/Tải lại";
            nutTimKiem.Location = new Point(180, 18); 
            nutTimKiem.Size = new Size(100, 30);
            nutTimKiem.Click += (s, e) => TaiThongTinCaNhan();
            this.Controls.Add(nutTimKiem);

            dgvThongTinKhachHang = new DataGridView();
            dgvThongTinKhachHang.Location = new Point(20, 60); 
            dgvThongTinKhachHang.Size = new Size(540, 280);
            dgvThongTinKhachHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; 
            this.Controls.Add(dgvThongTinKhachHang);
        }
        private void TaiThongTinCaNhan()
        {
            // kết nối tới sql server
            using (SqlConnection conn = Connection.GetConnection())
            {
                String maCanTim = truongNhapMa.Text.Trim();
                try
                {   
                    // mở kết nối
                    conn.Open();
                    // Tạo một command và cho type = procedure
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_KhachHang_XemChiTiet";
                    cmd.Parameters.AddWithValue("@MaKH", maCanTim);

                    // dùng để lấy thông tin và đưa vào dgvThongTinKhachHang 
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    // bảng chứa data
                    DataTable dt =  new DataTable();
                    // đưa data vào datatable 
                    adapter.Fill (dt);
                    dgvThongTinKhachHang.DataSource = dt; 

                }
                catch (Exception ex)
                {
                    MessageBox.Show ("Lỗi: " + ex.Message);
                }
            }
        }
    }
    
}