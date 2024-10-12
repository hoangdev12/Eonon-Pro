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
        public static async Task<string> UploadFile(Microsoft.AspNetCore.Http.IFormFile file, string sDirectory, string newname)
        {
            try
            {
                if (newname == null) newname = file.FileName;
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", sDirectory);
                CreateIfMissing(path);
                string pathFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", sDirectory, newname);
                var supportedTypes = new[] { "jpg", "jpeg", "png", "gif" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                if(!supportedTypes.Contains(fileExt.ToLower()))
                {
                    return null;
                }
                else
                {
                    using(var stream = new FileStream(pathFile, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return newname;
                }
            } catch (Exception ex) {
                return null;
            }
        }

        internal static Task<string> UploadFile(HttpPostedFileBase fThumb, string v1, string v2)
        {
            throw new NotImplementedException();
        }
    }
    
}