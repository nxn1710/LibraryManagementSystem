using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryManagement.Models;
using PagedList;

namespace LibraryManagement.Controllers
{
    public class AuthorsController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Authors
        public ActionResult Index(int ? page, string searchString)
        {
            ViewBag.title = "Library Management - Authors";
            if (TempData["message"] != null) {
                ViewBag.message = TempData["message"].ToString();
            }
            if (page == null) page = 1;
            var authors = (from a in _db.Authors
                         select a).OrderBy(a => a.id);
            if (!String.IsNullOrEmpty(searchString)) {
                 authors = authors.Where(a => a.author_name.Contains(searchString)).OrderBy(a => a.id);
                ViewBag.searchValue = searchString;
            }
            int pageSize = 6;
            int pageNumber = (page ?? 1);
            ViewBag.authors = authors;
            return View(authors.ToPagedList(pageNumber,pageSize));
        }

        public ActionResult Add() {
            return View();
        }

        public ActionResult Delete(int authorId) {
            var author = (from a in _db.Authors where a.id == authorId select a).FirstOrDefault();
            _db.Authors.Remove(author);
            _db.SaveChanges();
            TempData["message"] = $"Delete Author {authorId} - {author.author_name} successfully";
            return RedirectToAction("Index");
        }
    }
}