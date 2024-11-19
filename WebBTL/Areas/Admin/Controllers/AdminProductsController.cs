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
        public ActionResult Create([Bind(Include = "ProductID,ProductName,ShortDesc,Description,CatID,Price,Discount,Thumb,Video,DateCreated,Datemodified,BestSellers,HomeFlag,Active,Tags,Titles,Alias,MetaDesc,MetaKey,UnitsInStock")] Product product, HttpPostedFileBase image)
        {
            // Kiểm tra nếu có ảnh mới được tải lên
            if (image != null && image.ContentLength > 0)
            {
                // Lấy tên file và đường dẫn lưu ảnh
                string fileName = System.IO.Path.GetFileName(image.FileName);
                string filePath = Server.MapPath("~/Content/images/products/" + fileName);

                // Đảm bảo thư mục lưu ảnh tồn tại
                string directoryPath = Server.MapPath("~/Content/images/products/");
                if (!System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                // Cố gắng lưu ảnh vào thư mục
                try
                {
                    image.SaveAs(filePath);
                    // Cập nhật đường dẫn ảnh trong sản phẩm
                    product.Thumb = Url.Content("~/Content/images/products/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Không thể lưu ảnh. Vui lòng thử lại. " + ex.Message);
                    var categories = _context.Categories.ToList();
                    ViewBag.CatID = new SelectList(categories, "CategoryID", "CategoryName", product.CatID);
                    return View(product);
                }
            }
            else
            {
                // Nếu không có ảnh được tải lên, sử dụng ảnh mặc định
                product.Thumb = Url.Content("~/Content/images/default.png");
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Trả về view nếu có lỗi trong ModelState
            var categoriesList = _context.Categories.ToList();
            ViewBag.CatID = new SelectList(categoriesList, "CategoryID", "CategoryName", product.CatID);
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
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,ShortDesc,Description,CatID,Price,Discount,Thumb,Video,DateCreated,Datemodified,BestSellers,HomeFlag,Active,Tags,Titles,Alias,MetaDesc,MetaKey,UnitsInStock")] Product product, HttpPostedFileBase image)
        {
            // Lấy sản phẩm hiện tại từ cơ sở dữ liệu
            var existingProduct = _context.Products.Find(product.ProductID);
            if (existingProduct == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra nếu có ảnh mới được upload
            if (image != null && image.ContentLength > 0)
            {
                // Lấy tên file và tạo đường dẫn
                string fileName = System.IO.Path.GetFileName(image.FileName);
                string filePath = Server.MapPath("~/Content/images/products/" + fileName);

                // Kiểm tra xem file đã tồn tại trong thư mục hay chưa
                if (!System.IO.File.Exists(filePath))
                {
                    // Nếu file chưa tồn tại, lưu ảnh mới
                    try
                    {
                        // Đảm bảo thư mục lưu ảnh tồn tại
                        string directoryPath = Server.MapPath("~/Content/images/products/");
                        if (!System.IO.Directory.Exists(directoryPath))
                        {
                            System.IO.Directory.CreateDirectory(directoryPath);
                        }

                        // Lưu ảnh vào thư mục
                        image.SaveAs(filePath);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Không thể lưu ảnh. Vui lòng thử lại. " + ex.Message);
                        var Categories = _context.Categories.ToList();
                        ViewBag.CatID = new SelectList(Categories, "CategoryID", "CategoryName", product.CatID);
                        return View(product);
                    }
                }
                // Dù ảnh đã tồn tại hay vừa lưu mới, cập nhật đường dẫn ảnh
                existingProduct.Thumb = Url.Content("~/Content/images/products/" + fileName);
            }

            if (ModelState.IsValid)
            {
                // Cập nhật các thuộc tính của sản phẩm hiện tại (bao gồm Thumb)
                existingProduct.ProductName = product.ProductName;
                existingProduct.ShortDesc = product.ShortDesc;
                existingProduct.Description = product.Description;
                existingProduct.CatID = product.CatID;
                existingProduct.Price = product.Price;
                existingProduct.Discount = product.Discount;
                existingProduct.Video = product.Video;
                existingProduct.DateCreated = product.DateCreated;
                existingProduct.Datemodified = DateTime.Now;
                existingProduct.BestSellers = product.BestSellers;
                existingProduct.HomeFlag = product.HomeFlag;
                existingProduct.Active = product.Active;
                existingProduct.Tags = product.Tags;
                existingProduct.Titles = product.Titles;
                existingProduct.Alias = product.Alias;
                existingProduct.MetaDesc = product.MetaDesc;
                existingProduct.MetaKey = product.MetaKey;
                existingProduct.UnitsInStock = product.UnitsInStock;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Tạo lại dropdown list nếu ModelState không hợp lệ
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
