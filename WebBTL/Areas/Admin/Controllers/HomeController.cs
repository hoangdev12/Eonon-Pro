using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBTL.Models;

namespace WebBTL.Areas.Admin.Controllers
{
    
    public class HomeController : Controller
    {

        private readonly Eonon_ProEntities1 _context;

        public HomeController()
        {
            _context = new Eonon_ProEntities1();
        }
        // GET: Admin/Home
        
            [HttpGet]
            public ActionResult Index()
            {
                // Check if the user is logged in
                if (Session["AccountId"] == null)
                {
                    return RedirectToAction("Login", "Home");
                }

                // Check if the user is an admin
                if (Session["Role"] == null || Session["Role"].ToString() != "admin")
                {
                    return RedirectToAction("AccessDenied", "Home"); // Redirect to a specific Access Denied page
                }

                int accountId = (int)Session["AccountId"]; // Get AccountId from session
                var user = _context.Customers.FirstOrDefault(u => u.AccountID == accountId);

                if (user != null)
                {
                    // Check if Avatar and FullName exist, if not, assign default values
                    ViewBag.Avatar = string.IsNullOrEmpty(user.Avatar) ? "~/Content/images/avatar/defaultAvatar.jpg" : user.Avatar;
                    ViewBag.FullName = string.IsNullOrEmpty(user.FullName) ? "No Name Available" : user.FullName;
                }
                else
                {
                    // If user not found, assign default values for Avatar and FullName
                    ViewBag.Avatar = "~/Content/images/avatar/defaultAvatar.jpg";
                    ViewBag.FullName = "No Name Available";
                }

                return View();
            }


        [HttpGet]
        public ActionResult AccessDenied() { 
            TempData["Error"] = "Bạn không có quyền truy cập vào trang quản trị."; 
            return View();
        }


        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel customer)
        {
            if (ModelState.IsValid)
            {
                // Tìm người dùng theo email
                var user = _context.Accounts.FirstOrDefault(s => s.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    var enteredPassword = customer.Password;

                    // Kiểm tra mật khẩu
                    if (enteredPassword.Equals(user.Password))
                    {
                        // Lưu thông tin người dùng vào session
                        Session["Email"] = user.Email;
                        Session["AccountId"] = user.AccountID;
                        Session["Role"] = user.Role.RoleName;

                        // Khôi phục giỏ hàng từ cookie nếu có
                        var cartCookie = Request.Cookies["cart"]?.Value;
                        if (!string.IsNullOrEmpty(cartCookie))
                        {
                            var cart = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
                            Session["Cart"] = cart; // Đặt lại giỏ hàng vào session
                                                    // Xóa Cookie sau khi khôi phục
                            var cookie = new HttpCookie("cart")
                            {
                                Expires = DateTime.Now.AddDays(-1) // Xóa cookie
                            };
                            Response.Cookies.Add(cookie);
                        }

                        // Kiểm tra nếu có URL nào đã được lưu trong session trước khi đăng nhập
                        string returnUrl = Session["ReturnUrl"] as string;
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            // Xóa ReturnUrl khỏi Session sau khi sử dụng
                            Session.Remove("ReturnUrl");
                            return Redirect(returnUrl); // Chuyển hướng về URL được lưu
                        }

                        // Kiểm tra vai trò người dùng và chuyển hướng đến trang tương ứng
                        if (user.Role.RoleName == "admin")
                        {
                            // Nếu là Admin, chuyển hướng tới trang Admin
                            return RedirectToAction("Index", "Home");
                        }
                        else if (user.Role.RoleName == "user" || Session["AccountId"] == null)
                        {

                            return RedirectToAction("Index", "Home", new { area = "" });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid Email or Password");
                        return View(customer);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Can't find your account. Please register");
                    return View(customer);
                }
            }

            // Nếu không hợp lệ, hiển thị lại form
            return View(customer); // Trả về view với trạng thái model hiện tại để hiển thị lỗi xác thực
        }
    }
}