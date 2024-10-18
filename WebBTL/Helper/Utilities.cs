using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Security.Claims;
using System.Security.Principal;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebBTL.Helper
{
   public static class Utilities
    {
        public static int PAGE_SIZE = 20;
        public static void CreateIfMissing(string path)
        {
            // Kiểm tra xem thư mục đã tồn tại chưa
            if (!Directory.Exists(path))
            {
                // Nếu chưa tồn tại, tạo thư mục
                Directory.CreateDirectory(path);
            }
        }

        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        public static string ToTitleCase(string str)
        {
            // Kiểm tra nếu chuỗi rỗng hoặc null
            if (string.IsNullOrEmpty(str))
            {
                return str; // Trả về chuỗi ban đầu nếu nó rỗng hoặc null
            }

            // Lấy thông tin văn bản cho ngôn ngữ hiện tại
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

            // Chuyển đổi chuỗi thành tiêu đề
            return textInfo.ToTitleCase(str.ToLower());
        }
        public static bool IsInteger(string str)
        {
            // Kiểm tra nếu chuỗi null hoặc rỗng
            if (string.IsNullOrEmpty(str))
            {
                return false; // Không phải là số nguyên
            }

            // Sử dụng int.TryParse để kiểm tra xem chuỗi có phải là số nguyên không
            return int.TryParse(str, out _);
        }
        public static string GetRandomKey(int length = 5)
        {
           string parttern = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random rd = new Random();
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < length; i++)
            {
                sb.Append(parttern[rd.Next(parttern.Length)]);
            }

            return sb.ToString();
        }
        public static string SEOurl(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty; // Trả về chuỗi rỗng nếu đầu vào null hoặc rỗng
            }

            // Chuyển đổi thành chữ thường
            str = str.ToLower();

            // Thay thế ký tự đặc biệt bằng khoảng trắng
            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // Giữ lại chữ cái, số và khoảng trắng
            str = Regex.Replace(str, @"\s+", " "); // Thay thế nhiều khoảng trắng bằng một khoảng trắng
            str = str.Trim(); // Xóa khoảng trắng ở đầu và cuối

            // Thay thế khoảng trắng bằng dấu gạch ngang
            str = str.Replace(" ", "-");

            // Trả về kết quả
            return str;
        }

        public static MvcHtmlString Image(this HtmlHelper html, string imagePath, string className, object htmlAttributes = null)
        {
            var builder = new TagBuilder("img");
            builder.MergeAttribute("class", className);
            builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            // Kiểm tra nếu imagePath là null hoặc rỗng
            if (!string.IsNullOrEmpty(imagePath))
            {
                builder.MergeAttribute("src", html.Encode(imagePath));
            }
            else
            {
                // Thêm giá trị mặc định hoặc không làm gì
                builder.MergeAttribute("src", "~/Content/images/default-image.jpg"); 
            }

            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }


    }

}