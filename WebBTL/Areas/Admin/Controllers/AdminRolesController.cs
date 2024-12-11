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
    public class AdminRolesController : Controller
    {
        private Eonon_ProEntities1 db = new Eonon_ProEntities1();

        // GET: Admin/AdminRoles
        public ActionResult Index()
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = db.Customers.FirstOrDefault(u => u.AccountID == accountId);

            if (user != null)
            {
                // Check if Avatar and FullName exist, if not, assign default values
                ViewBag.Avatar = string.IsNullOrEmpty(user.Avatar) ? "~/Content/images/avatar/defaultAvatar.jpg" : user.Avatar;
                ViewBag.FullName = string.IsNullOrEmpty(user.FullName) ? "No Name Available" : user.FullName;
            }
            else
            {
                // If user not found, assign default values for Avatar and FullName
                ViewBag.Avatar = "~/Content/images/avatar/defaultAvatar.jpg";
                ViewBag.FullName = "No Name Available";
            }
            return View(db.Roles.ToList());
        }

        // GET: Admin/AdminRoles/Details/5
        public ActionResult Details(int? id)
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = db.Customers.FirstOrDefault(u => u.AccountID == accountId);

            if (user != null)
            {
                // Check if Avatar and FullName exist, if not, assign default values
                ViewBag.Avatar = string.IsNullOrEmpty(user.Avatar) ? "~/Content/images/avatar/defaultAvatar.jpg" : user.Avatar;
                ViewBag.FullName = string.IsNullOrEmpty(user.FullName) ? "No Name Available" : user.FullName;
            }
            else
            {
                // If user not found, assign default values for Avatar and FullName
                ViewBag.Avatar = "~/Content/images/avatar/defaultAvatar.jpg";
                ViewBag.FullName = "No Name Available";
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // GET: Admin/AdminRoles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminRoles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RoleID,RoleName,Description")] Role role)
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = db.Customers.FirstOrDefault(u => u.AccountID == accountId);

            if (user != null)
            {
                // Check if Avatar and FullName exist, if not, assign default values
                ViewBag.Avatar = string.IsNullOrEmpty(user.Avatar) ? "~/Content/images/avatar/defaultAvatar.jpg" : user.Avatar;
                ViewBag.FullName = string.IsNullOrEmpty(user.FullName) ? "No Name Available" : user.FullName;
            }
            else
            {
                // If user not found, assign default values for Avatar and FullName
                ViewBag.Avatar = "~/Content/images/avatar/defaultAvatar.jpg";
                ViewBag.FullName = "No Name Available";
            }
            if (ModelState.IsValid)
            {
                db.Roles.Add(role);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(role);
        }

        // GET: Admin/AdminRoles/Edit/5
        public ActionResult Edit(int? id)
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = db.Customers.FirstOrDefault(u => u.AccountID == accountId);

            if (user != null)
            {
                // Check if Avatar and FullName exist, if not, assign default values
                ViewBag.Avatar = string.IsNullOrEmpty(user.Avatar) ? "~/Content/images/avatar/defaultAvatar.jpg" : user.Avatar;
                ViewBag.FullName = string.IsNullOrEmpty(user.FullName) ? "No Name Available" : user.FullName;
            }
            else
            {
                // If user not found, assign default values for Avatar and FullName
                ViewBag.Avatar = "~/Content/images/avatar/defaultAvatar.jpg";
                ViewBag.FullName = "No Name Available";
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Admin/AdminRoles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RoleID,RoleName,Description")] Role role)
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = db.Customers.FirstOrDefault(u => u.AccountID == accountId);

            if (user != null)
            {
                // Check if Avatar and FullName exist, if not, assign default values
                ViewBag.Avatar = string.IsNullOrEmpty(user.Avatar) ? "~/Content/images/avatar/defaultAvatar.jpg" : user.Avatar;
                ViewBag.FullName = string.IsNullOrEmpty(user.FullName) ? "No Name Available" : user.FullName;
            }
            else
            {
                // If user not found, assign default values for Avatar and FullName
                ViewBag.Avatar = "~/Content/images/avatar/defaultAvatar.jpg";
                ViewBag.FullName = "No Name Available";
            }
            if (ModelState.IsValid)
            {
                db.Entry(role).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(role);
        }

        // GET: Admin/AdminRoles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Admin/AdminRoles/Delete/5
        [HttpPost, ActionName("Delete")]
      
        public ActionResult DeleteConfirmed(int id)
        {

            var role = db.Roles.Find(id);

            if (role != null)
            {
                db.Roles.Remove(role);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
