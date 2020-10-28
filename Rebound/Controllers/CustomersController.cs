using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Rebound.Models;

namespace Rebound.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Customers
        [AccessAuthorize(Roles = "Customer View")]
        public ActionResult Index()
        {
            return View(db.Customer.ToList());
        }
        public JsonResult GetCustomer()
        {
            var result = db.Customer.Select(s => new
            {
                s.Address,
                s.Code,
                FullName = s.FirstName + " " + s.LastName,
                s.Mobile,
                s.PaymentType,
                s.Id,
                s.Balance,
                s.Blacklist,

            }).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Customers/Create
        [AccessAuthorize(Roles = "Customer Create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Customer customer)
        {
            if (ModelState.IsValid)
            {
                var rowcount = db.Customer.Count();
                if (rowcount > 0)
                {
                    var cuscode = db.Customer.Max(x => x.Code);
                    customer.Code = cuscode + 1;
                }
                else
                {
                    customer.Code = 1000;
                }
                customer.CreatedOn = DateTime.Now.ToUniversalTime().AddHours(2);
                db.Customer.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(customer);
        }
        [AccessAuthorize(Roles = "Transaction Create")]
        public ActionResult GetMonthlyBills()
        {
            return View();
        }

        [AccessAuthorize(Roles = "Transaction Create")]
        public ActionResult GetBills()
        {
            ViewBag.Client = new SelectList(db.Customer, "Id", "FullName");
            return View();
        }
        [HttpPost]
        public async Task< ActionResult> GetBills(Transaction t)
        {
            if (ModelState.IsValid)
            {


                using (var savedata = db.Database.BeginTransaction())
                {
                    try
                    {
                        var username = User.Identity.GetUserName();

                        t.Id = Guid.NewGuid();
                        t.CreatedOn = DateTime.Now.ToUniversalTime().AddHours(2);
                        t.UserId = username;
                        var count = db.Transaction.Count();
                        if (count > 0)
                        {
                            var cuscode = db.Transaction.Max(x => x.VoucherNo);
                            t.VoucherNo = cuscode + 1;
                        }
                        else
                        {
                            t.VoucherNo = 1000;
                        }
                        t.VoucherType = "Receive";
                        t.TrasactionalAmount = Convert.ToDecimal(t.DebitAmount) + Convert.ToDecimal(t.CreditAmount);
                        if (t.DebitAmount == null) { t.DebitAmount = 0; }
                        if (t.CreditAmount == null) { t.CreditAmount = 0; }
                        t.TransactionDate = DateTime.Now.ToUniversalTime().AddHours(2);
                        db.Transaction.Add(t);

                        var cBalance = db.Customer.Where(x => x.Id == t.Client).Select(s => s.Balance).FirstOrDefault();

                        Customer c = new Customer();
                        c = db.Customer.FirstOrDefault(f => f.Id == t.Client);
                        c.Balance = Convert.ToDecimal(cBalance) + Convert.ToDecimal(t.DebitAmount) - (t.CreditAmount);
                        db.Entry(c).State = EntityState.Modified;

                        db.SaveChanges();

                        Cashbook tc = new Cashbook();

                        tc.Id = Guid.NewGuid();
                        tc.CreatedOn = DateTime.Now.ToUniversalTime().AddHours(2);
                        tc.UserId = username;
                        var co = db.Cashbook.Count();
                        if (co > 0)
                        {
                            var cuscode = db.Cashbook.Max(x => x.VoucherNo);
                            tc.VoucherNo = cuscode + 1;
                        }
                        else
                        {
                            tc.VoucherNo = 1000;
                        }

                        tc.Client = t.Client;
                        tc.DebitAmount = t.DebitAmount;
                        tc.CreditAmount = t.CreditAmount;
                        tc.Narration = t.Narration;

                        tc.VoucherType = "Cash";
                        tc.TrasactionalAmount = Convert.ToDecimal(tc.DebitAmount) + Convert.ToDecimal(tc.CreditAmount);
                        if (tc.DebitAmount == null) { tc.DebitAmount = 0; }
                        if (tc.CreditAmount == null) { tc.CreditAmount = 0; }
                        tc.TransactionDate = DateTime.Now.ToUniversalTime().AddHours(2);
                        db.Cashbook.Add(tc);
                        db.SaveChanges();

                        await db.SaveChangesAsync();

                        savedata.Commit();

                        return RedirectToAction("GetBills");
                    }
                    catch (Exception)
                    {
                        savedata.Rollback();
                        return RedirectToAction("GetBills");

                    }

                }
                
            }

            ViewBag.Client = new SelectList(db.Customer, "Id", "FullName");
            return View(t);
        }
        // GET: Customers/Edit/5
        [AccessAuthorize(Roles = "Customer Update")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.Entry(customer).Property(a => a.Balance).IsModified = false;
                db.Entry(customer).Property(a => a.Status).IsModified = false;
                db.Entry(customer).Property(a => a.Code).IsModified = false;
                db.Entry(customer).Property(a => a.CodeNumber).IsModified = false;
                db.Entry(customer).Property(a => a.CodeNumber).IsModified = false;
                db.Entry(customer).Property(a => a.CreatedOn).IsModified = false;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        [AccessAuthorize(Roles = "Customer Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customer.Find(id);
            db.Customer.Remove(customer);
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
