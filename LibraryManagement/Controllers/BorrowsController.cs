using LibraryManagement.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using System.Data;
using ClosedXML.Excel;
using System.IO;
using System.Data.Entity;

namespace LibraryManagement.Controllers
{
    public class BorrowsController : Controller
    {
        LibraryEntities _db = new LibraryEntities();
        // GET: Borrows
        [HttpGet]
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder, string key)
        {
            ViewBag.title = "Borrows";
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
            var properties = typeof(Borrowed).GetProperties();
            List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();
            foreach (var item in properties)
            {
                var isVirtual = item.GetAccessors()[0].IsVirtual;
                if (item.Name == "MemberID") { isVirtual = true; }
                if (item.Name == "StaffID") { isVirtual = true; }
                if (item.Name == "TotalPrice") { isVirtual = true; }
                if (item.Name == "BorrowedTime") { isVirtual = true; }
                if (item.Name == "ReturnDeadline") { isVirtual = true; }
                if (item.Name == "ReturnTime") { isVirtual = true; }
                if (item.Name == "Return") { isVirtual = true; }
                if (item.Name == "Member") { continue; }
                if (item.Name == "StaffAccount") { continue; }
                if (item.Name == "BorrowedDetails") { continue; }
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
                        ViewBag.Headings += "<th><a href='/borrow/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                       ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-desc'></i></th></a></th>";
                    }
                    else if (sortOrder == "asc" && sortProperty == item.Item1)
                    {
                        ViewBag.Headings += "<th><a href='/borrow/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                            ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-asc'></a></th>";
                    }
                    else
                    {
                        ViewBag.Headings += "<th><a href='/borrow/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
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
            var borrows = from b in _db.Borroweds
                          select b;
            //check authors list is empty
            if (borrows.Count() == 0)
            {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
                return View(borrows.ToPagedList(pageNumber, pageSize));
            }

            //filter author with key search
            if (!String.IsNullOrEmpty(key))
            {
                borrows = borrows.Where(a => (a.ID + " ").Contains(key)).OrderBy(a => a.ID);
                ViewBag.searchValue = key;
            }

            //sort using dynamic linq
            if (sortOrder == "desc")
            {
                borrows = borrows.OrderBy(sortProperty + " desc");
            }
            else if (sortOrder == "asc")
            {
                borrows = borrows.OrderBy(sortProperty);
            }
            else
            {
                //default is sort by id
                borrows = borrows.OrderBy("id");
            }
            return View(borrows.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Confirm(int? id)
        {
            var borrow = (from br in _db.Borroweds where br.ID == id select br).SingleOrDefault();
            if (id == null || borrow == null)
            {
                TempData["message"] = $"Confirm fail, Cannot found that Borrow in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            borrow.Return = true;
            _db.Entry(borrow).State = EntityState.Modified;
            _db.SaveChanges();
            TempData["message"] = $"Confirm Borrow {id} - {borrow.ID} successfully!";
            return RedirectToAction("Index");
        }

        public JsonResult viewDetail(int bID)
        {
            var detail = _db.BorrowedDetails.Where(br => br.BorrowID == bID).Select(br => new {
                br.Book.ID,
               br.Book.Title,
                br.Book.Thumbnail,
                br.Book.Price,
            }).ToList();
            return Json(detail, JsonRequestBehavior.AllowGet);
        }
        public FileResult Export()
        {
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
            foreach (var borrow in borrows)
            {
                dt.Rows.Add(borrow.ID, borrow.MemberID, borrow.StaffID, borrow.TotalPrice, borrow.BorrowedTime, borrow.ReturnDeadline, borrow.ReturnTime, borrow.Return);

            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                DateTime today = DateTime.Today;
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Borrows {today.ToString("dd/MM/yyyy")}.xlsx");
                }
            }
        }
    }
}