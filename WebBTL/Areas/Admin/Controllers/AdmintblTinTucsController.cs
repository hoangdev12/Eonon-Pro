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
        // GET: Admin/AdminPages
        public ActionResult Index(string searchTerm, int? tintuc, int page = 1, int pageSize = 10, int? CategoryId = null, int? TrangThai = null)
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = _context.Customers.FirstOrDefault(u => u.AccountID == accountId);

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

            // Lấy danh sách bài viết từ database, sắp xếp theo PostID giảm dần
            var lsTintuc = _context.tblTinTucs
                                   .AsNoTracking()
                                   .OrderByDescending(x => x.PostID); // Sử dụng OrderByDescending mà không cần ép kiểu

            // Nếu có searchTerm, thực hiện tìm kiếm theo tiêu đề bài viết
            if (!string.IsNullOrEmpty(searchTerm))
            {
                lsTintuc = (IOrderedQueryable<tblTinTuc>)lsTintuc.Where(x => x.Title.Contains(searchTerm));
            }

            // Tạo phân trang
            var pagedProducts = lsTintuc.ToPagedList(page, pageSize);

            // Truyền thông tin tìm kiếm vào ViewBag để giữ giá trị khi chuyển trang
            ViewBag.SearchTerm = searchTerm;
            

            return View(pagedProducts); // Trả về view với dữ liệu phân trang
        }

        // GET: Admin/AdmintblTinTucs/Details/5
        public ActionResult Details(int? id)
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = _context.Customers.FirstOrDefault(u => u.AccountID == accountId);

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
            tblTinTuc tblTinTuc = _context.tblTinTucs.Find(id);
            if (tblTinTuc == null)
            {
                return HttpNotFound();
            }
            return View(tblTinTuc);
        }

        // GET: Admin/AdmintblTinTucs/Create
        [HttpGet]
        public ActionResult Create()
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = _context.Customers.FirstOrDefault(u => u.AccountID == accountId);

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
            var model = new tblTinTuc
            {
                Contents = string.Empty,  // Giá trị mặc định
                SContents = string.Empty // Giá trị mặc định
            };

            return View(model);
        }


        // POST: Admin/AdmintblTinTucs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PostID,Title,SContents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountID,Tags,CatID,isHot,isNewFeed,MetaKey,MetaDesc,Views")] tblTinTuc tblTinTuc, HttpPostedFileBase image)
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = _context.Customers.FirstOrDefault(u => u.AccountID == accountId);

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
            // Kiểm tra nếu có ảnh mới được tải lên
            if (image != null && image.ContentLength > 0)
            {
                // Lấy tên file và đường dẫn lưu ảnh
                string fileName = System.IO.Path.GetFileName(image.FileName);
                string filePath = Server.MapPath("~/Content/images/tintucs/" + fileName);

                // Đảm bảo thư mục lưu ảnh tồn tại
                string directoryPath = Server.MapPath("~/Content/images/tintucs/");
                if (!System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                // Cố gắng lưu ảnh vào thư mục
                try
                {
                    image.SaveAs(filePath);
                    // Cập nhật đường dẫn ảnh trong sản phẩm
                    tblTinTuc.Thumb = Url.Content("~/Content/images/tintucs/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Không thể lưu ảnh. Vui lòng thử lại. " + ex.Message);
                   
                    return View(tblTinTuc);
                }
            }
            else
            {
                // Nếu không có ảnh được tải lên, sử dụng ảnh mặc định
                tblTinTuc.Thumb = Url.Content("~/Content/images/default.png");
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
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = _context.Customers.FirstOrDefault(u => u.AccountID == accountId);

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
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "PostID,Title,SContents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountID,Tags,CatID,isHot,isNewFeed,MetaKey,MetaDesc,Views")] tblTinTuc tblTinTuc, HttpPostedFileBase image)
        {
            int accountId = (int)Session["AccountId"]; // Get AccountId from session
            var user = _context.Customers.FirstOrDefault(u => u.AccountID == accountId);

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
            var existingtblTinTuc = _context.tblTinTucs.Find(tblTinTuc.PostID);

            if (existingtblTinTuc == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra nếu có ảnh mới được tải lên
            if (image != null && image.ContentLength > 0)
            {
                // Lấy tên file và đường dẫn lưu ảnh
                string fileName = System.IO.Path.GetFileName(image.FileName);
                string filePath = Server.MapPath("~/Content/images/tintucs/" + fileName);

                // Đảm bảo thư mục lưu ảnh tồn tại
                string directoryPath = Server.MapPath("~/Content/images/tintucs/");
                if (!System.IO.Directory.Exists(directoryPath))
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                }

                // Cố gắng lưu ảnh vào thư mục
                try
                {
                    image.SaveAs(filePath);
                    // Cập nhật đường dẫn ảnh trong sản phẩm
                    tblTinTuc.Thumb = Url.Content("~/Content/images/tintucs/" + fileName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Không thể lưu ảnh. Vui lòng thử lại. " + ex.Message);

                    return View(tblTinTuc);
                }
            }
            else
            {
                // Nếu không có ảnh được tải lên, sử dụng ảnh mặc định
                tblTinTuc.Thumb = Url.Content("~/Content/images/default.png");
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
                return RedirectToAction("Index","AdmintblTintucs");
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
