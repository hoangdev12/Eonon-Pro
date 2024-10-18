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

        public ActionResult About(int id)
        {
            var content = _context.Pages.FirstOrDefault(x => x.PageID == id);
            ViewBag.Message = "Your application description page.";
            ViewBag.Content = content;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
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
                string salt = Utilities.GetRandomKey();
                Customer khachhang = new Customer
                {
                    Email = account.Email.Trim().ToLower(),
                    Password = (account.Password + salt.Trim()).ToMD5(),
                    Active = true,
                    Salt = salt.Trim(),
                    CreateDate = DateTime.Now

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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(customer);
            }

            var user = _context.Accounts.FirstOrDefault(s => s.Email.Equals(customer.Email));
            if (user == null)
            {
                ModelState.AddModelError("", "Không thể tìm thấy tài khoản của bạn. Vui lòng đăng ký");
                return View(customer); // Trở về trang đăng nhập với thông báo lỗi
            }

            var enteredPassword = (customer.Password.Trim().ToLower()).ToMD5(); // Thay đổi sang mã hóa an toàn
            var dbPassword = user.Password.Trim().ToLower();

            if (enteredPassword.Equals(dbPassword))
            {
                Session["Email"] = user.Email;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Email hoặc Mật khẩu không hợp lệ");
                return View(customer); 
            }
        }



    }
}
