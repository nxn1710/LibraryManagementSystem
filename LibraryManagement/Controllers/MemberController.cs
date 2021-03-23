using System;
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
            if (sortOrder == "asc") {
                ViewBag.sortOrder = "desc";
            } else if (sortOrder == "desc") {
                ViewBag.sortOrder = "";
            } else {
                ViewBag.sortOrder = "asc";
            }
            //default sort is sort id
            if (sortProperty == null) {
                sortProperty = "id";
                ViewBag.sortOrder = "";
            }
            ViewBag.sortProperty = sortProperty;
            ViewBag.currentSize = size;
            var properties = typeof(Member).GetProperties();
            List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();
            foreach (var item in properties) {
                var isVirtual = item.GetAccessors()[0].IsVirtual;
                Tuple<string, bool> t = new Tuple<string, bool>(item.Name, isVirtual);
                if (isVirtual) {
                    continue;
                }
                list.Add(t);
            }
            //initial sort heading
            foreach (var item in list) {
                //create heading table with non virtual part
                if (!item.Item2) {
                    if (sortOrder == "desc" && sortProperty == item.Item1) {
                        ViewBag.Headings += "<th><a href='/member/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                       ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-desc'></i></th></a></th>";
                    } else if (sortOrder == "asc" && sortProperty == item.Item1) {
                        ViewBag.Headings += "<th><a href='/member/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                            ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-asc'></a></th>";
                    } else {
                        ViewBag.Headings += "<th><a href='/member/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                           ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort'></a></th>";
                    }

                } else {
                    ViewBag.Headings += "<th>" + item.Item1 + "</th>";
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
            foreach (var item in items) {
                if (item.Value == size.ToString()) item.Selected = true;
            }
            ViewBag.size = items;
            int pageNumber = (page ?? 1);
            int pageSize = (size ?? 5);
            //get all authors 
            var members = from m in _db.Members
                          select m;
            //check members list is empty
            if (members.Count() == 0) {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
                return View(members.ToPagedList(pageNumber, pageSize));
            }

            //filter member with key search
            if (!String.IsNullOrEmpty(key)) {
                members = members.Where(a => (a.ID + " " + a.FullName).Contains(key)).OrderBy(a => a.ID);
                ViewBag.searchValue = key;
            }

            //sort using dynamic linq
            if (sortOrder == "desc") {
                members = members.OrderBy(sortProperty + " desc");
            } else if (sortOrder == "asc") {
                members = members.OrderBy(sortProperty);
            } else {
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
                return RedirectToAction("Index", new { key = member.ID + " " + member.FullName });
            }
            return View();
        }


        public ActionResult Edit(int? id) {
            var member = (from a in _db.Members where a.ID == id select a).SingleOrDefault();
            if (id == null || member == null) {
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
                return RedirectToAction("Index", new { key = member.ID + " " + member.FullName });
            }
            return View(member);
        }

        public ActionResult Delete(int? id)
        {
            var member = (from a in _db.Members where a.ID == id select a).SingleOrDefault();
            if (id == null || member == null)
            {                TempData["message"] = $"Delete fail, Cannot found that Member in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            _db.Members.Remove(member);
            _db.SaveChanges();
            TempData["message"] = $"Delete Author {id} - {member.FullName} successfully!";
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
            foreach (var member in members) {
                dt.Rows.Add(member.ID, member.FullName, member.PhoneNumber, member.Address);

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
    }
}