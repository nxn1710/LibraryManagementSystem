using LibraryManagement.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data;
using ClosedXML.Excel;
using System.IO;

namespace LibraryManagement.Controllers {
    [Authorize]
    public class BorrowsController : Controller {
        LibraryEntities _db = new LibraryEntities();
        // GET: Borrows
        [HttpGet]
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder, string key, string overdue) {
            ViewBag.title = "Borrows";
            ViewBag.overdue = overdue;
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
            var properties = typeof(Borrowed).GetProperties();
            List<Tuple<string, bool, int>> list = new List<Tuple<string, bool, int>>();
            foreach (var item in properties) {
                var isVirtual = item.GetAccessors()[0].IsVirtual;
                int order = 999;
                if (item.Name == "MemberID") { continue; }
                if (item.Name == "StaffID") { continue; }
                if (item.Name == "ID") { order = 1; }
                if (item.Name == "TotalPrice") { isVirtual = true; order = 4; }
                if (item.Name == "BorrowedTime") { isVirtual = true; order = 5; }
                if (item.Name == "ReturnDeadline") { isVirtual = true; order = 6; }
                if (item.Name == "ReturnTime") { isVirtual = true; order = 7; }
                if (item.Name == "Return") { isVirtual = true; order = 8; }
                if (item.Name == "Member") { isVirtual = true; order = 2; }
                if (item.Name == "StaffAccount") { isVirtual = true; order = 3; }
                if (item.Name == "BorrowedDetails") { continue; }
                Tuple<string, bool, int> t = new Tuple<string, bool, int>(item.Name, isVirtual, order);
                list.Add(t);
            }

            list = list.OrderBy(i => i.Item3).ToList();

            //initial sort heading
            foreach (var item in list) {
                //create heading table with non virtual part
                if (!item.Item2) {
                    if (sortOrder == "desc" && sortProperty == item.Item1) {
                        ViewBag.Headings += "<th><a href='/borrow/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                       ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-desc'></i></th></a></th>";
                    } else if (sortOrder == "asc" && sortProperty == item.Item1) {
                        ViewBag.Headings += "<th><a href='/borrow/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                            ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-asc'></a></th>";
                    } else {
                        ViewBag.Headings += "<th><a href='/borrow/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
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
            //get all borrows 
            var borrows = (from b in _db.Borroweds
                              select b);
            if (overdue == "true") {
                borrows = borrows.Where(b => b.ReturnDeadline < DateTime.Now);
            }
            
            //check borrows list is empty
            if (borrows.Count() == 0) {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
                return View(borrows.ToPagedList(pageNumber, pageSize));
            }

            //filter borrows with key search
            if (!String.IsNullOrEmpty(key)) {
                borrows = borrows.Where(a => (a.ID + " ").Contains(key)).OrderBy(a => a.ID);
                ViewBag.searchValue = key;
            }

            //sort using dynamic linq
            if (sortOrder == "desc") {
                borrows = borrows.OrderBy(sortProperty + " desc");
            } else if (sortOrder == "asc") {
                borrows = borrows.OrderBy(sortProperty);
            } else {
                //default is sort by id
                borrows = borrows.OrderBy("id desc");
            }
            return View(borrows.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult Confirm(int? id) {
            var borrow = (from br in _db.Borroweds where br.ID == id select br).SingleOrDefault();
            if (id == null || borrow == null) {
                TempData["message"] = $"Confirm fail, Cannot found that Borrow in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            borrow.Return = true;
            borrow.ReturnTime = DateTime.Now;
            _db.Entry(borrow).State = EntityState.Modified;
            _db.SaveChanges();
            TempData["message"] = $"Confirm Borrow {id} - {borrow.ID} successfully!";
            return RedirectToAction("Index");
        }

        public JsonResult viewDetail(int bID) {
            var detail = _db.BorrowedDetails.Where(br => br.BorrowID == bID).Select(br => new {
                br.Book.ID,
                br.Book.Title,
                br.Book.Thumbnail,
                br.Book.Price,
            }).ToList();
            return Json(detail, JsonRequestBehavior.AllowGet);
        }
        public FileResult Export() {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[8] { new DataColumn("ID"),
                                                     new DataColumn("Member ID"),
            new DataColumn("StaffID"),
            new DataColumn("Total Price"),
             new DataColumn("Borrowed Time"),
            new DataColumn("Return Deadline"),
             new DataColumn("Return Time"),
            new DataColumn("Return"),
            });
            var borrows = from b in _db.Borroweds
                          select b;
            foreach (var borrow in borrows) {
                dt.Rows.Add(borrow.ID, borrow.MemberID, borrow.StaffID, borrow.TotalPrice, borrow.BorrowedTime, borrow.ReturnDeadline, borrow.ReturnTime, borrow.Return);

            }
            using (XLWorkbook wb = new XLWorkbook()) {
                wb.Worksheets.Add(dt);
                DateTime today = DateTime.Today;
                using (MemoryStream stream = new MemoryStream()) {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Borrows {today.ToString("dd/MM/yyyy")}.xlsx");
                }
            }
        }
        public ActionResult Add() {
            Session.Remove("bill");
            string currentDate = DateTime.Now.ToString("MM/dd/yyyy");
            ViewBag.currentDate = currentDate;
            return View();
        }

        [HttpPost]
        public JsonResult AddBookToBill(int id) {
            if (Session["bill"] == null) {
                List<int> listBookIDs = new List<int>();
                listBookIDs.Add(id);
                Session["bill"] = listBookIDs;
                return Json("OK");
            } else {
                List<int> listBookIDs = (List<int>)Session["bill"];
                if (!isExistInBill(listBookIDs, id)) {

                    listBookIDs.Add(id);
                    Session["bill"] = listBookIDs;
                    return Json("OK");
                }
            }
            return Json("Not OK");
        }

        [HttpPost]
        public ActionResult CreateBill(int memberId, DateTime returnDate) {
            int staffId;
            int.TryParse(Request.Cookies["ID"].Value, out staffId);
            Borrowed borrowed = new Borrowed { MemberID = memberId, StaffID = staffId, BorrowedTime = DateTime.Now, ReturnDeadline = returnDate, TotalPrice = 0, Return = false };
            int numberOfDays = (int)(returnDate - DateTime.Now).TotalDays;
            double totalPrice = 0;
            _db.Borroweds.Add(borrowed);
            List<int> listBookIDs = (List<int>)Session["bill"];
            for (int i = 0; i < listBookIDs.Count; i++) {
                BorrowedDetail borrowedDetail = new BorrowedDetail { BookID = listBookIDs[i], BorrowID = borrowed.ID };
                int bookId = listBookIDs[i];
                var price = (from b in _db.Books where b.ID == bookId select b.Price).SingleOrDefault();
                totalPrice += (price * numberOfDays);
                _db.BorrowedDetails.Add(borrowedDetail);
            }
            borrowed.TotalPrice = (float)totalPrice;
            _db.SaveChanges();
            return RedirectToAction("Add");
        }
        private bool isExistInBill(List<int> listBookID, int id) {
            for (int i = 0; i < listBookID.Count; i++) {
                if (listBookID[i] == id) {
                    return true;
                }
            }
            return false;
        }
    }
}