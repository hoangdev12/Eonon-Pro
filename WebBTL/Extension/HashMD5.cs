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
            using (var md5 = MD5.Create())
            {
                // Băm chuỗi thành mảng byte
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

                var sb = new StringBuilder(hashBytes.Length * 2);
                // Chuyển đổi từng byte thành chuỗi hex
                foreach (var b in hashBytes)
                    sb.AppendFormat("{0:x2}", b); // Định dạng từng byte thành hex

                return sb.ToString(); // Trả về chuỗi băm MD5
            }
        }

    }
}
