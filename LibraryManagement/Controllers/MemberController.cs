using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryManagement.Models;
namespace LibraryManagement.Controllers
{
    public class MemberController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Member
        [HttpGet]
        public ActionResult ShowAll(string fullname)
        {
            var members = _db.Members.Where(m => m.fullname.StartsWith(fullname) || fullname == null).ToList();
            ViewBag.members = members;
            //var members = (from s in _db.Members select s).ToList();
            //ViewBag.members = members;

            return View(members);
        }

        [HttpGet]
        public ActionResult Store()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Store(Member mb)
        {
            if (ModelState.IsValid) {
                _db.Members.Add(mb);
                _db.SaveChanges();
                return RedirectToAction("ShowAll");
            }
            ViewBag.message = "Insert member failed!!";
            return View();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var memberByID = _db.Members.Where(m => m.id == id).FirstOrDefault();
            return View(memberByID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,fullname,phonenumber,address,Update_at")] Member mb)
        {
            if (ModelState.IsValid)
            {
                var memberByID = _db.Members.Find(mb.id);
                memberByID.fullname = mb.fullname;
                memberByID.phonenumber = mb.phonenumber;
                memberByID.address = mb.address;
                _db.Entry(memberByID).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("ShowAll");
            }
            var memberEdit = _db.Members.Where(m => m.id == mb.id).FirstOrDefault();
            return View(memberEdit);
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            var member = _db.Members.Where(m => m.id == id).First();
            _db.Members.Remove(member);
            _db.SaveChanges();
            return RedirectToAction("ShowAll");
        }

    }
}