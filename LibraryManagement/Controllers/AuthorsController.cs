using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using LibraryManagement.Models;
using PagedList;

namespace LibraryManagement.Controllers {
    public class AuthorsController : Controller {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Authors
        public ActionResult Index(int? page, string key, int? size) {
            ViewBag.title = "Authors";
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
            ViewBag.currentSize = size;
            ViewBag.size = items;
            if (page == null) page = 1;
            int pageNumber = (page ?? 1);
            int pageSize = (size ?? 5);
            var authors = (from a in _db.Authors
                           select a).OrderBy(a => a.id);
            if (!String.IsNullOrEmpty(key)) {
                authors = authors.Where(a => (a.id + " " + a.author_name).Contains(key)).OrderBy(a => a.id);
                ViewBag.searchValue = key;
            }
            if (authors.Count() == 0) {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
            }             
            return View(authors.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Add() {
            ViewBag.title = "Authors - Add";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Author author) {
            if (ModelState.IsValid) {
                _db.Authors.Add(author);
                _db.SaveChanges();
                TempData["message"] = $"Add author successfully!";
                return RedirectToAction("Index", new { key = author.id + " " + author.author_name });
            }
            return View();
        }

        public ActionResult Edit(int? id) {
            var author = (from a in _db.Authors where a.id == id select a).SingleOrDefault();
            if (id == null || author == null) {
                TempData["message"] = $"Update fail, Cannot found that Author in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            return View(author);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Author author) {
            if (ModelState.IsValid) {
                _db.Entry(author).State = EntityState.Modified;
                _db.SaveChanges();
                TempData["message"] = $"Update author successfully!";
                return RedirectToAction("Index", new { key = author.id + " " + author.author_name });
            }
               return View(author);
        }

        public ActionResult Delete(int? id) {
            var author = (from a in _db.Authors where a.id == id select a).SingleOrDefault();
            if (id == null || author == null) {
                TempData["message"] = $"Delete fail, Cannot found that Author in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            _db.Authors.Remove(author);
            _db.SaveChanges();
            TempData["message"] = $"Delete Author {id} - {author.author_name} successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public FileResult Export() {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[2] { new DataColumn("Author ID"),
                                            new DataColumn("Author Name") });
            var authors = from a in _db.Authors
                            select a;
            foreach (var author in authors) {
                dt.Rows.Add(author.id, author.author_name);
            }
            using (XLWorkbook wb = new XLWorkbook()) {
                wb.Worksheets.Add(dt);
                DateTime today = DateTime.Today;
                using (MemoryStream stream = new MemoryStream()) {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Author {today.ToString("dd/MM/yyyy")}.xlsx");
                }
            }
        }
    }
}