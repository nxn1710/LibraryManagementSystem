using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using LibraryManagement.Commons;
using LibraryManagement.Models;
using System.Data.Entity;
using System.Linq.Dynamic;
using System.Data;
using ClosedXML.Excel;
using System.IO;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Staff
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder, string key)
        {
            ViewBag.title = "Staff Account";
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
            //get properties of staffaccount
            var properties = typeof(StaffAccount).GetProperties();
            List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();
            foreach (var item in properties) {
                var isVirtual = item.GetAccessors()[0].IsVirtual;
                //dont show virtual part, password and repassword in table
                if (isVirtual || item.Name == "Password" || item.Name == "RePassword") {
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
                        ViewBag.Headings += "<th><a href='/staff/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                       ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-desc'></i></th></a></th>";
                    } else if (sortOrder == "asc" && sortProperty == item.Item1) {
                        ViewBag.Headings += "<th><a href='/staff/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
                            ViewBag.sortOrder + "&key=" + key + "'>" + item.Item1 + "<i class='fa fa-fw fa-sort-asc'></a></th>";
                    } else {
                        ViewBag.Headings += "<th><a href='/staff/page/" + page + "?size=" + ViewBag.currentSize + "&sortProperty=" + item.Item1 + "&sortOrder=" +
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
            //get all staffs 
            var staffs = from s in _db.StaffAccounts
                          select s;
            //check staffs list is empty
            if (staffs.Count() == 0) {
                TempData["message"] = $"Not found anything in system!";
                TempData["error"] = true;
                return View(staffs.ToPagedList(pageNumber, pageSize));
            }

            //filter staff with key search
            if (!String.IsNullOrEmpty(key)) {
                staffs = staffs.Where(s => (s.ID + " " + s.Username + " " + s.FullName).Contains(key)).OrderBy(s => s.ID);
                ViewBag.searchValue = key;
            }

            //sort using dynamic linq
            if (sortOrder == "desc") {
                staffs = staffs.OrderBy(sortProperty + " desc");
            } else if (sortOrder == "asc") {
                staffs = staffs.OrderBy(sortProperty);
            } else {
                //default is sort by id
                staffs = staffs.OrderBy("id");
            }
            return View(staffs.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Add() {
            ViewBag.title = "Staff Account - Add";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(StaffAccount staff) {
            if (ModelState.IsValid) {
                //encrypt password
                staff.Password = Password.Encrypt(staff.Password);
                _db.StaffAccounts.Add(staff);
                _db.SaveChanges();
                TempData["message"] = $"Add Staff successfully!";
                return RedirectToAction("Index", new { key = staff.Username });
            }
            return View();
        }

        public ActionResult Edit(int? id) {
            var staff = (from a in _db.StaffAccounts where a.ID == id select a).SingleOrDefault();
            if (id == null || staff == null) {
                TempData["message"] = $"Update fail, Cannot found that Staff Account in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            staff.Password = null;
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StaffAccount staffAccount) {
            if (ModelState.IsValid || String.IsNullOrEmpty(staffAccount.Password)) {
                //dont update password
                if (String.IsNullOrEmpty(staffAccount.Password)) {
                    var oldPassword = (from a in _db.StaffAccounts where a.ID == staffAccount.ID select a.Password).FirstOrDefault();
                    staffAccount.Password = oldPassword;
                } else {
                    //update password
                    staffAccount.Password = Password.Encrypt(staffAccount.Password);
                }
                _db.Entry(staffAccount).State = EntityState.Modified;
                _db.SaveChanges();
                TempData["message"] = $"Update Staff Account successfully!";
                return RedirectToAction("Index", new { key = staffAccount.Username });
            }
            return View(staffAccount);
        }

        public ActionResult Delete(int? id) {
            var staffAccount = (from s in _db.StaffAccounts where s.ID == id select s).SingleOrDefault();
            if (id == null || staffAccount == null) {
                TempData["message"] = $"Delete fail, Cannot found that Staff account in system!";
                TempData["error"] = true;
                return RedirectToAction("Index");
            }
            _db.StaffAccounts.Remove(staffAccount);
            _db.SaveChanges();
            TempData["message"] = $"Delete Author {id} - {staffAccount.FullName} successfully!";
            return RedirectToAction("Index");
        }

        public FileResult Export() {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[3] { new DataColumn("ID"),
                                            new DataColumn("Username") , new DataColumn("Fullname")});
            var staffAccounts = from a in _db.StaffAccounts
                             select a;
            foreach (var staffAccount in staffAccounts) {
                dt.Rows.Add(staffAccount.ID, staffAccount.Username, staffAccount.FullName);
            }
            using (XLWorkbook wb = new XLWorkbook()) {
                wb.Worksheets.Add(dt);
                DateTime today = DateTime.Today;
                using (MemoryStream stream = new MemoryStream()) {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Staff {today.ToString("dd/MM/yyyy")}.xlsx");
                }
            }
        }
    }
}