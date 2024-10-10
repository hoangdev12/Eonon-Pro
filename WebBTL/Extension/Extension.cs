using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace WebBTL.Extension
{
    public static class Extension
    {
        // Chuyển đổi giá trị double thành chuỗi tiền tệ VND
        public static string ToVnd(this double donGia)
        {
            return donGia.ToString("#,##0") + " đ";
        }

        // Chuyển đổi chuỗi thành dạng viết hoa chữ cái đầu tiên của mỗi từ
        public static string ToTitleCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str; // Trả về null hoặc chuỗi rỗng nếu đầu vào không hợp lệ

            // Tách chuỗi thành mảng từ
            var words = str.Split(' ');
            for (int index = 0; index < words.Length; index++)
            {
                var s = words[index];
                if (s.Length > 0)
                {
                    // Chuyển chữ cái đầu tiên thành chữ hoa và kết hợp với phần còn lại của từ
                    words[index] = char.ToUpper(s[0]) + s.Substring(1).ToLower();
                }
            }

            // Kết hợp các từ lại thành chuỗi
            return string.Join(" ", words);
        }

    }
    

     
}