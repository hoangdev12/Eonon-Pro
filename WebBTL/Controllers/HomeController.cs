using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using WebBTL.Models;
using WebBTL.Extension;
using WebBTL.Helper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace WebBTL.Controllers
{
    public class HomeController : Controller
    {
        private readonly Eonon_ProEntities1 _context;

        public HomeController()
        {
            _context = new Eonon_ProEntities1();
        }

       
        public ActionResult Index()
        {
            var SPNoiBat = _context.Products.OrderBy(p => p.ProductID).Take(6).ToList();
            ViewBag.SPNoiBat = SPNoiBat;
            return View(SPNoiBat);
            
        }
     
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel account)
        {
            if (ModelState.IsValid)
            {

                var existingCustomer = _context.Customers.FirstOrDefault(c => c.Email == account.Email.Trim().ToLower());
                if (existingCustomer != null)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(account);
                }

                string salt = Utilities.GetRandomKey();

                Account newAccount = new Account
                {
                    Email = account.Email.Trim().ToLower(),
                    Password = account.Password ,
                    Salt = salt.Trim(), 
                    Active = true,
                    CreateDate = DateTime.Now
                };

                _context.Accounts.Add(newAccount);
                _context.SaveChanges();


                Customer khachhang = new Customer
                {
                    Email = newAccount.Email,
                    Password = newAccount.Password,
                    Active = true,
                    Salt = newAccount.Salt,
                    CreateDate = DateTime.Now,
                    AccountID = newAccount.AccountID
                };

                _context.Customers.Add(khachhang);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(account);
            }
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
                            // Nếu là Admin, chuyển hướng tới trang Admin (Home Controller trong Admin Area)
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                        }

                        else if (user.Role.RoleName == "user")
                        {
                            // Nếu là User, chuyển hướng tới trang User (hoặc trang khác của người dùng)
                            return RedirectToAction("Index", "Home");
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




        public ActionResult Logout()
        {
            // Lưu lại URL của trang hiện tại trước khi logout
            string currentUrl = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Index", "Home");

            // Xóa session
            Session.Clear();

            // Chuyển hướng về trang mà người dùng vừa truy cập (hoặc trang chủ nếu không có)
            return Redirect(currentUrl);
        }

        


        [HttpGet]
        public ActionResult Profiles()
        {

            if (Session["Email"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int accountId = int.Parse(Session["AccountId"].ToString());

            if (Session["AccountId"] != null)
            {
                accountId = int.Parse(Session["AccountId"].ToString());
                var customer = _context.Customers.FirstOrDefault(c => c.AccountID == accountId);

                if (customer == null)
                {
                    return HttpNotFound("Không tìm thấy thông tin người dùng.");
                }



                return View(customer);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(Customer updatedCustomer)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int accountId = int.Parse(Session["UserID"].ToString());
            var customer = _context.Customers.FirstOrDefault(c => c.AccountID == accountId);

            if (customer != null)
            {
                // Cập nhật các thông tin mới
                customer.FullName = updatedCustomer.FullName;
                customer.Phone = updatedCustomer.Phone;
                customer.Avatar = updatedCustomer.Avatar;

                _context.SaveChanges();

                // Cập nhật Session
                Session["FullName"] = customer.FullName;
                Session["Avatar"] = customer.Avatar;

                return RedirectToAction("Profiles");
            }

            return View("Profiles", updatedCustomer);
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Accounts.FirstOrDefault(c => c.Email == model.Email.Trim().ToLower());

                if (user != null)
                {
                    // Create a reset token
                    var token = Guid.NewGuid().ToString();
                    user.PasswordResetToken = token; 
                    _context.SaveChanges();

                    // Send the email with the reset token
                    try
                    {
                        SendEmail(user.Email, token);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error sending email: " + ex.Message);
                        return View(model);
                    }

                    // Redirect to the ResetPassword action
                    return RedirectToAction("ResetPassword", new { token = token });
                }
                else
                {
                    Console.Write("loi");
                    ModelState.AddModelError("", "Không tìm thấy tài khoản của bạn. Vui lòng đăng ký.");
                    return View(model);
                }
            }

            return View(model);
        }




        public ActionResult ResetPassword(string token)
        {
            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Accounts.FirstOrDefault(c => c.PasswordResetToken == model.Token);

                if (user != null)
                {
                    // Cập nhật mật khẩu
                    user.Password = model.NewPassword;
                    user.PasswordResetToken = null; // Xóa mã sau khi sử dụng
                    _context.SaveChanges();

                    return RedirectToAction("Login", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Mã xác nhận không hợp lệ.");
                }
            }

            return View(model);
        }


        private void SendEmail(string email, string token)
        {
            var fromAddress = new MailAddress("your-email@gmail.com", "Your Name");
            var toAddress = new MailAddress(email);
            const string fromPassword = "your-email-password"; // Use a secure method to store credentials
            const string subject = "Mã xác nhận quên mật khẩu";
            string body = $"Vui lòng sử dụng mã xác nhận sau để đặt lại mật khẩu của bạn: {token}. " +
                          $"Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu: " +
                          $"{Url.Action("ResetPassword", "Home", new { token = token }, Request.Url.Scheme)}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }


    }
}
