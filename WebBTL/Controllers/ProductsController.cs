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
            var pageNumber = page ?? 1;
            var pageSize = 20;

            // Bắt đầu với danh sách sản phẩm, nếu có id thì lọc theo CatID, nếu không thì hiển thị tất cả sản phẩm
            var products = _context.Products
                .Include(c => c.Category)
                .AsNoTracking()
                .AsQueryable();

            // Nếu id (CatID) có giá trị, lọc theo danh mục sản phẩm
            if (id.HasValue)
            {
                products = products.Where(x => x.CatID == id);
            }

            // Lọc theo tên sản phẩm nếu searchTerm không rỗng
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(x => x.ProductName.Contains(searchTerm));
            }

            // Thực hiện phân trang trên danh sách sản phẩm đã được lọc
            var models = new PagedList<Product>(products.OrderBy(p => p.ProductID), pageNumber, pageSize);

            // Lưu giá trị searchTerm để hiển thị lại trên view
            ViewBag.SearchTerm = searchTerm;
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

            var lsSanPhamLienQuan = _context.Products
             .AsNoTracking().Include(x => x.Category)
             .Where(x => x.Active == true && x.CatID == product.CatID && x.ProductID != id)
             .Take(3)
            .ToList();


            ViewBag.lsSanPhamLienQuan = lsSanPhamLienQuan;
            return View(productsList);
        }
                
    }
}