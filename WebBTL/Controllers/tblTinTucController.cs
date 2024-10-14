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

        private readonly Eonon_ProEntities1 _context;

        public tblTinTucController()
        {
            _context = new Eonon_ProEntities1();
            
        }
        // GET: tblTinTuc
        public ActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var lsPages = _context.tblTinTucs
                .AsNoTracking()
                .OrderBy(x => x.PostID);

            PagedList<tblTinTuc> models = new PagedList<tblTinTuc>(lsPages, pageNumber, pageSize);

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

            
            return View(tintuc);
        }
    }
}