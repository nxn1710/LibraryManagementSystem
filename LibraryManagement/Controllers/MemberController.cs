using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryManagement.Models;
using PagedList;

namespace LibraryManagement.Controllers
{
    public class MemberController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Member
        [HttpGet]

        public ActionResult Index(int? page, string key, int? size)
        {
            ViewBag.title = "Members";
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "5", Value = "5" });
            items.Add(new SelectListItem { Text = "10", Value = "10" });
            items.Add(new SelectListItem { Text = "20", Value = "20" });
            items.Add(new SelectListItem { Text = "25", Value = "25" });
            items.Add(new SelectListItem { Text = "50", Value = "50" });
            items.Add(new SelectListItem { Text = "100", Value = "100" });
            items.Add(new SelectListItem { Text = "200", Value = "200" });
            foreach (var item in items)
            {
                if (item.Value == size.ToString()) item.Selected = true;
            }
            ViewBag.currentSize = size;
            ViewBag.size = items;
            if (page == null) page = 1;
            int pageNumber = (page ?? 1);
            int pageSize = (size ?? 5);
            var members = (from m in _db.Members
                           select m).OrderBy(m => m.id);
            if (!String.IsNullOrEmpty(key))
            {
                members = members.Where(a => (a.id + " " + a.fullname).Contains(key)).OrderBy(a => a.id);
                ViewBag.searchValue = key;
            }
            if (members.Count() == 0)
            {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
            }
            return View(members.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Add()
        {
            ViewBag.title = "Members - Add";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Member member)
        {
            if (ModelState.IsValid)
            {
                _db.Members.Add(member);
                _db.SaveChanges();
                TempData["message"] = $"Add author successfully!";
                return RedirectToAction("Index", new { key = member.id + " " + member.fullname });
            }
            return View();
        }


        public ActionResult Edit(int? id)
        {
            var member = (from a in _db.Members where a.id == id select a).SingleOrDefault();
            if (id == null || member == null)
            {
                TempData["message"] = $"Update fail, Cannot found that Author in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            return View(member);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Member member)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(member).State = EntityState.Modified;
                _db.SaveChanges();
                TempData["message"] = $"Update author successfully!";
                return RedirectToAction("Index", new { key = member.id + " " + member.fullname });
            }
            return View(member);
        }

        public ActionResult Delete(int? id)
        {
            var member = (from a in _db.Members where a.id == id select a).SingleOrDefault();
            if (id == null || member == null)
            {
                TempData["message"] = $"Delete fail, Cannot found that Author in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            _db.Members.Remove(member);
            _db.SaveChanges();
            TempData["message"] = $"Delete Author {id} - {member.fullname} successfully!";
            return RedirectToAction("Index");
        }

        //public ActionResult ShowAll(string fullname)
        //{
        //    var members = _db.Members.Where(m => m.fullname.StartsWith(fullname) || fullname == null).ToList();
        //    ViewBag.members = members;
        //    //var members = (from s in _db.Members select s).ToList();
        //    //ViewBag.members = members;

        //    return View(members);
        //}

        //[HttpGet]
        //public ActionResult Store()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Store(Member mb)
        //{
        //    if (ModelState.IsValid) {
        //        _db.Members.Add(mb);
        //        _db.SaveChanges();
        //        return RedirectToAction("ShowAll");
        //    }
        //    ViewBag.message = "Insert member failed!!";
        //    return View();
        //}

        //[HttpGet]
        //public ActionResult Edit(int id)
        //{
        //    var memberByID = _db.Members.Where(m => m.id == id).FirstOrDefault();
        //    return View(memberByID);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "id,fullname,phonenumber,address,Update_at")] Member mb)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var memberByID = _db.Members.Find(mb.id);
        //        memberByID.fullname = mb.fullname;
        //        memberByID.phonenumber = mb.phonenumber;
        //        memberByID.address = mb.address;
        //        _db.Entry(memberByID).State = EntityState.Modified;
        //        _db.SaveChanges();
        //        return RedirectToAction("ShowAll");
        //    }
        //    var memberEdit = _db.Members.Where(m => m.id == mb.id).FirstOrDefault();
        //    return View(memberEdit);
        //}

        //[HttpGet]
        //public ActionResult Delete(int? id)
        //{
        //    var member = _db.Members.Where(m => m.id == id).First();
        //    _db.Members.Remove(member);
        //    _db.SaveChanges();
        //    return RedirectToAction("ShowAll");
        //}

    }
}