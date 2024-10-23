using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using WebBTL.Models;
using WebBTL.Extension;

using WebBTL.Helper;
using System.Security.Principal;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
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
            return View();
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

                var existingCustomer = _context.Customer.FirstOrDefault(c => c.Email == account.Email.Trim().ToLower());
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

                _context.Account.Add(newAccount);
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

                _context.Customer.Add(khachhang);
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
                var user = _context.Account.FirstOrDefault(s => s.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    var enteredPassword = customer.Password; 
                    

                    if (enteredPassword.Equals(user.Password))
                    {
                        Session["Email"] = user.Email;
                        Session["AccountId"] = user.AccountID;
                        return RedirectToAction("Index", "Home");
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

            // If we got this far, something failed; redisplay form.
            return View(customer); // Return the view with the current model state to show validation errors
        }



        public ActionResult Logout()
        {
            Session.Clear(); 
            return RedirectToAction("Index", "Home");
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
                var customer = _context.Customer.FirstOrDefault(c => c.AccountID == accountId);

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
            var customer = _context.Customer.FirstOrDefault(c => c.AccountID == accountId);

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
        public ActionResult ForgotPassword(ForgotPasswordViewModel forgotPassword)
        {
            if (ModelState.IsValid) 
            {
                var user = _context.Account.FirstOrDefault(c => c.Email == forgotPassword.Email.Trim().ToLower());

                if (user != null) 
                {
                    var enteredPassword = forgotPassword.newPassword;
                    
                    if(enteredPassword.Equals(user.Password))
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

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel resetPassword)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Account.FirstOrDefault(c => c.Password == resetPassword.Password);

                if (user != null)
                {
                    var enteredPassword = resetPassword.Password;

                    if (enteredPassword.Equals(user.Password))
                    {
                        if (!resetPassword.Equals(user.Password))
                        {
                            user.Password = resetPassword.newPassword;
                            _context.SaveChanges();
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Your password must not be the same as the old password!");
                            return View(resetPassword);
                        }
                    }
                    else
                    { 
                        ModelState.AddModelError("", "Wrong Password!");
                        return View(resetPassword);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Something went wrong!");
                    return View(resetPassword);
                }

            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(resetPassword);
            }
        }

        [HttpGet]
        public ActionResult ExternalLoginConfirmationViewModel(string returnUrl)
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
        //        if (info == null)
        //        {
        //            return View("ExternalLoginFailure");
        //        }

        //        var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //        var result = await UserManager.CreateAsync(user);
        //        if (result.Succeeded)
        //        {
        //            result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //            if (result.Succeeded)
        //            {
        //                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
        //                return RedirectToLocal(returnUrl);
        //            }
        //        }
        //        AddErrors(result);
        //    }

        //    // Nếu có lỗi, quay lại view với ViewModel cũ
        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(model);
        //}
    }
}
