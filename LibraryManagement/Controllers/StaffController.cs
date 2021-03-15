using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using LibraryManagement.Models;
namespace LibraryManagement.Controllers
{
    public class StaffController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Staff
        public ActionResult Index(int ? page, string key)
        {
            ViewBag.title = "Authors";
            if (page == null) page = 1;
            int pageNumber = (page ?? 1);
            int pageSize = 6;
            var authors = (from s in _db.StaffAccounts
                           select s).OrderBy(s => s.id);
            if (!String.IsNullOrEmpty(key)) {
                authors = authors.Where(s => (s.id + " " + s.username + " " + s.fullname).Contains(key)).OrderBy(s => s.id);
                ViewBag.searchValue = key;
            }
            if (authors.Count() == 0) {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
            }
            return View(authors.ToPagedList(pageNumber, pageSize));
        }
    }
}