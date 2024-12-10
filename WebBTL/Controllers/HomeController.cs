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
                string hashPassword = Extension.HashMD5.ToMD5(account.Password + salt);

                // Logging salt and hashed password for debugging
                System.Diagnostics.Debug.WriteLine($"Salt: {salt}");
                System.Diagnostics.Debug.WriteLine($"Hashed Password: {hashPassword}");

                Account newAccount = new Account
                {
                    Email = account.Email.Trim().ToLower(),
                    Password = hashPassword,
                    Salt = salt,
                    Active = true,
                    CreateDate = DateTime.Now,
                    RoleID = 2
                };

                _context.Accounts.Add(newAccount);
                _context.SaveChanges();

                Customer khachhang = new Customer
                {
                    Email = newAccount.Email,
                    Password = hashPassword,
                    Active = true,
                    Salt = salt,
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
                var user = _context.Accounts.FirstOrDefault(c => c.Email == customer.Email.Trim().ToLower());

                if (user != null)
                {
                    var enteredPassword = Extension.HashMD5.ToMD5(customer.Password.Trim() + user.Salt.Trim());

                    TempData["DebugInfo"] = $"Stored Salt: {user.Salt}, Entered Hashed Password: {enteredPassword}, Stored Hashed Password: {user.Password}";


                    if (enteredPassword.Equals(user.Password))
                    {
                        Session["Email"] = user.Email;
                        Session["AccountId"] = user.AccountID;
                        Session["Role"] = user.Role.RoleName;

                        var cartCookie = Request.Cookies["cart"]?.Value;
                        if (!string.IsNullOrEmpty(cartCookie))
                        {
                            var cart = JsonConvert.DeserializeObject<List<CartItem>>(cartCookie);
                            Session["Cart"] = cart;
                            var cookie = new HttpCookie("cart") { Expires = DateTime.Now.AddDays(-1) };
                            Response.Cookies.Add(cookie);
                        }

                        string returnUrl = Session["ReturnUrl"] as string;
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            Session.Remove("ReturnUrl");
                            return Redirect(returnUrl);
                        }

                        if (user.Role.RoleName == "admin")
                        {
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                        }
                        else if (user.Role.RoleName == "user")
                        {
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

            return View(customer);
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
        public ActionResult ForgotPassword(ForgotPasswordViewModel forgotPassword)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Accounts.FirstOrDefault(c => c.Email == forgotPassword.Email.Trim().ToLower());

                if (user != null)
                {
                    var enteredPassword = forgotPassword.newPassword;

                    if (enteredPassword.Equals(user.Password))
                    {
                        ModelState.AddModelError("", "Your password must not be the same as the old password!");
                        return View(forgotPassword);
                    }
                    else
                    {
                        user.Password = forgotPassword.newPassword;
                        _context.SaveChanges();
                        return RedirectToAction("Login", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Can't find your account. Please register");
                    return View(forgotPassword);
                }

            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(forgotPassword);
            }

        }

        //[HttpGet]
        //public ActionResult ResetPassword()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ResetPassword(ResetPasswordViewModel resetPassword)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = _context.Accounts.FirstOrDefault(c => c.Password == resetPassword.Password);

        //        if (user != null)
        //        {
        //            var enteredPassword = resetPassword.Password;

        //            if (enteredPassword.Equals(user.Password))
        //            {
        //                if (!resetPassword.Equals(user.Password))
        //                {
        //                    user.Password = resetPassword.newPassword;
        //                    _context.SaveChanges();
        //                    return RedirectToAction("Index", "Home");
        //                }
        //                else
        //                {
        //                    ModelState.AddModelError("", "Your password must not be the same as the old password!");
        //                    return View(resetPassword);
        //                }
        //            }
        //            else
        //            {
        //                ModelState.AddModelError("", "Wrong Password!");
        //                return View(resetPassword);
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "Something went wrong!");
        //            return View(resetPassword);
        //        }

        //    }
        //    else
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors);
        //        return View(resetPassword);
        //    }
        //}

        [HttpGet]
        public ActionResult ExternalLoginConfirmationViewModel(string returnUrl)
        {
            return View();
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
