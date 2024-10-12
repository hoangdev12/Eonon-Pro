﻿using PagedList;
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
        public async Task<ActionResult> Create([Bind(Include = "ProductID,ProductName,ShortDesc,Description,CatID,Price,Discount,Thumb,Video,DateCreated,Datemodified,BestSellers,HomeFlag,Active,Tags,Titles,Alias,MetaDesc,MetaKey,UnitsInStock")] Product product, HttpPostedFileBase fThumb)
        {
            
            if (ModelState.IsValid)
            {

                

                // Handle thumbnail upload
                if (fThumb != null && fThumb.ContentLength > 0)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string imageName = Utilities.SEOurl(product.ProductName) + extension;
                    product.Thumb = await Utilities.UploadFile(fThumb, @"products", imageName.ToLower());
                }

                // Set a default thumbnail if none was uploaded
                if (string.IsNullOrEmpty(product.Thumb))
                    product.Thumb = "default.jpg";

                // Add the product to the context
                _context.Products.Add(product);

                try
                {
                    // Save changes to the database
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbEntityValidationException ex)
                {
                    // Log validation errors for debugging
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
            }

            // If we got this far, something failed; redisplay the form
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
        public async Task<ActionResult> Edit([Bind(Include = "ProductID,ProductName,ShortDesc,Description,CatID,Price,Discount,Thumb,Video,DateCreated,Datemodified,BestSellers,HomeFlag,Active,Tags,Titles,Alias,MetaDesc,MetaKey,UnitsInStock")] Product product, HttpPostedFileBase fThumb)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing product from the database
                var existingProduct = await _context.Products.FindAsync(product.ProductID);

                if (existingProduct == null)
                {
                    return HttpNotFound(); 
                }

                // Handle thumbnail upload
                if (fThumb != null && fThumb.ContentLength > 0)
                {
                    // If a new thumbnail is uploaded, delete the old one
                    if (!string.IsNullOrEmpty(existingProduct.Thumb) && existingProduct.Thumb != "default.jpg")
                    {
                        var oldImagePath = Path.Combine(Server.MapPath("~/products"), existingProduct.Thumb);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath); // Delete the old file
                        }
                    }

                    // Upload the new thumbnail
                    string extension = Path.GetExtension(fThumb.FileName);
                    string imageName = Utilities.SEOurl(product.ProductName) + extension;
                    product.Thumb = await Utilities.UploadFile(fThumb, @"products", imageName.ToLower());
                }
                else
                {
                    // If no new thumbnail is uploaded, keep the existing one
                    product.Thumb = existingProduct.Thumb;
                }

                // Update properties from the product being edited
                existingProduct.ProductName = product.ProductName;
                existingProduct.ShortDesc = product.ShortDesc;
                existingProduct.Description = product.Description;
                existingProduct.CatID = product.CatID;
                existingProduct.Price = product.Price;
                existingProduct.Discount = product.Discount;
                existingProduct.Video = product.Video;
                existingProduct.DateCreated = existingProduct.DateCreated; // Keep the original creation date
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

                // Mark the entity as modified and save changes
                _context.Entry(existingProduct).State = EntityState.Modified;

                try
                {
                    // Save changes to the database
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbEntityValidationException ex)
                {
                    // Log validation errors for debugging
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
            }

            // Re-populate the category select list if model state is invalid
            ViewBag.CatID = new SelectList(_context.Categories, "CatID", "CatName", product.CatID);

            // Return the same view with the product model to show validation errors
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
