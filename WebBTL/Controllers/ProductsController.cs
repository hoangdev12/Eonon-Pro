using PagedList;
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
        public ActionResult Index(string searchTerm, string tag, int? page, int? id)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var lsPages = _context.Products.Where(x => x.CatID == id)
                .AsNoTracking()
                .OrderBy(x => x.ProductID);

            var products = _context.Products.Include(c => c.Category).OrderBy(p => p.ProductID).AsQueryable();

            if (!String.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(x => x.ProductName.Contains(searchTerm));
            }


            ViewBag.SearchTerm = searchTerm;

            PagedList<Product> models = new PagedList<Product>((IQueryable<Product>)lsPages, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);
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