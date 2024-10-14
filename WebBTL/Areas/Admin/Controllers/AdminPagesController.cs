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
    public class AdminPagesController : Controller
    {
        private readonly Eonon_ProEntities1 _context;

        public AdminPagesController()
        {
            _context = new Eonon_ProEntities1();
        }

        // GET: Admin/AdminPages
        public ActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var lsPages = _context.Pages
                .AsNoTracking()
                .OrderByDescending(x => x.PageID);

            PagedList<Page> models = new PagedList<Page>(lsPages, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);

        }

        // GET: Admin/AdminPages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Page page = _context.Pages.Find(id);
            if (page == null)
            {
                return HttpNotFound();
            }
            return View(page);
        }

        // GET: Admin/AdminPages/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminPages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PageID,PageName,Contents,Thumb,Published,Title,MetaDesc,MetaKey,Alias,CreateDate,Ordering")] Page page)
        {



            if (ModelState.IsValid)
            {
                _context.Pages.Add(page);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(page);
        }

        // GET: Admin/AdminPages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Page page = _context.Pages.Find(id);
            if (page == null)
            {
                return HttpNotFound();
            }
            return View(page);
        }

        // POST: Admin/AdminPages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PageID,PageName,Contents,Thumb,Published,Title,MetaDesc,MetaKey,Alias,CreateDate,Ordering")] Page page, HttpPostedFileBase image)
        {
            // Retrieve the existing page from the database
            var existingPage = _context.Pages.Find(page.PageID);
            if (existingPage == null)
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
                    // Update the page thumb path with the new image
                    existingPage.Thumb = Url.Content("~/Content/images/products/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save image. Please try again. " + ex.Message);
                    return View(page);
                }
            }

            // If no new image is uploaded, keep the existing Thumb value or a default image
            if (string.IsNullOrEmpty(existingPage.Thumb))
            {
                existingPage.Thumb = Url.Content("~/Content/images/default.png");
            }

            if (ModelState.IsValid)
            {
                // Update the existing page properties
                existingPage.PageName = page.PageName;
                existingPage.Alias = page.Alias;
                existingPage.MetaDesc = page.MetaDesc;
                existingPage.Title = page.Title;
                existingPage.Contents = page.Contents;
                existingPage.Published = page.Published;
                existingPage.Ordering = page.Ordering;
                existingPage.CreateDate = DateTime.Now; // Update the modified date
                existingPage.MetaKey = page.MetaKey;

                // Save changes to the database
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate the dropdown if the model state is invalid
            return View(page);
        }



        // GET: Admin/AdminPages/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var page = _context.Pages.Find(id);

            if (page != null)
            {
                _context.Pages.Remove(page);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
