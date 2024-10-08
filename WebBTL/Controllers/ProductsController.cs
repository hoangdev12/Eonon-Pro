using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBTL.Models;

namespace WebBTL.Controllers
{
    public class ProductsController : Controller
    {
        private readonly Eonon_ProEntities1 _context;

        public ProductsController()
        {
            _context = new Eonon_ProEntities1();
        }



        // GET: Products
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int id)
        {


            var product = _context.Products.Include(x => x.Category).FirstOrDefault(x => x.ProductID == id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }
            var productsList = new List<Product> { product };
            return View(productsList);
        }
                

    }
}