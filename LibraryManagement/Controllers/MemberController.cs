﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using LibraryManagement.Models;
using PagedList;
using System.Linq.Dynamic;
namespace LibraryManagement.Controllers
{
    public class MemberController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Member
        [HttpGet]

        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder, string key)
        {
            ViewBag.title = "Members";
            if (page == null) page = 1;
            //add sortOrder to view bag
            if (sortOrder == "asc")
            {
                ViewBag.sortOrder = "desc";
            }
            else if (sortOrder == "desc")
            {
                ViewBag.sortOrder = "";
            }
            else
            {
                ViewBag.sortOrder = "asc";
            }
            //default sort is sort id
            if (sortProperty == null)
            {
                sortProperty = "id";
                ViewBag.sortOrder = "";
            }
            ViewBag.sortProperty = sortProperty;
            ViewBag.currentSize = size;
            var properties = typeof(Member).GetProperties();
            List<Tuple<string, bool, string>> list = new List<Tuple<string, bool, string>>();
            foreach (var item in properties)
            {
                var isVirtual = item.GetAccessors()[0].IsVirtual;
                string nameHeading = "";
                if (item.Name == "id")
                {
                    nameHeading = "Member ID";
                }
                if (item.Name == "fullname")
                {
                    nameHeading = "Full Name";
                }
                if (item.Name == "phonenumber")
                {
                    isVirtual = true;
                    nameHeading = "phonenumber";
                }
                if (item.Name == "address")
                {
                    nameHeading = "Address";
                }
                if (item.Name == "Borroweds") { continue; }
                Tuple<string, bool, string> t = new Tuple<string, bool, string>(item.Name, isVirtual, nameHeading);
                list.Add(t);
            }
            //initial sort heading
            foreach (var item in list)
            {
                //create heading table with non virtual part
                if (!item.Item2)
                {
                    if (sortOrder == "desc" && sortProperty == item.Item1)
                    {
                        ViewBag.Headings += "<th><a href='/book/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                       ViewBag.sortOrder + "&key=" + key + "'>" + item.Item3 + "<i class='fa fa-fw fa-sort-desc'></i></th></a></th>";
                    }
                    else if (sortOrder == "asc" && sortProperty == item.Item1)
                    {
                        ViewBag.Headings += "<th><a href='/book/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                            ViewBag.sortOrder + "&key=" + key + "'>" + item.Item3 + "<i class='fa fa-fw fa-sort-asc'></a></th>";
                    }
                    else
                    {
                        ViewBag.Headings += "<th><a href='/book/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                           ViewBag.sortOrder + "&key=" + key + "'>" + item.Item3 + "<i class='fa fa-fw fa-sort'></a></th>";
                    }

                }
                else
                {
                    ViewBag.Headings += "<th>" + item.Item3 + "</th>";
                }
            }
            //initial dropdown list size
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
            ViewBag.size = items;
            int pageNumber = (page ?? 1);
            int pageSize = (size ?? 5);
            //get all authors 
            var members = from m in _db.Members
                        select m;
            //check authors list is empty
            if (members.Count() == 0)
            {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
                return View(members.ToPagedList(pageNumber, pageSize));
            }

            //filter author with key search
            if (!String.IsNullOrEmpty(key))
            {
                members = members.Where(a => (a.id + " " + a.fullname).Contains(key)).OrderBy(a => a.id);
                ViewBag.searchValue = key;
            }

            //sort using dynamic linq
            if (sortOrder == "desc")
            {
                members = members.OrderBy(sortProperty + " desc");
            }
            else if (sortOrder == "asc")
            {
                members = members.OrderBy(sortProperty);
            }
            else
            {
                //default is sort by id
                members = members.OrderBy("id");
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
                TempData["message"] = $"Add member successfully!";
                return RedirectToAction("Index", new { key = member.id + " " + member.fullname });
            }
            return View();
        }


        public ActionResult Edit(int? id)
        {
            var member = (from a in _db.Members where a.id == id select a).SingleOrDefault();
            if (id == null || member == null)
            {
                TempData["message"] = $"Update fail, Cannot found that Members in system!";
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
                TempData["message"] = $"Update members successfully!";
                return RedirectToAction("Index", new { key = member.id + " " + member.fullname });
            }
            return View(member);
        }

        public ActionResult Delete(int? id)
        {
            var member = (from a in _db.Members where a.id == id select a).SingleOrDefault();
            if (id == null || member == null)
            {
                TempData["message"] = $"Delete fail, Cannot found that Member in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            _db.Members.Remove(member);
            _db.SaveChanges();
            TempData["message"] = $"Delete Author {id} - {member.fullname} successfully!";
            return RedirectToAction("Index");
        }

        public FileResult Export()
        {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[4] { new DataColumn("ID"),
                                                     new DataColumn("Full Name"),
            new DataColumn("Phone Number"),
            new DataColumn("Address"),
            });
            var members = from m in _db.Members
                          select m;
            foreach (var member in members)
            {
                dt.Rows.Add(member.id, member.fullname, member.phonenumber, member.address);
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                DateTime today = DateTime.Today;
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Member {today.ToString("dd/MM/yyyy")}.xlsx");
                }
            }
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