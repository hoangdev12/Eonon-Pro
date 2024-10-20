using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using WebBTL.Models;
using WebBTL.Extension;
using WebBTL.Helper;

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
                var user = _context.Accounts.FirstOrDefault(s => s.Email.Equals(customer.Email, StringComparison.OrdinalIgnoreCase));

                if (user != null)
                {
                    var enteredPassword = customer.Password; 
                    

                    if (enteredPassword.Equals(user.Password))
                    {
                        Session["Email"] = user.Email;
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

            int accountId = Convert.ToInt32(Session["Email"]);

            var customer = _context.Customers.FirstOrDefault(c => c.AccountID == accountId);

            if (customer == null)
            {
                return HttpNotFound("Không tìm thấy thông tin người dùng.");
            }

            ViewBag.FullName = customer.FullName;
            ViewBag.Email = customer.Email;
            ViewBag.Phone = customer.Phone;
            ViewBag.Avatar = customer.Avatar;

            return View(customer); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(Customer updatedCustomer)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int accountId = Convert.ToInt32(Session["UserID"]);
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
    }
}
