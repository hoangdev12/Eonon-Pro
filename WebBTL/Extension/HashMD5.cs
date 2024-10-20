using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Text;
using System.Threading.Tasks;

namespace WebBTL.Extension
{
    public static class HashMD5
    {
        public static string ToMD5(this string str)
        {
            using (MD5 md5 = MD5.Create())
            {
                // Chuyển đổi chuỗi đầu vào thành mảng byte
                byte[] inputBytes = Encoding.UTF8.GetBytes(str);
                // Tính toán giá trị băm
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Chuyển đổi mảng byte thành chuỗi hex
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); 
                }
                return sb.ToString();
            }
        }

    }
}
