using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using WebBTL.Models;
using WebBTL.Extension;

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

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Account account)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                // In ra lỗi để kiểm tra
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // Log hoặc in ra
                }
                return View(account); // Trả về cùng một view với lỗi xác thực
            }

            // Check if the account already exists
            var existingAccount = _context.Accounts.FirstOrDefault(s => s.Email == account.Email);
            if (existingAccount != null)
            {
                ModelState.AddModelError("Email", "Email is already in use.");
                return View(account); // Return the same view with an error
            }

            // Hash the password using MD5 (or preferably a more secure method)
            account.Password = account.Password.ToMD5(); // Cân nhắc đổi sang phương pháp bảo mật hơn

            account.CreateDate = DateTime.Now;

            try
            {
                // Save the new account to the database
                _context.Accounts.Add(account);
                _context.SaveChanges(); // Entity Framework sẽ tự động gán giá trị cho khóa chính

                // Thêm thông báo thành công vào ViewBag
                ViewBag.SuccessMessage = "Registration successful!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error while saving account. Please try again.");
                // Log error if needed (ex.Message)
                return View(account);
            }
        }





    }
}
