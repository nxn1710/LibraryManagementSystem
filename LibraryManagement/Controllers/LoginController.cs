using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using LibraryManagement.Models;
using LibraryManagement.Commons;
namespace LibraryManagement.Controllers
{
    public class LoginController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string username, string password) {
            if (ModelState.IsValid) {
                password = Password.Encrypt(password);
                 var login = _db.StaffAccounts.Where(s => s.Username.Equals(username) && s.Password.Equals(password)).ToList();
                if (login.Count() > 0) {
                    Session["fullname"] = login.FirstOrDefault().FullName;
                    Session["username"] = login.FirstOrDefault().Username;
                    return RedirectToAction("Index", "Home", null);
                } else {
                    TempData["error"] = "Login failed";
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public ActionResult LogOut() {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}