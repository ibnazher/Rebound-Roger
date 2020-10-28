using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Rebound.Models;

namespace Rebound.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Reservations
        [AccessAuthorize(Roles = "Add Reservation")]
        public ActionResult Index()
        {
            var reservation = db.Reservation.Include(r => r.Customer).Include(r => r.Item).Include(r => r.Operators);
            return View(reservation.ToList());
        }

        // GET: Reservations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservation.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // GET: Reservations/Create
        public ActionResult Create()
        {
            ViewBag.Client = new SelectList(db.Customer, "Id", "FullName");
            ViewBag.ItemId = new SelectList(db.Items, "Id", "Name");
            ViewBag.OperatorsId = new SelectList(db.Operators, "Id", "FullName");

            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.GetUserName();
                var customerCode = db.Customer.Where(a => a.Id == reservation.Client).Select(s => s.Code).FirstOrDefault();
                var customerName = db.Customer.Where(a => a.Id == reservation.Client).Select(s => s.FirstName).FirstOrDefault() + " "+ db.Customer.Where(a => a.Id == reservation.Client).Select(s => s.LastName).FirstOrDefault();
                var itemTitle = db.Items.Where(x => x.Id == reservation.ItemId).Select(z => z.Name).FirstOrDefault();
                CultureInfo provider = new CultureInfo("en-LB");
                reservation.StartedAt = Convert.ToDateTime(reservation.Fromdate);
                reservation.EndAt = Convert.ToDateTime(reservation.ToDate);

                reservation.Status = true;

                reservation.CreateDate = DateTime.Now.ToUniversalTime().AddHours(2);
                reservation.CreateUser = username;
                reservation.ScheduleTitle =  customerName + " - " + itemTitle;
                reservation.ScheduleNote = "Price: " + reservation.Price.ToString() + " Total : " + reservation.TotalPrice.ToString();
                if (reservation.BillingStatus.ToString() == "Confirmed") { reservation.Color = "green"; }
                if (reservation.BillingStatus.ToString() == "Pending" || reservation.BillingStatus.ToString() == null) { reservation.Color = "red"; }

                db.Reservation.Add(reservation);
                db.SaveChanges();

                Transaction t = new Transaction();

                t.Id = Guid.NewGuid();
                t.CreatedOn = DateTime.Now.ToUniversalTime().AddHours(2);
                t.UserId = username;
                var count = db.Transaction.Count();
                if (count > 0)
                {
                    var cuscode = db.Customer.Max(x => x.Code);
                    t.VoucherNo = cuscode + 1;
                }
                else
                {
                    t.VoucherNo = 1000;
                }
                t.VoucherType = "Reservation";
                t.TrasactionalAmount = reservation.Price + Convert.ToDecimal(reservation.ExtraitemPrice) + Convert.ToDecimal(reservation.PaidPrice);
                t.Narration = reservation.ScheduleTitle + " - " + reservation.StartedAt.ToString() + " to " + reservation.EndAt.ToString();
                t.DebitAmount = Convert.ToDecimal(reservation.PaidPrice);
                t.CreditAmount = reservation.Price + Convert.ToDecimal(reservation.ExtraitemPrice);
                t.TransactionDate = reservation.StartedAt;
                t.Client = reservation.Client;
                t.ReservationId = reservation.Id;
                db.Transaction.Add(t);
                db.SaveChanges();

                var cBalance = db.Customer.Where(x => x.Id == t.Client).Select(s => s.Balance).FirstOrDefault();

                Customer c = new Customer();
                c = db.Customer.FirstOrDefault(f => f.Id == t.Client);
                c.Balance = Convert.ToDecimal(cBalance) + Convert.ToDecimal(reservation.PaidPrice) - (reservation.Price + Convert.ToDecimal(reservation.ExtraitemPrice));
                db.Entry(c).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Client = new SelectList(db.Customer, "Id", "FirstName", reservation.Client);
            ViewBag.ItemId = new SelectList(db.Items, "Id", "Name", reservation.ItemId);
            ViewBag.OperatorsId = new SelectList(db.Operators, "Id", "FirstName", reservation.OperatorsId);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservation.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            ViewBag.Client = new SelectList(db.Customer, "Id", "FirstName", reservation.Client);
            ViewBag.ItemId = new SelectList(db.Items, "Id", "Name", reservation.ItemId);
            ViewBag.OperatorsId = new SelectList(db.Operators, "Id", "FirstName", reservation.OperatorsId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,StartedAt,EndAt,Note,CreateDate,ApproveDate,CancelDate,CreateUser,UpdateUser,ApproveUser,CancelUser,CancelNote,BillingStatus,Status,Price,PaidPrice,ExtraitemPrice,Client,ItemId,OperatorsId")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Client = new SelectList(db.Customer, "Id", "FirstName", reservation.Client);
            ViewBag.ItemId = new SelectList(db.Items, "Id", "Name", reservation.ItemId);
            ViewBag.OperatorsId = new SelectList(db.Operators, "Id", "FirstName", reservation.OperatorsId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservation.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reservation reservation = db.Reservation.Find(id);
            db.Reservation.Remove(reservation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
