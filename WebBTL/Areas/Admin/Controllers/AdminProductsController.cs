using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBTL.Models;

namespace WebBTL.Areas.Admin.Controllers
{
    public class AdminProductsController : Controller
    {
        

        private readonly Eonon_ProEntities1 _context;

        public AdminProductsController()
        {
            _context = new Eonon_ProEntities1();
        }

        // GET: Admin/AdminProducts
        public ActionResult Index(string searchTerm, int page = 1, int pageSize = 10, int? CategoryId = null, int? TrangThai = null )
        {
            // Lấy danh sách các thể loại để hiển thị trong dropdown
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatID", "CatName");

            // Lấy trạng thái sản phẩm (InStock và OutStock)
            List<SelectListItem> IsTrangThai = new List<SelectListItem>();
            IsTrangThai.Add(new SelectListItem() { Text = "In stock", Value = "1" });
            IsTrangThai.Add(new SelectListItem() { Text = "Out Stock", Value = "0" });
            ViewData["IsTrangThai"] = IsTrangThai;

            // Lấy danh sách sản phẩm từ CSDL
            var products = _context.Products
            .Include(c => c.Category)
            .OrderBy(p => p.ProductID) 
            .AsQueryable();


            // Lọc theo CategoryId nếu có giá trị
            if (CategoryId.HasValue && CategoryId != 0)
            {
                products = products.Where(p => p.CatID == CategoryId);
            }

            // Lọc theo trạng thái (UnitsInStock > 0 là InStock)
            if (TrangThai.HasValue)
            {
                if (TrangThai == 1) // InStock
                {
                    products = products.Where(p => p.UnitsInStock > 0);
                }
                else if (TrangThai == 0) // OutStock
                {
                    products = products.Where(p => p.UnitsInStock == 0);
                }
            }
         
            if (!String.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(x => x.ProductName.Contains(searchTerm));
            }


            ViewBag.SearchTerm = searchTerm;           
            ViewBag.TrangThai = TrangThai;
            

            // Phân trang
            var pagedProducts = products.ToList().ToPagedList(page, pageSize);

            return View(pagedProducts);
        }


        // GET: Admin/AdminProducts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = _context.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/AdminProducts/Create
        public ActionResult Create()
        {
            ViewBag.CatID = new SelectList(_context.Categories, "CatID", "CatName");
            return View();
        }

        // POST: Admin/AdminProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,ShortDesc,Description,CatID,Price,Discount,Thumb,Video,DateCreated,Datemodified,BestSellers,HomeFlag,Active,Tags,Titles,Alias,MetaDesc,MetaKey,UnitsInStock")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CatID = new SelectList(_context.Categories, "CatID", "CatName", product.CatID);
            return View(product);
        }

        // GET: Admin/AdminProducts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = _context.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CatID = new SelectList(_context.Categories, "CatID", "CatName", product.CatID);
            return View(product);
        }

        // POST: Admin/AdminProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,ShortDesc,Description,CatID,Price,Discount,Thumb,Video,DateCreated,Datemodified,BestSellers,HomeFlag,Active,Tags,Titles,Alias,MetaDesc,MetaKey,UnitsInStock")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(product).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CatID = new SelectList(_context.Categories, "CatID", "CatName", product.CatID);
            return View(product);
        }

        // GET: Admin/AdminProducts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = _context.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = _context.Products.Find(id);
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
