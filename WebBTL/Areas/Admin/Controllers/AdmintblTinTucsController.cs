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
    public class AdmintblTinTucsController : Controller
    {
        

        // GET: Admin/AdmintblTinTucs
        
             private readonly Eonon_ProEntities1 _context;

        public AdmintblTinTucsController()
        {
            _context = new Eonon_ProEntities1();
        }

        // GET: Admin/AdminPages
        public ActionResult Index(string searchTerm,int? tintuc)
        {
            var tintucNumber = tintuc == null || tintuc <= 0 ? 1 : tintuc.Value;
            var tintucSize = 20;
            var lsTintuc = _context.tblTinTucs
                .AsNoTracking()
                .OrderBy(x => x.PostID);

            // Nếu có searchTerm, thực hiện tìm kiếm theo CatName
            if (!String.IsNullOrEmpty(searchTerm))
            {
                // Lọc các mục có CatName chứa searchTerm
                lsTintuc = (IOrderedQueryable<tblTinTuc>)lsTintuc.Where(x => x.Title.Contains(searchTerm));
            }

            PagedList<tblTinTuc> models = new PagedList<tblTinTuc>(lsTintuc, tintucNumber, tintucSize);

            ViewBag.CurrentPage = tintucNumber;
            ViewBag.SearchTerm = searchTerm;
            return View(models);
        }

        // GET: Admin/AdmintblTinTucs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblTinTuc tblTinTuc = _context.tblTinTucs.Find(id);
            if (tblTinTuc == null)
            {
                return HttpNotFound();
            }
            return View(tblTinTuc);
        }

        // GET: Admin/AdmintblTinTucs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdmintblTinTucs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PostID,Title,SContents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountID,Tags,CatID,isHot,isNewFeed,MetaKey,MetaDesc,Views")] tblTinTuc tblTinTuc, HttpPostedFileBase image)
        {

            var existingTblTinTuc = _context.tblTinTucs.Find(tblTinTuc.PostID);
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
                    tblTinTuc.Thumb = Url.Content("~/Content/images/products/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save image. Please try again. " + ex.Message);
                    var Categories = _context.Categories.ToList();
                    ViewBag.CatID = new SelectList(Categories, "CategoryID", "CategoryName", tblTinTuc.CatID);
                    return View(tblTinTuc);
                }
            }
            else
            {
                // If no new image is uploaded, keep the existing Thumb value or a default image
                tblTinTuc.Thumb = string.IsNullOrEmpty(existingTblTinTuc.Thumb) ? Url.Content("~/Content/images/default.png") : existingTblTinTuc.Thumb;
            }

            if (ModelState.IsValid)
            {
                _context.tblTinTucs.Add(tblTinTuc);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tblTinTuc);
        }

        // GET: Admin/AdmintblTinTucs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblTinTuc tblTinTuc = _context.tblTinTucs.Find(id);
            if (tblTinTuc == null)
            {
                return HttpNotFound();
            }
            return View(tblTinTuc);
        }

        // POST: Admin/AdmintblTinTucs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PostID,Title,SContents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountID,Tags,CatID,isHot,isNewFeed,MetaKey,MetaDesc,Views")] tblTinTuc tblTinTuc, HttpPostedFileBase image)
        {
            var existingtblTinTuc = _context.tblTinTucs.Find(tblTinTuc.PostID);

            if (existingtblTinTuc == null)
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
                    tblTinTuc.Thumb = Url.Content("~/Content/images/products/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save image. Please try again. " + ex.Message);
                    var Categories = _context.Categories.ToList();
                    ViewBag.CatID = new SelectList(Categories, "CategoryID", "CategoryName", tblTinTuc.CatID);
                    return View(tblTinTuc);
                }
            }
            else
            {
                // If no new image is uploaded, keep the existing Thumb value or a default image
                tblTinTuc.Thumb = string.IsNullOrEmpty(existingtblTinTuc.Thumb) ? Url.Content("~/Content/images/default.png") : existingtblTinTuc.Thumb;
            }


            if (ModelState.IsValid)
            {
                // Update the existing product properties
                existingtblTinTuc.PostID = tblTinTuc.PostID;
                existingtblTinTuc.Title = tblTinTuc.Title;
                existingtblTinTuc.SContents = tblTinTuc.SContents;
                existingtblTinTuc.Contents = tblTinTuc.Contents;
                existingtblTinTuc.Thumb = tblTinTuc.Thumb;
                existingtblTinTuc.Published = tblTinTuc.Published;
                existingtblTinTuc.Alias = tblTinTuc.Alias;
                existingtblTinTuc.CreateDate = DateTime.Now;
                existingtblTinTuc.Author = tblTinTuc.Author;
                existingtblTinTuc.AccountID = tblTinTuc.AccountID;
                existingtblTinTuc.Tags = tblTinTuc.Tags;
                existingtblTinTuc.CatID = tblTinTuc.CatID;
                existingtblTinTuc.isNewFeed = tblTinTuc.isNewFeed;
                existingtblTinTuc.isHot = tblTinTuc.isHot;
                existingtblTinTuc.MetaDesc = tblTinTuc.MetaDesc;
                existingtblTinTuc.MetaKey = tblTinTuc.MetaKey;
                existingtblTinTuc.Views = tblTinTuc.Views;

                // Save changes to the database
                _context.SaveChanges();
                return RedirectToAction("Index");
            }


            return View(tblTinTuc);
        }

        // GET: Admin/AdmintblTinTucs/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var tltTintuc = _context.tblTinTucs.Find(id);

            if (tltTintuc != null)
            {
                _context.tblTinTucs.Remove(tltTintuc);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
