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
using System.Data.Entity.Validation;

namespace LibraryManagement.Controllers
{
    public class BookController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Member
        [HttpGet]
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder, string key)
        {
            ViewBag.title = "Books";
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
            var properties = typeof(Book).GetProperties();
            List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();
            foreach (var item in properties)
            {
                var isVirtual = item.GetAccessors()[0].IsVirtual;
               
                if (item.Name == "BorrowedDetails") { continue; }
                if (item.Name == "Author") { continue; }
                if (item.Name == "BookCategory") { continue; }
                if (item.Name == "ImageFile") { continue; }
                if (item.Name == "AvailableAuthors") { continue; }
                if (item.Name == "AvailableCategories") { continue; }
                if (item.Name == "AuthorID") { isVirtual = true; }
                if (item.Name == "CategoryID") { isVirtual = true; }
                if (item.Name == "Thumbnail") { isVirtual = true; }
                Tuple<string, bool> t = new Tuple<string, bool>(item.Name, isVirtual);
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
                       ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-desc'></i></th></a></th>";
                    }
                    else if (sortOrder == "asc" && sortProperty == item.Item1)
                    {
                        ViewBag.Headings += "<th><a href='/book/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                            ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-asc'></a></th>";
                    }
                    else
                    {
                        ViewBag.Headings += "<th><a href='/book/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                           ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort'></a></th>";
                    }

                }
                else
                {
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
            foreach (var item in items)
            {
                if (item.Value == size.ToString()) item.Selected = true;
            }
            ViewBag.size = items;
            int pageNumber = (page ?? 1);
            int pageSize = (size ?? 5);
            //get all authors 
            var books = from b in _db.Books
                        select b;
            //check authors list is empty
            if (books.Count() == 0)
            {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
                return View(books.ToPagedList(pageNumber, pageSize));
            }

            //filter author with key search
            if (!String.IsNullOrEmpty(key))
            {
                books = books.Where(a => (a.ID + " ").Contains(key)).OrderBy(a => a.ID);
                ViewBag.searchValue = key;
            }

            //sort using dynamic linq
            if (sortOrder == "desc")
            {
                books = books.OrderBy(sortProperty + " desc");
            }
            else if (sortOrder == "asc")
            {
                books = books.OrderBy(sortProperty);
            }
            else
            {
                //default is sort by id
                books = books.OrderBy("id");
            }
            return View(books.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Add()
        {
            //var model = new Book { AvailableAuthors = getAuthors(), AvailableCategories = getCategories() };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Book book)
        {
            if (ModelState.IsValid)
            {
                book = uploadImage(book);
                _db.Books.Add(book);
                _db.SaveChanges();
                ModelState.Clear();
                TempData["message"] = $"Add book successfully!";
                return RedirectToAction("Index", new { key = book.ID + " " });
            }
            //book.AvailableAuthors = getAuthors();
            //book.AvailableCategories = getCategories();
            return View(book);
        }


        public ActionResult Edit(int? id)
        {
            var book = (from a in _db.Books where a.ID == id select a).SingleOrDefault();
            
            if (id == null || book == null)
            {
                TempData["message"] = $"Update fail, Cannot found that Books in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            //book.AvailableAuthors = getAuthors();
            //book.AvailableCategories = getCategories();
            return View(book);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Book book)
        {
            if (ModelState.IsValid)
            {
                if (book.ImageFile == null)
                {
                    var oldThumnail = ((from a in _db.Books where a.ID == book.ID select a).SingleOrDefault()).Thumbnail;
                    book.Thumbnail = oldThumnail;
                    book = uploadImage(book);
                }
                else
                {
                    book = uploadImage(book);
                    _db.Entry(book).State = EntityState.Modified;
                }
                _db.SaveChanges();
                TempData["message"] = $"Update books successfully!";
                return RedirectToAction("Index", new { key = book.ID + " " });
            }
            //book.AvailableAuthors = getAuthors();
            //book.AvailableCategories = getCategories();
            return View(book);
        }

        public ActionResult Delete(int? id)
        {
            var book = (from b in _db.Books where b.ID == id select b).SingleOrDefault();
            if (id == null || book == null)
            {
                TempData["message"] = $"Delete fail, Cannot found that Book in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            _db.Books.Remove(book);
            _db.SaveChanges();
            TempData["message"] = $"Delete Books {id} - {book.Title} successfully!";
            return RedirectToAction("Index");
        }

        public FileResult Export()
        {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[8] { new DataColumn("id"),
                                                     new DataColumn("title"),
            new DataColumn("thumbnail"),
            new DataColumn("price"),
            new DataColumn("available_book"),
            new DataColumn("description"),
            new DataColumn("author_id"),
            new DataColumn("category_id"),
            });
            var books = from b in _db.Books
                        select b;
            foreach (var book in books)
            {
                dt.Rows.Add(book.ID, book.Title, book.Thumbnail, book.Price, book.AvailableBook, book.Description, book.AuthorID, book.CategoryID);
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                DateTime today = DateTime.Today;
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Book {today.ToString("dd/MM/yyyy")}.xlsx");
                }
            }
        }

        public Book uploadImage(Book book)
        {
            try
            {
                if (Request.Files.Count > 0)
                {
                    if (book.ImageFile != null && book.ImageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(book.ImageFile.FileName);
                        string extension = Path.GetExtension(book.ImageFile.FileName);
                        fileName += extension;
                        book.Thumbnail = "/UploadedFiles/" + fileName;
                        fileName = Path.Combine(Server.MapPath("/UploadedFiles/"), fileName);
                        book.ImageFile.SaveAs(fileName);
                    }
                }
                return book;
            }
            catch (Exception ex)
            {
                ViewBag.message = ex.Message;
            }
            return null;
        }

        public IList<SelectListItem> getAuthors()
        {
            IList<SelectListItem> authors = _db.Authors.Select(a => new SelectListItem
            {
                Value = a.ID.ToString(),
                Text = a.AuthorName,
            }).ToList();
            return authors;
        }
        public IList<SelectListItem> getCategories()
        {
            IList<SelectListItem> categories = _db.BookCategories.Select(a => new SelectListItem
            {
                Value = a.ID.ToString(),
                Text = a.CategoryName,
            }).ToList();
            return categories;
        }
    }
}

        