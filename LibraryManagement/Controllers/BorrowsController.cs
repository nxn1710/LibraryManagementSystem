using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LibraryManagement.Models;

namespace LibraryManagement.Controllers {
    [Authorize]
    public class BorrowsController : Controller {
        private LibraryEntities _db = new LibraryEntities();
        // GET: Borrows
        public ActionResult Index() {

            return View();
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
            Borrowed borrowed = new Borrowed { MemberID = memberId, StaffID = 9, BorrowedTime = DateTime.Now, ReturnDeadline = returnDate, TotalPrice = 0, Return = false };
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