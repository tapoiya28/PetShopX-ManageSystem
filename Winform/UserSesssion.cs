namespace Winform
{
    public static class UserSession
    {
        public static string UserName { get; set; } = "";
        public static string FullName { get; set; } = "";
        public static string Role { get; set; } = "";
        public static int UserId { get; set; } = 0;

        public static int WorkBranchId { get; set; } = 0;     

        public static bool IsLoggedIn => !string.IsNullOrEmpty(UserName);
        
        public static bool IsQuanLy() => Role == "QuanLy";
        public static bool IsBacSi() => Role == "BacSi";
        public static bool IsNhanVien() => Role == "NhanVien";
        public static bool IsKhachHang() => Role == "KhachHang";

        public static void Clear()
        {
            UserName = ""; FullName = ""; Role = ""; UserId = 0;
            WorkBranchId = 0; 
        }
    }
}