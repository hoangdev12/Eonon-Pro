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


namespace WebBTL.Areas.Admin.Controllers
{
    public class AdminCustomersController : Controller
    {
        

        private readonly Eonon_ProEntities1 _context;

        public AdminCustomersController()
        {
            _context = new Eonon_ProEntities1();
        }

        // GET: Admin/AdminCustomers
        public ActionResult Index(int page = 1, int pageSize = 10)
        {

            var customers = _context.Customers.Include(c => c.Location).ToList();

            var pagedCustomers = customers.ToPagedList(page, pageSize);

            return View(pagedCustomers);
        }

        // GET: Admin/AdminCustomers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Admin/AdminCustomers/Create
        public ActionResult Create()
        {
            ViewBag.LocationID = new SelectList(_context.Locations, "LocationID", "Name");
            return View();
        }

        // POST: Admin/AdminCustomers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerID,FullName,Birthday,Avatar,Address,Email,Phone,LocationID,District,Ward,CreateDate,Password,Salt,LastLogin,Active")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Customers.Add(customer);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LocationID = new SelectList(_context.Locations, "LocationID", "Name", customer.LocationID);
            return View(customer);
        }

        // GET: Admin/AdminCustomers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.LocationID = new SelectList(_context.Locations, "LocationID", "Name", customer.LocationID);
            return View(customer);
        }

        // POST: Admin/AdminCustomers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,FullName,Birthday,Avatar,Address,Email,Phone,LocationID,District,Ward,CreateDate,Password,Salt,LastLogin,Active")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(customer).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LocationID = new SelectList(_context.Locations, "LocationID", "Name", customer.LocationID);
            return View(customer);
        }

        // GET: Admin/AdminCustomers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Admin/AdminCustomers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = _context.Customers.Find(id);
            _context.Customers.Remove(customer);
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
