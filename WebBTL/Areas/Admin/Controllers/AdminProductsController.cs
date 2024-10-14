using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBTL.Helper;
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
        public ActionResult Create([Bind(Include = "ProductID,ProductName,ShortDesc,Description,CatID,Price,Discount,Thumb,Video,DateCreated,Datemodified,BestSellers,HomeFlag,Active,Tags,Titles,Alias,MetaDesc,MetaKey,UnitsInStock")] Product product, HttpPostedFileBase image)
        {
            var existingProduct = _context.Products.Find(product.ProductID);
            // Check if a new image is uploaded
            if (image != null && image.ContentLength > 0)
            {
                // Get the image name and set path
                string fileName = System.IO.Path.GetFileName(image.FileName);
                string filePath = Server.MapPath("~/Content/images/products/" + fileName);

                // Ensure the directory exists
                string directoryPath = Server.MapPath("~/Content/images/products/");
                if (!System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                // Try to save the new image
                try
                {
                    image.SaveAs(filePath);
                    // Update the product thumb path with the new image
                    product.Thumb = Url.Content("~/Content/images/products/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save image. Please try again. " + ex.Message);
                    var Categories = _context.Categories.ToList();
                    ViewBag.CatID = new SelectList(Categories, "CategoryID", "CategoryName", product.CatID);
                    return View(product);
                }
            }
            else
            {
                // If no new image is uploaded, keep the existing Thumb value or a default image
                product.Thumb = string.IsNullOrEmpty(existingProduct.Thumb) ? Url.Content("~/Content/images/default.png") : existingProduct.Thumb;
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,ShortDesc,Description,CatID,Price,Discount,Thumb,Video,DateCreated,Datemodified,BestSellers,HomeFlag,Active,Tags,Titles,Alias,MetaDesc,MetaKey,UnitsInStock")] Product product, HttpPostedFileBase image)
        {
            // Retrieve the existing product from the database
            var existingProduct = _context.Products.Find(product.ProductID);
            if (existingProduct == null)
            {
                return HttpNotFound();
            }

            // Check if a new image is uploaded
            if (image != null && image.ContentLength > 0)
            {
                // Get the image name and set path
                string fileName = System.IO.Path.GetFileName(image.FileName);
                string filePath = Server.MapPath("~/Content/images/products/" + fileName);

                // Ensure the directory exists
                string directoryPath = Server.MapPath("~/Content/images/products/");
                if (!System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                // Try to save the new image
                try
                {
                    image.SaveAs(filePath);
                    // Update the product thumb path with the new image
                    product.Thumb = Url.Content("~/Content/images/products/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save image. Please try again. " + ex.Message);
                    var Categories = _context.Categories.ToList();
                    ViewBag.CatID = new SelectList(Categories, "CategoryID", "CategoryName", product.CatID);
                    return View(product);
                }
            }
            else
            {
                // If no new image is uploaded, keep the existing Thumb value or a default image
                product.Thumb = string.IsNullOrEmpty(existingProduct.Thumb) ? Url.Content("~/Content/images/default.png") : existingProduct.Thumb;
            }

            if (ModelState.IsValid)
            {
                // Update the existing product properties
                existingProduct.ProductName = product.ProductName;
                existingProduct.ShortDesc = product.ShortDesc;
                existingProduct.Description = product.Description;
                existingProduct.CatID = product.CatID;
                existingProduct.Price = product.Price;
                existingProduct.Discount = product.Discount;
                existingProduct.Video = product.Video;
                existingProduct.DateCreated = product.DateCreated;
                existingProduct.Datemodified = DateTime.Now; // Update the modified date
                existingProduct.BestSellers = product.BestSellers;
                existingProduct.HomeFlag = product.HomeFlag;
                existingProduct.Active = product.Active;
                existingProduct.Tags = product.Tags;
                existingProduct.Titles = product.Titles;
                existingProduct.Alias = product.Alias;
                existingProduct.MetaDesc = product.MetaDesc;
                existingProduct.MetaKey = product.MetaKey;
                existingProduct.UnitsInStock = product.UnitsInStock;
                existingProduct.Thumb = product.Thumb;  // Update image

                // Save changes to the database
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Repopulate the dropdown if the model state is invalid
            var categories = _context.Categories.ToList();
            ViewBag.CatID = new SelectList(categories, "CategoryID", "CategoryName", product.CatID);
            return View(product);
        }


        // GET: Admin/AdminProducts/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {

            var item = _context.Products.Find(id);
            if (item != null)
            {
                _context.Products.Remove(item);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        
    }
}
