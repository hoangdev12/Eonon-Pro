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
using System.IO;
using PagedList;
using System.Data;
using System.Data.Entity;

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
                    CreateDate = DateTime.Now,
                    RoleID = 2
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

            if (Session["AccountId"] != null)
            {
                int accountId = int.Parse(Session["AccountId"].ToString());
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

        [HttpGet]
        public ActionResult UpdateProfile()
        {
            // Retrieve user data for display
            if (Session["AccountId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int accountId = int.Parse(Session["AccountId"].ToString());
            var customer = _context.Customers.FirstOrDefault(c => c.AccountID == accountId);

            if (customer == null)
            {
                return HttpNotFound("Không tìm thấy thông tin người dùng.");
            }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(Customer updatedCustomer, HttpPostedFileBase image)
        {
            if (Session["AccountId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int accountId = int.Parse(Session["AccountId"].ToString());
            var customer = _context.Customers.FirstOrDefault(c => c.AccountID == accountId);
            var account = _context.Accounts.FirstOrDefault(c => c.AccountID == accountId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin tài khoản.";
                return RedirectToAction("Profiles");
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra nếu không có thay đổi
                bool hasChanges = false;

                if (customer.FullName != updatedCustomer.FullName)
                {
                    customer.FullName = updatedCustomer.FullName;
                    account.FullName = updatedCustomer.FullName;
                    hasChanges = true;
                }

                if (customer.Phone != updatedCustomer.Phone)
                {
                    customer.Phone = updatedCustomer.Phone;
                    account.Phone = updatedCustomer.Phone;
                    hasChanges = true;
                }

                if (customer.Address != updatedCustomer.Address)
                {
                    customer.Address = updatedCustomer.Address;

                    hasChanges = true;
                }

                if (image != null && image.ContentLength > 0)
                {
                    var validImageTypes = new[] { "image/gif", "image/jpeg", "image/png" };

                    if (!validImageTypes.Contains(image.ContentType))
                    {
                        ModelState.AddModelError("", "Please choose either a GIF, JPG or PNG image.");
                        return View(customer);
                    }

                    string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(image.FileName);
                    string filePath = Server.MapPath("~/Content/images/avatar/" + fileName);

                    try
                    {
                        string directoryPath = Server.MapPath("~/Content/images/avatar/");
                        if (!System.IO.Directory.Exists(directoryPath))
                        {
                            System.IO.Directory.CreateDirectory(directoryPath);
                        }

                        image.SaveAs(filePath);
                        customer.Avatar = Url.Content("~/Content/images/avatar/" + fileName);
                        hasChanges = true;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Không thể lưu ảnh. Vui lòng thử lại. " + ex.Message);
                        return View(customer);
                    }
                }

                // Nếu không có thay đổi, không lưu và hiển thị thông báo
                if (!hasChanges)
                {
                    TempData["Info"] = "Không có thay đổi nào được thực hiện.";
                    return RedirectToAction("Profiles");
                }

                // Lưu thay đổi
                _context.SaveChanges();

                // Cập nhật session
                Session["FullName"] = customer.FullName;
                Session["Avatar"] = customer.Avatar;

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profiles");
            }

            TempData["Error"] = "Thông tin không hợp lệ.";
            return View(customer);
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

        public ActionResult OrderHistory()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (Session["AccountId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int accountId = int.Parse(Session["AccountId"].ToString());

            // Lấy danh sách đơn hàng của người dùng
            var orders = _context.Orders
                                 .Where(o => o.Customer.AccountID == accountId)  // Lọc theo tài khoản người dùng
                                 .OrderByDescending(o => o.Orderdate)    // Sắp xếp theo ngày đặt đơn hàng
                                 .ToList();

            return View(orders);
        }

        public ActionResult OrderDetails(int orderId)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (Session["AccountId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int accountId = int.Parse(Session["AccountId"].ToString());

            // Lấy đơn hàng theo OrderId và AccountId
            var order = _context.Orders
                                .Where(o => o.OrderID == orderId && o.Customer.AccountID == accountId)
                                .FirstOrDefault();

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.";
                return RedirectToAction("OrderHistory"); // Chuyển về trang lịch sử đơn hàng
            }

            // Lấy chi tiết các sản phẩm trong đơn hàng
            var orderDetails = _context.OrderDetails
                                       .Where(od => od.OrderID == orderId)
                                       .ToList();

            // Tạo ViewModel để chứa thông tin đơn hàng và chi tiết đơn hàng
            var orderViewModel = new OrderDetailsViewModel
            {
                Order = order,
                OrderDetails = orderDetails
            };

            return View(orderViewModel);
        }



    }
}
