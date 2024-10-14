using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBTL.Models;
using PagedList;
using System.Security.Principal;

namespace WebBTL.Areas.Admin.Controllers
{
    public class AdminCategoriesController : Controller
    {
        private readonly Eonon_ProEntities1 _context;

        public AdminCategoriesController()
        {
            _context = new Eonon_ProEntities1();
        }

        // GET: Admin/AdminCategories
        public ActionResult Index(string searchTerm, int? page)
        {
            // Số trang hiện tại, mặc định là 1 nếu không có giá trị được truyền vào
            var pageNumber = page ?? 1;

            // Kích thước trang, số mục trên mỗi trang
            var pageSize = 10;

            // Lấy danh sách Categories (chưa phân trang) và sắp xếp theo CatID
            var lsCategories = _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.CatID);

            // Nếu có searchTerm, thực hiện tìm kiếm theo CatName
            if (!String.IsNullOrEmpty(searchTerm))
            {
                // Lọc các mục có CatName chứa searchTerm
                lsCategories = (IOrderedQueryable<Category>)lsCategories.Where(x => x.CatName.Contains(searchTerm));
            }

            // Phân trang sau khi đã tìm kiếm
            var pagedListCategories = lsCategories.ToPagedList(pageNumber, pageSize);

            // Truyền searchTerm vào ViewBag để giữ giá trị tìm kiếm trên View
            ViewBag.SearchTerm = searchTerm;

            // Truyền dữ liệu phân trang vào View
            ViewBag.CurrentPage = pageNumber;
            return View(pagedListCategories);
        }



        // GET: Admin/AdminCategories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = _context.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Admin/AdminCategories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CatID,CatName,Description,ParentID,Leveks,Ordering,Published,Thumb,Title,Alias,MetaDesc,MetaKey,Cover,SchemaMarkup")] Category category, HttpPostedFileBase image)
        {

            var existingProduct = _context.Products.Find(category.CatID);
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
                    category.Thumb = Url.Content("~/Content/images/products/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save image. Please try again. " + ex.Message);
                    var Categories = _context.Categories.ToList();
                    ViewBag.CatID = new SelectList(Categories, "CategoryID", "CategoryName", category.CatID);
                    return View(category);
                }
            }
            else
            {
                // If no new image is uploaded, keep the existing Thumb value or a default image
                category.Thumb = string.IsNullOrEmpty(existingProduct.Thumb) ? Url.Content("~/Content/images/default.png") : existingProduct.Thumb;
            }

            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/AdminCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = _context.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/AdminCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CatID,CatName,Description,ParentID,Leveks,Ordering,Published,Thumb,Title,Alias,MetaDesc,MetaKey,Cover,SchemaMarkup")] Category category, HttpPostedFileBase image)
        {
            var existingCategory = _context.Categories.Find(category.CatID);

            if (existingCategory == null)
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
                    category.Thumb = Url.Content("~/Content/images/products/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save image. Please try again. " + ex.Message);
                    var Categories = _context.Categories.ToList();
                    ViewBag.CatID = new SelectList(Categories, "CategoryID", "CategoryName", category.CatID);
                    return View(category);
                }
            }
            else
            {
                // If no new image is uploaded, keep the existing Thumb value or a default image
                category.Thumb = string.IsNullOrEmpty(existingCategory.Thumb) ? Url.Content("~/Content/images/default.png") : existingCategory.Thumb;
            }


            if (ModelState.IsValid)
            {
                // Update the existing product properties
                existingCategory.CatID = category.CatID;
                existingCategory.CatName = category.CatName;
                existingCategory.Cover = category.Cover;
                existingCategory.Leveks = category.Leveks;
                existingCategory.SchemaMarkup = category.SchemaMarkup;
                existingCategory.Ordering = category.Ordering;
                existingCategory.Alias = category.Alias;
                existingCategory.Description = category.Description;
                existingCategory.ParentID = category.ParentID;
                existingCategory.Published = category.Published;
                existingCategory.Thumb = category.Thumb;
                existingCategory.Title = category.Title;
                existingCategory.MetaDesc = category.MetaDesc;
                existingCategory.MetaKey = category.MetaKey;

                // Save changes to the database
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Repopulate the dropdown if the model state is invalid
            var categories = _context.Categories.ToList();
            ViewBag.CatID = new SelectList(categories, "CategoryID", "CategoryName", category.CatID);
            return View(category);
        }

        // GET: Admin/AdminCategories/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);

            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


    }
}
