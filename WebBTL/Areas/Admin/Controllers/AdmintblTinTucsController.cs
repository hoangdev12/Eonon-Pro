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
        public ActionResult Index(int? tintuc)
        {
            var tintucNumber = tintuc == null || tintuc <= 0 ? 1 : tintuc.Value;
            var tintucSize = 20;
            var lsTintuc = _context.tblTinTucs
                .AsNoTracking()
                .OrderByDescending(x => x.PostID);

            PagedList<tblTinTuc> models = new PagedList<tblTinTuc>(lsTintuc, tintucNumber, tintucSize);

            ViewBag.CurrentPage = tintucNumber;
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
        public ActionResult Create([Bind(Include = "PostID,Title,SContents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountID,Tags,CatID,isHot,isNewFeed,MetaKey,MetaDesc,Views")] tblTinTuc tblTinTuc)
        {
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
        public ActionResult Edit([Bind(Include = "PostID,Title,SContents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountID,Tags,CatID,isHot,isNewFeed,MetaKey,MetaDesc,Views")] tblTinTuc tblTinTuc)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(tblTinTuc).State = EntityState.Modified;
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
