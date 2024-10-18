using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebBTL.Models;

namespace WebBTL.Areas.Admin.Controllers
{
    public class AdminAccountsController : Controller
    {

        private readonly Eonon_ProEntities _context;

        public AdminAccountsController()
        {
            _context = new Eonon_ProEntities();
        }

        // GET: Admin/AdminAccounts
        public ActionResult Index(int? RoleId = null)
        {

            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "RoleName");

            List<SelectListItem> IsTrangThai = new List<SelectListItem>();
            IsTrangThai.Add(new SelectListItem() { Text = "Active", Value = "1" });
            IsTrangThai.Add(new SelectListItem() { Text = "Block", Value = "0" });
            ViewData["IsTrangThai"] = IsTrangThai;

            var accounts = _context.Account.Include(a => a.Roles).AsQueryable();

            // Lọc theo quyền truy cập nếu có giá trị
            if (RoleId.HasValue)
            {
                accounts = accounts.Where(a => a.RoleID == RoleId);
            }


            return View(accounts.ToList());
        }

        // GET: Admin/AdminAccounts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = _context.Account.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // GET: Admin/AdminAccounts/Create
     

        // GET: Admin/AdminAccounts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = _context.Account.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            ViewBag.RoleID = new SelectList(_context.Roles, "RoleID", "RoleName", account.RoleID);
            return View(account);
        }

        // POST: Admin/AdminAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AccountID,Phone,Email,Password,Salt,Active,FullName,RoleID,LastLogin,CreateDate")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(account).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RoleID = new SelectList(_context.Roles, "RoleID", "RoleName", account.RoleID);
            return View(account);
        }

        // GET: Admin/AdminAccounts/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var account = _context.Account.Find(id);

            if (account != null)
            {
                _context.Account.Remove(account); 
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }



    }
}
