using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryManagement.Models;
namespace LibraryManagement.Controllers
{
    public class LoginController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        public ActionResult Index()
        {
            ViewBag.title = "Library Management - Login to system";
            if (TempData["error"] != null) {
                ViewBag.error = TempData["error"].ToString();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string username, string password) {
            if (ModelState.IsValid) {
                var data = _db.StaffAccounts.Where(s => s.username.Equals(username) && s.password.Equals(password)).ToList();
                if (data.Count() > 0) {
                    Session["fullname"] = data.FirstOrDefault().fullname;
                    Session["username"] = data.FirstOrDefault().username;
                    return RedirectToAction("Index", "Home", new { area = "" });
                } else {
                    TempData["error"] = "Login failed";
                    return RedirectToAction("Index");
                }
            }
            return View();
        }
    }
}