using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebBTL.Extension;
using WebBTL.Helper;
using WebBTL.Models;

namespace WebBTL.Controllers
{
    public class AccountsController : Controller
    {
        private readonly Eonon_ProEntities _context;

        public AccountsController()
        {
            _context = new Eonon_ProEntities();
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

                    var user = _context.Account.FirstOrDefault(s => s.Email.Equals(customer.Email));

                    if (user != null)
                    {
                        var enteredPassword = (customer.Password.Trim().ToLower()).ToMD5();
                        var dbPassword = user.Password.Trim().ToLower();

                        if (enteredPassword.Equals(dbPassword))
                        {
                            Session["Email"] = user.Email;
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid Email or Password");
                            return RedirectToAction("Register", "Accounts");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error";
                        ModelState.AddModelError("", "Can't find your account. Please register");
                            return View();
                    }
                }   
                else if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    return View(customer);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid");
                    return RedirectToAction("Register", "Accounts");
                }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }


    }
}