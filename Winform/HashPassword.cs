using System;
using System.Text;
using System.Security.Cryptography;

namespace Winform
{
    public static class SecurityHelper
    {
        public static string HashPassword(string password, string salt)
        {
            if (string.IsNullOrEmpty(password)) return "";

            using (MD5 md5 = MD5.Create())
            {
                string rawInput = password + salt; 
                byte[] inputBytes = Encoding.UTF8.GetBytes(rawInput);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2")); 
                }
                return sb.ToString();
            }
        }
    }
}