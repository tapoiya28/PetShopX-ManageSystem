using System;
using System.Configuration; 
using System.Data;
using System.Data.SqlClient;

namespace Winform
{
    public static class Connection
    {
        private static string connectionString = @"Data Source=DESKTOP-E7PEM57;Initial Catalog=PetcareX;Integrated Security=True;TrustServerCertificate=True";
        public static SqlConnection GetConnection()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Chuỗi kết nối chưa được cấu hình!");
            }
            return new SqlConnection(connectionString);
        }
    }
}
