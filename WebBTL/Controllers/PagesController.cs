using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBTL.Models;

namespace WebBTL.Controllers
{
    public class PagesController : Controller
    {
        private readonly Eonon_ProEntities1 _context;

        public PagesController()
        {
            _context = new Eonon_ProEntities1();

        }

        // GET: Pages
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About(int id = 5)
        {
            var about = _context.Pages.FirstOrDefault(x => x.PageID == id);

            if (about == null)
            {
                return RedirectToAction("Index");
            }
           

            // Lấy ra các bài viết liên quan có cùng tag, ngoại trừ bài viết hiện tại
            var lsBaiVietLienQuan = _context.tblTinTucs
                .AsNoTracking()
                .Where(x => x.Published == true
                            && x.PostID != id
                            ) // Kiểm tra bài viết có chứa bất kỳ tag nào trong bài viết hiện tại
                .OrderByDescending(x => x.CreateDate) // Sắp xếp theo ngày tạo
                .Take(3) // Lấy 3 bài viết
                .ToList();
            ViewBag.lsBaiVietLienQuan = lsBaiVietLienQuan;

            return View(about);
            
        }
    }
}