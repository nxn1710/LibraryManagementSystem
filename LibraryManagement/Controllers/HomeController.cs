using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryManagement.Models;
namespace LibraryManagement.Controllers {
    [Authorize]
    public class HomeController : Controller {
        private LibraryEntities _db = new LibraryEntities();
        public ActionResult Index() {
            ViewBag.Title = "Home Page";
            var countMember = _db.Members.Select(m => m).Count();
            ViewBag.CountMember = countMember;
            var countBook = _db.Books.Select(b => b).Count();
            ViewBag.CountBook = countBook;
            var countBorrow = _db.Borroweds.Select(br => br).Count();
            ViewBag.CountBorrow = countBorrow;
            var overdueBorrows = _db.Borroweds.Where(br => br.ReturnDeadline < DateTime.Now).Count();
            ViewBag.OverdueBorrows = overdueBorrows;
            return View();
        }
    }
}