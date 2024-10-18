using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using WebBTL.Models;


namespace WebBTL.Controllers
{
    public class tblTinTucController : Controller
    {

        private readonly Eonon_ProEntities _context;

        public tblTinTucController()
        {
            _context = new Eonon_ProEntities();
            
        }
        // GET: tblTinTuc
        public ActionResult Index(string tag, int? page)
        {
           
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var lsPages = _context.tblTinTucs
                .AsNoTracking()
                .OrderBy(x => x.PostID);
            if (!string.IsNullOrEmpty(tag))
            {
                lsPages = (IOrderedQueryable<tblTinTucs>)lsPages.Where(x => x.Tags.Contains(tag));
            }

            PagedList<tblTinTucs> models = new PagedList<tblTinTucs>(lsPages, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);
            
        }

        public ActionResult Details(int id)
        {


            var tintuc = _context.tblTinTucs.FirstOrDefault(x => x.PostID == id);

            if (tintuc == null)
            {
                return RedirectToAction("Index");
            }

            // Tách các tags của bài viết hiện tại thành mảng
            var tags = tintuc.Tags.Split(',');

            // Lấy ra các bài viết liên quan có cùng tag, ngoại trừ bài viết hiện tại
            var lsBaiVietLienQuan = _context.tblTinTucs
                .AsNoTracking()
                .Where(x => x.Published == true
                            && x.PostID != id
                            && tags.Any(tag => x.Tags.Contains(tag))) // Kiểm tra bài viết có chứa bất kỳ tag nào trong bài viết hiện tại
                .OrderByDescending(x => x.CreateDate) // Sắp xếp theo ngày tạo
                .Take(3) // Lấy 3 bài viết
                .ToList();
            ViewBag.lsBaiVietLienQuan = lsBaiVietLienQuan;
            return View(tintuc);
        }
    }
}