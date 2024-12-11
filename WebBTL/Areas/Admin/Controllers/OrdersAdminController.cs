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
using ClosedXML.Excel;
using System.IO;

namespace WebBTL.Areas.Admin.Controllers
{
    public class OrdersAdminController : Controller
    {
        private Eonon_ProEntities1 db = new Eonon_ProEntities1();

        // GET: Admin/OrdersAdmin
        public ActionResult Index(int? page)
        {

            var orders = db.Orders.Include(o => o.Customer).Include(o => o.TransactStatu).OrderByDescending(o => o.OrderID);

            // Set the page number to the requested page or default to 1 if no page is specified
            int pageNumber = page ?? 1;
            int pageSize = 10;

            // Paginate the list of orders
            var pagedOrders = orders.ToPagedList(pageNumber, pageSize);

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

            return View(pagedOrders);
        }


        // GET: Admin/OrdersAdmin/Details/5
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

            // Sử dụng đúng id truyền vào
            var order = db.Orders
                          .Include(o => o.OrderDetails.Select(od => od.Product)) // Load OrderDetails và Product liên quan
                          .FirstOrDefault(o => o.OrderID == id); // Sửa từ orderId thành id

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Admin/OrdersAdmin/Edit/5
        [HttpGet]
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
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            // Đổ dữ liệu cho DropdownList CustomerID
            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "FullName", order.CustomerID);
            ViewBag.TransactStatusID = new SelectList(db.TransactStatus, "TransactStatusID", "Status", order.TransactStatusID);
            return View(order);
        }


        // POST: Admin/OrdersAdmin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,CustomerID,Orderdate,ShipDate,TransactStatusID,Deleted,Paid,PaymentDate,PaymentID,Note,TotalAmount")] Order order)
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
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Đổ lại dữ liệu cho DropdownList CustomerID và TransactStatusID
            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "FullName", order.CustomerID);
            ViewBag.TransactStatusID = new SelectList(db.TransactStatus, "TransactStatusID", "Status", order.TransactStatusID);

            return View(order);
        }



        public ActionResult RevenueChart()
        {
            int accountId = (int)Session["AccountId"]; 
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

            // Get the orders with Orderdate and TotalAmount
            var orders = db.Orders
                .Where(o => o.Orderdate.HasValue) 
                .Select(o => new
                {
                    Year = o.Orderdate.Value.Year,
                    Month = o.Orderdate.Value.Month,
                    TotalAmount = o.TotalAmount
                })
                .ToList(); 

            // Group the data by Year and Month
            var revenueData = orders
                .GroupBy(o => new { o.Year, o.Month })
                .Select(g => new
                {
                    YearMonth = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalRevenue = g.Sum(o => o.TotalAmount) 
                })
                .OrderBy(o => o.YearMonth) // Order by Year and Month
                .ToList();

            // Prepare data for the chart
            ViewBag.Months = revenueData.Select(r => r.YearMonth.ToString("MMM yyyy")).ToList();
            ViewBag.Revenue = revenueData.Select(r => r.TotalRevenue).ToList();

            return View();
        }

        public ActionResult ExportToExcel()
        {
            var orders = db.Orders
                .Where(o => o.Orderdate.HasValue)
                .Select(o => new
                {
                    Year = o.Orderdate.Value.Year,
                    Month = o.Orderdate.Value.Month,
                    TotalAmount = o.TotalAmount
                })
                .ToList();

            var revenueData = orders
                .GroupBy(o => new { o.Year, o.Month })
                .Select(g => new
                {
                    YearMonth = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(o => o.YearMonth)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Doanh thu theo tháng");
                worksheet.Cell(1, 1).Value = "Month";
                worksheet.Cell(1, 2).Value = "Total Revenue";

                for (int i = 0; i < revenueData.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = revenueData[i].YearMonth.ToString("MMM yyyy");
                    worksheet.Cell(i + 2, 2).Value = revenueData[i].TotalRevenue;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RevenueReport.xlsx");
                }
            }
        }
    }
}
