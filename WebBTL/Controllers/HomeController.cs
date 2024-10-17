using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBTL.Models;

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
            if(content.PageID== 5)
            {
                
            }
           return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}