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

namespace LibraryManagement.Controllers {
    [Authorize]
    public class CategoryController : Controller {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Category
        [HttpGet]

        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder, string key) {
            ViewBag.title = "Categories";
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
            var properties = typeof(BookCategory).GetProperties();
            List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();
            foreach (var item in properties) {
                var isVirtual = item.GetAccessors()[0].IsVirtual;
                if (item.Name == "Books") {
                    continue;
                }
                Tuple<string, bool> t = new Tuple<string, bool>(item.Name, isVirtual);
                list.Add(t);
            }
            //initial sort heading
            foreach (var item in list) {
                //create heading table with non virtual part
                if (!item.Item2) {
                    if (sortOrder == "desc" && sortProperty == item.Item1) {
                        ViewBag.Headings += "<th><a href='/category/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                       ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-desc'></i></th></a></th>";
                    } else if (sortOrder == "asc" && sortProperty == item.Item1) {
                        ViewBag.Headings += "<th><a href='/category/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                            ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-asc'></a></th>";
                    } else {
                        ViewBag.Headings += "<th><a href='/category/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
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
            //get all categories 
            var categories = from c in _db.BookCategories
                             select c;
            //check categories list is empty
            if (categories.Count() == 0) {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
                return View(categories.ToPagedList(pageNumber, pageSize));
            }

            //filter author with key search
            if (!String.IsNullOrEmpty(key)) {
                categories = categories.Where(c => (c.ID + " " + c.CategoryName).Contains(key)).OrderBy(c => c.ID);
                ViewBag.searchValue = key;
            }

            //sort using dynamic linq
            if (sortOrder == "desc") {
                categories = categories.OrderBy(sortProperty + " desc");
            } else if (sortOrder == "asc") {
                categories = categories.OrderBy(sortProperty);
            } else {
                //default is sort by id
                categories = categories.OrderBy("id");
            }
            return View(categories.ToPagedList(pageNumber, pageSize));
        }


        public ActionResult Add() {
            ViewBag.title = "Categories - Add";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(BookCategory category) {
            if (ModelState.IsValid) {
                _db.BookCategories.Add(category);
                _db.SaveChanges();
                TempData["message"] = $"Add author successfully!";
                return RedirectToAction("Index", new { key = category.ID + " " + category.CategoryName });
            }
            return View();
        }


        public ActionResult Edit(int? id) {
            var category = (from a in _db.BookCategories where a.ID == id select a).SingleOrDefault();
            if (id == null || category == null) {
                TempData["message"] = $"Update fail, Cannot found that Category in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BookCategory category) {
            if (ModelState.IsValid) {
                _db.Entry(category).State = EntityState.Modified;
                _db.SaveChanges();
                TempData["message"] = $"Update author successfully!";
                return RedirectToAction("Index", new { key = category.ID + " " + category.CategoryName });
            }
            return View(category);
        }

        public ActionResult Delete(int? id) {
            var category = (from a in _db.BookCategories where a.ID == id select a).SingleOrDefault();
            if (id == null || category == null) {
                TempData["message"] = $"Delete fail, Cannot found that Author in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            _db.BookCategories.Remove(category);
            _db.SaveChanges();
            TempData["message"] = $"Delete Author {id} - {category.CategoryName} successfully!";
            return RedirectToAction("Index");
        }

        public FileResult Export() {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[3] { new DataColumn("ID"),
                                            new DataColumn("Name") , new DataColumn("Description")});
            var categories = from a in _db.BookCategories
                             select a;
            foreach (var category in categories) {
                dt.Rows.Add(category.ID, category.CategoryName, category.Description);
            }
            using (XLWorkbook wb = new XLWorkbook()) {
                wb.Worksheets.Add(dt);
                DateTime today = DateTime.Today;
                using (MemoryStream stream = new MemoryStream()) {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Category {today.ToString("dd/MM/yyyy")}.xlsx");
                }
            }
        }




    }
}