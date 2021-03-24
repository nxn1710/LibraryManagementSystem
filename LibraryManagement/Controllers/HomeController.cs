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
            return View();
        }
    }
}