using Microsoft.AspNet.Identity;
using Rebound.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Rebound.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        [Route("Home")]
        [Route("Home/Index")]
        [AccessAuthorize(Roles = "Add Reservation")]
        public ActionResult Index()
        {
            var customer = db.Customer.Select(s => new
            {
                Text = s.FirstName + " " + s.LastName + "-" + s.Phone,
                value = s.Id,
                Group = s.Code
            });
            ViewBag.Client = new SelectList(customer, "value", "Text", "Group", null, null);
            ViewBag.ItemId = new SelectList(db.Items, "Id", "Name");
            ViewBag.OperatorsId = new SelectList(db.Operators, "Id", "FullName");
            return View();
        }
        [AccessAuthorize(Roles = "Dashboard")]
        public ActionResult Dashboard()
        {
            return View();
        }
        public JsonResult GetEvents(string start, string end, string qcustomer)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                object events;
                CultureInfo provider = new CultureInfo("en-LB");
                DateTime sDate = Convert.ToDateTime(start);
                DateTime eDate = Convert.ToDateTime(end);
                int customer;
                if (qcustomer != "")
                {
                    customer = db.Customer.Where(a => a.FirstName.ToLower().Contains(qcustomer) || a.LastName.ToLower().Contains(qcustomer) || a.Code.ToString().ToLower().Contains(qcustomer) || a.Mobile.ToLower().Contains(qcustomer) || a.Phone.ToLower().Contains(qcustomer)).Select(s => s.Id).FirstOrDefault();
                    events = db.Reservation.Where(x => x.Status == true && x.Client == customer && x.StartedAt >= sDate && x.EndAt <= eDate).Select(s => new
                    {
                        id = s.Id,
                        resourceId = s.ItemId,
                        start = s.StartedAt,
                        end = s.EndAt,
                        title = s.ScheduleTitle,
                        color = s.Color,
                        description = s.Note,
                        allDay = s.IsFullDay,
                        price = s.Price,
                        eprice = s.ExtraitemPrice
                    }).ToList();
                }
                else
                {
                    qcustomer = null;
                    events = db.Reservation.Where(x => x.Status == true && x.StartedAt >= sDate && x.EndAt <= eDate).Select(s => new
                    {
                        id = s.Id,
                        resourceId = s.ItemId,
                        start = s.StartedAt,
                        end = s.EndAt,
                        title = s.ScheduleTitle,
                        color = s.Color,
                        description = s.Note,
                        allDay = s.IsFullDay,
                        price = s.Price,
                        eprice = s.ExtraitemPrice
                    }).ToList();
                }

                var items = db.Items.Where(z => z.Published == true).Select(s => new
                {
                    id = s.Id,
                    name = s.Name
                }).ToList();
                return Json(new { Data = events, Item = items }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetReservationData(int id)
        {
            var result = db.Reservation.Where(a => a.Id == id).Select(s => new
            {
                s.Id,
                s.Client,
                s.Price,
                s.ExtraitemPrice,
                s.StartedAt,
                s.EndAt,
                s.ItemId,
                s.BillingStatus,
                s.PaidPrice,
                s.Color,
                s.Note,
                s.IsBallRent

            }).ToList();
            var cid = db.Reservation.Where(a => a.Id == id).Select(s => s.Client).FirstOrDefault();
            var getClient = db.Customer.Where(a => a.Id == cid).Select(s => new
            {
                s.FirstName,
                s.LastName,
                s.Mobile
            }).ToList();
            return Json(new { result, client = getClient }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPrice(int id)
        {
            var result = db.Items.Where(a => a.Id == id).Select(s => s.Price).FirstOrDefault();

            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<JsonResult> UpdateEvent(Reservation e)
        {
            var status = false;
            using (var savedata = db.Database.BeginTransaction())
            {
                try
                {

                    if (e.Id != 0)
                    {
                        var username = User.Identity.GetUserName();
                        CultureInfo provider = new CultureInfo("en-LB");

                        var getId = db.Transaction.Where(x => x.ReservationId == e.Id).OrderByDescending(o => o.CreatedOn).Take(1).Select(l => l.Id).FirstOrDefault();
                        var getDebit = db.Transaction.Where(x => x.Id == getId).Select(a => a.DebitAmount).FirstOrDefault();
                        var getCredit = db.Transaction.Where(x => x.Id == getId).Select(a => a.CreditAmount).FirstOrDefault();

                        var getPrice = db.Reservation.Where(x => x.Id == e.Id).Select(a => a.Price).FirstOrDefault();
                        var getEPrice = db.Reservation.Where(x => x.Id == e.Id).Select(a => a.ExtraitemPrice).FirstOrDefault();
                        var getPaid = db.Reservation.Where(x => x.Id == e.Id).Select(a => a.PaidPrice).FirstOrDefault();
                        if (getPaid == null) { decimal? v = getPaid = 0; }

                        var getClientId = db.Reservation.Where(x => x.Id == e.Id).Select(a => a.Client).FirstOrDefault();

                        var cBalance = db.Customer.Where(x => x.Id == getClientId).Select(k => k.Balance).FirstOrDefault();
                        if (cBalance == null) { decimal? v = cBalance = 0; }
                        Customer c = new Customer();
                        c = db.Customer.FirstOrDefault(f => f.Id == getClientId);


                        Transaction t = new Transaction();
                        t = db.Transaction.FirstOrDefault(f => f.Id == getId);
                        t.UserId = username;
                        t.VoucherType = "Update Reservation";
                        t.TransactionDate = Convert.ToDateTime(e.Fromdate);
                        t.Narration = e.ScheduleTitle + " - " + Convert.ToDateTime(e.Fromdate).ToString("dddd, dd MMMM yyyy hh:mm tt") + " to " + Convert.ToDateTime(e.ToDate).ToString("dddd, dd MMMM yyyy hh:mm tt");


                        if (getPrice != 0)
                        {
                            if (e.Price > getPrice)
                            {
                                t.CreditAmount = getCredit + (e.Price - getPrice);

                                getCredit = t.CreditAmount;

                                c.Balance = cBalance - (e.Price - getPrice);
                                db.Entry(c).State = EntityState.Modified;
                            }
                            else if (e.Price < getPrice)
                            {
                                t.CreditAmount = getCredit - (getPrice - e.Price);

                                getCredit = t.CreditAmount;

                                c.Balance = cBalance + (getPrice - e.Price);
                                db.Entry(c).State = EntityState.Modified;
                            }
                            else
                            {
                                t.CreditAmount = getCredit;

                                getCredit = t.CreditAmount;
                            }

                            db.Entry(t).State = EntityState.Modified;
                        }


                        if (e.ExtraitemPrice != null)
                        {

                            if (e.ExtraitemPrice > getEPrice)
                            {
                                t.CreditAmount = getCredit + (e.ExtraitemPrice - getEPrice);
                                getCredit = t.CreditAmount;

                                cBalance = db.Customer.Where(x => x.Id == getClientId).Select(k => k.Balance).FirstOrDefault();
                                c.Balance = cBalance - (e.ExtraitemPrice - getEPrice);
                                db.Entry(c).State = EntityState.Modified;
                            }
                            else if (e.ExtraitemPrice < getEPrice)
                            {
                                t.CreditAmount = getCredit - (getEPrice - e.ExtraitemPrice);

                                getCredit = t.CreditAmount;
                                cBalance = db.Customer.Where(x => x.Id == getClientId).Select(k => k.Balance).FirstOrDefault();
                                c.Balance = cBalance + (getEPrice - e.ExtraitemPrice);
                                db.Entry(c).State = EntityState.Modified;
                            }
                            else
                            {
                                t.CreditAmount = getCredit;
                                getCredit = t.CreditAmount;
                            }
                        }
                        if (e.PaidPrice != null)
                        {
                            if (e.PaidPrice == null) { e.PaidPrice = 0; }

                            if (e.PaidPrice > getPaid)
                            {
                                t.DebitAmount = getDebit + (e.PaidPrice - getPaid);
                                c.Balance = cBalance + (e.PaidPrice - getPaid);
                                db.Entry(c).State = EntityState.Modified;


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

                                tc.Client = getClientId;
                                tc.DebitAmount = (e.PaidPrice - getPaid);
                                tc.CreditAmount = 0;
                                tc.Narration = "For reservation on  " +  Convert.ToDateTime(e.Fromdate).ToString("dddd, dd MMMM yyyy hh:mm tt");
                                tc.ReservationId = e.Id;
                                tc.VoucherType = "Cash Reservation";
                                tc.TrasactionalAmount = e.Price + Convert.ToDecimal(e.ExtraitemPrice) + Convert.ToDecimal(e.PaidPrice); ;
                                if (tc.DebitAmount == null) { tc.DebitAmount = 0; }
                                if (tc.CreditAmount == null) { tc.CreditAmount = 0; }
                                tc.TransactionDate = Convert.ToDateTime(e.Fromdate);

                                db.Cashbook.Add(tc);

                            }
                            else if (e.PaidPrice < getPaid)
                            {
                                t.DebitAmount = getDebit - (getPaid - e.PaidPrice);
                                c.Balance = cBalance - (getPaid - e.PaidPrice);

                                db.Entry(c).State = EntityState.Modified;



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

                                tc.Client = getClientId;
                                tc.DebitAmount = 0;
                                tc.CreditAmount = (getPaid - e.PaidPrice);
                                tc.Narration = "For reservation on  " + Convert.ToDateTime(e.Fromdate).ToString("dddd, dd MMMM yyyy hh:mm tt");
                                tc.ReservationId = e.Id;
                                tc.VoucherType = "Cash Reservation";
                                tc.TrasactionalAmount = e.Price + Convert.ToDecimal(e.ExtraitemPrice) + Convert.ToDecimal(e.PaidPrice);
                                if (tc.DebitAmount == null) { tc.DebitAmount = 0; }
                                if (tc.CreditAmount == null) { tc.CreditAmount = 0; }
                                tc.TransactionDate = Convert.ToDateTime(e.Fromdate);

                                db.Cashbook.Add(tc);

                            }
                            else
                            {
                                 t.CreditAmount = getCredit;
                            }

                        }
                        db.Entry(t).State = EntityState.Modified;



                        Reservation s = new Reservation();
                        s = db.Reservation.FirstOrDefault(f => f.Id == e.Id);
                        if (e.Price != 0) { s.Price = e.Price; }
                        if (e.ExtraitemPrice != null) { s.ExtraitemPrice = e.ExtraitemPrice; }
                        if (e.PaidPrice != null) { s.PaidPrice = e.PaidPrice; }
                        if (e.BillingStatus.ToString() == "Confirmed") { s.Color = "green"; }
                        if (e.BillingStatus.ToString() == "Pending") { s.Color = "red"; }
                        s.UpdateUser = username;
                        s.StartedAt = Convert.ToDateTime(e.Fromdate);
                        s.EndAt = Convert.ToDateTime(e.ToDate);
                        s.Note = e.Note;
                        s.BillingStatus = e.BillingStatus;
                        s.IsBallRent = e.IsBallRent;
                        s.ItemId = e.ItemId;
                        db.Entry(s).State = EntityState.Modified;

                        await db.SaveChangesAsync();

                        savedata.Commit();
                        status = true;
                    }
                }
                catch (Exception)
                {
                    savedata.Rollback();
                    status = false;

                }

            }

            return new JsonResult { Data = new { status } };
        }

        [HttpPost]
        public JsonResult CancelReservation(int id)
        {
            var status = false;
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                if (id != 0)
                {
                    var username = User.Identity.GetUserName();
                    CultureInfo provider = new CultureInfo("en-LB");

                    Reservation s = new Reservation();
                    s = db.Reservation.FirstOrDefault(f => f.Id == id);
                    s.CancelUser = username;
                    s.CancelDate = DateTime.Now.ToUniversalTime().AddHours(2);
                    s.Status = false;

                    db.Entry(s).State = EntityState.Modified;

                    Transaction t = new Transaction();

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
                    t.VoucherType = "Cancel Reservation";
                    t.TrasactionalAmount = s.Price + Convert.ToDecimal(s.ExtraitemPrice) + Convert.ToDecimal(s.PaidPrice);
                    t.Narration = s.ScheduleTitle + " - " + s.StartedAt.ToString("dddd, dd MMMM yyyy hh:mm tt") + " to " + s.EndAt.ToString("dddd, dd MMMM yyyy hh:mm tt");
                    t.DebitAmount = s.Price + Convert.ToDecimal(s.ExtraitemPrice);
                    t.CreditAmount = Convert.ToDecimal(s.PaidPrice);
                    t.TransactionDate = s.StartedAt;
                    t.Client = s.Client;
                    t.ReservationId = s.Id;
                    db.Transaction.Add(t);

                    if (Convert.ToDecimal(s.PaidPrice) != 0)
                    {
                        Cashbook tc = new Cashbook();

                        var vtype = db.Cashbook.Where(x => x.ReservationId == s.Id).Any(a => a.VoucherType == "Cash Reservation (Monthly)");
                        tc.Id = Guid.NewGuid();
                       
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
                        tc.DebitAmount = 0;
                        tc.CreditAmount = s.PaidPrice;
                        tc.Narration = "For reservation on  " + Convert.ToDateTime(s.StartedAt).ToString("dddd, dd MMMM yyyy hh:mm tt");
                        tc.ReservationId = s.Id;
                        if (vtype) { tc.VoucherType = "Monthly Reservation Cancel"; }else { tc.VoucherType = "Cash Reservation Cancel"; }
                        if (vtype) { tc.CreatedOn = s.StartedAt; } else { tc.CreatedOn = DateTime.Now.ToUniversalTime().AddHours(2);  }
                        tc.TrasactionalAmount =  Convert.ToDecimal(s.PaidPrice); ;
                        if (tc.DebitAmount == null) { tc.DebitAmount = 0; }
                        if (tc.CreditAmount == null) { tc.CreditAmount = 0; }
                        tc.TransactionDate = s.StartedAt;

                        db.Cashbook.Add(tc);
                    }


                    var cBalance = db.Customer.Where(x => x.Id == s.Client).Select(z => z.Balance).FirstOrDefault();

                    Customer c = new Customer();
                    c = db.Customer.FirstOrDefault(f => f.Id == s.Client);
                    c.Balance = Convert.ToDecimal(cBalance) + (s.Price + Convert.ToDecimal(s.ExtraitemPrice)) - (Convert.ToDecimal(s.PaidPrice));
                    db.Entry(c).State = EntityState.Modified;

                    db.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status } };
        }
        public JsonResult GetCustomerData(string search)
        {
            search = search.ToLower();
            var customer = db.Customer.Where(x => x.Blacklist == false).Where(a => a.FirstName.ToLower().Contains(search) || a.LastName.ToLower().Contains(search) || a.Code.ToString().ToLower().Contains(search) || a.Mobile.ToLower().Contains(search) || a.Phone.ToLower().Contains(search)).Select(s => new
            {
                s.Id,
                s.Code,
                s.FirstName,
                s.LastName,
                s.Phone,
                s.Mobile
            }).OrderBy(o=>o.LastName).ToList();

            return Json(customer, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<JsonResult> SaveReservation(Reservation e)
        {
            var status = false;
            string massage ="";
            bool IsNumberExixt = false;
            using (var savedata = db.Database.BeginTransaction())
            {
                try
                {
                    if (e.Fromdate != null)
                    {
                        if (e.Client != 0 && e.FirstName == "" || e.FirstName == null)
                        {
                            var itemTitle = db.Items.Where(x => x.Id == e.ItemId).Select(z => z.Name).FirstOrDefault();
                            var customerCode = db.Customer.Where(a => a.Id == e.Client).Select(s => s.Code).FirstOrDefault();
                            var customerName = db.Customer.Where(a => a.Id == e.Client).Select(s => s.FirstName).FirstOrDefault() + " " + db.Customer.Where(a => a.Id == e.Client).Select(s => s.LastName).FirstOrDefault();
                            e.ScheduleTitle = customerName + " - " + itemTitle;
                            e.Client = e.Client;
                        }
                        if (e.FirstName != null)
                        {
                            if (db.Customer.Any(a => a.Mobile == e.Mobile)) {
                                massage = "Mobile number already exist";
                                return new JsonResult { Data = new { IsNumberExixt = true, massage } };
                            }
                            else
                            {
                                Customer cus = new Customer();

                                cus.FirstName = e.FirstName;
                                cus.LastName = e.LastName;
                                cus.Mobile = e.Mobile;
                                var itemTitle = db.Items.Where(x => x.Id == e.ItemId).Select(z => z.Name).FirstOrDefault();
                                var rowcount = db.Customer.Count();
                                if (rowcount > 0)
                                {
                                    var cuscode = db.Customer.Max(x => x.Code);
                                    cus.Code = cuscode + 1;
                                }
                                else
                                {
                                    cus.Code = 1000;
                                }
                                cus.CreatedOn = DateTime.Now.ToUniversalTime().AddHours(2);
                                cus.Phone = e.Phone;
                                db.Customer.Add(cus);
                                db.SaveChanges();
                                e.Client = cus.Id;
                                e.ScheduleTitle = e.FirstName + " " + e.LastName + " - " + itemTitle;
                            }
                        }

                        var username = User.Identity.GetUserName();
                        CultureInfo provider = new CultureInfo("en-LB");
                        e.StartedAt = Convert.ToDateTime(e.Fromdate);
                        e.EndAt = Convert.ToDateTime(e.ToDate);


                        if (e.Advancereservation == true)
                        {

                            var getDate = e.StartedAt;
                            var getDay = e.StartedAt.Day;
                            var theTime = e.StartedAt.TimeOfDay;
                            var theDay = e.StartedAt.DayOfWeek;
                            var lastDayOfMonth = new DateTime(getDate.Year, getDate.Month, DateTime.DaysInMonth(getDate.Year, getDate.Month));
                            var lastDay = lastDayOfMonth.Day;
                            var days = Convert.ToInt16(lastDay - getDay);
                            var numberOfweek = Convert.ToInt16(days / 7) + 1;

                            List<Reservation> r = new List<Reservation>();
                            for (int i = 0; i < numberOfweek; i++)
                            {
                                var rv = new Reservation();
                                if (i == 0)
                                {
                                    rv = new Reservation()
                                    {
                                        StartedAt = e.StartedAt,
                                        EndAt = e.EndAt,
                                        PaidPrice = e.PaidPrice,
                                        //ExtraitemPrice = e.ExtraitemPrice,
                                        BillingStatus = e.BillingStatus,
                                        //IsBallRent = e.IsBallRent
                                    };
                                }
                                else
                                {
                                    rv = new Reservation()
                                    {
                                        StartedAt = e.StartedAt.AddDays(i * 7),
                                        EndAt = e.EndAt.AddDays(i * 7),
                                        PaidPrice = 0,
                                        //ExtraitemPrice = 0,
                                    };
                                }

                                r.Add(rv);
                            }
                            foreach (var re in r)
                            {
                                if (IsExixt(re.StartedAt, re.EndAt, e.ItemId))
                                {

                                    massage = re.StartedAt.ToString("dddd, dd MMMM yyyy hh:mm tt") + " is not available";
                                    return new JsonResult { Data = new { status = false, massage } };
                                }
                            }
                            foreach (var re in r)
                            {
                                if (!IsExixt( re.StartedAt, re.EndAt, e.ItemId)) {
                                    re.StartedAt = re.StartedAt;
                                    re.EndAt = re.EndAt;
                                    re.CreateDate = DateTime.Now.ToUniversalTime().AddHours(2);
                                    re.CreateUser = username;
                                    re.ScheduleTitle = e.ScheduleTitle;
                                    re.Client = e.Client;
                                    re.Status = true;
                                    if (re.BillingStatus.ToString() == "Confirmed") { re.Color = "green"; }
                                    if (re.BillingStatus.ToString() == "Pending" || re.BillingStatus.ToString() == null) { re.Color = "red"; }
                                    if (e.Price != 0)
                                    {
                                        re.ScheduleNote = "Price: " + re.Price.ToString() + " Total : " + re.TotalPrice.ToString();
                                    }
                                    re.BillingStatus = re.BillingStatus;
                                    re.Price = e.Price;
                                    re.PaidPrice = re.PaidPrice;
                                    re.ExtraitemPrice = e.ExtraitemPrice;
                                    re.IsBallRent = e.IsBallRent;
                                    re.ItemId = e.ItemId;
                                    re.Note = e.Note;


                                    db.Reservation.Add(re);

                                    db.SaveChanges();

                                    Transaction t = new Transaction();

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
                                    t.VoucherType = "Reservation";
                                    t.TrasactionalAmount = re.Price + Convert.ToDecimal(e.ExtraitemPrice) + Convert.ToDecimal(re.PaidPrice);
                                    t.Narration = re.ScheduleTitle + " - " + re.StartedAt.ToString("dddd, dd MMMM yyyy hh:mm tt") + " to " + re.EndAt.ToString("dddd, dd MMMM yyyy hh:mm tt");
                                    t.DebitAmount = Convert.ToDecimal(re.PaidPrice);
                                    t.CreditAmount = re.Price + Convert.ToDecimal(e.ExtraitemPrice);
                                    t.TransactionDate = re.StartedAt;
                                    t.CreatedOn = DateTime.Now.ToUniversalTime().AddHours(2);
                                    t.Client = re.Client;
                                    t.ReservationId = re.Id;
                                    db.Transaction.Add(t);

                                    db.SaveChanges();

                                    if (re.PaidPrice == null) { re.PaidPrice = 0; }

                                    if (re.PaidPrice != 0)
                                    {
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
                                        tc.CreditAmount = 0;
                                        tc.Narration = "For reservation on  " + Convert.ToDateTime(e.StartedAt).ToString("dddd, dd MMMM yyyy hh:mm tt");
                                        tc.ReservationId = re.Id;
                                        tc.VoucherType = "Cash Reservation";
                                        tc.TrasactionalAmount = re.Price + Convert.ToDecimal(e.ExtraitemPrice) + Convert.ToDecimal(re.PaidPrice); ;
                                        if (tc.DebitAmount == null) { tc.DebitAmount = 0; }
                                        if (tc.CreditAmount == null) { tc.CreditAmount = 0; }
                                        tc.TransactionDate = re.StartedAt;

                                        db.Cashbook.Add(tc);
                                        db.SaveChanges();
                                    }


                                    var cBalance = db.Customer.Where(x => x.Id == e.Client).Select(s => s.Balance).FirstOrDefault();

                                    Customer c = new Customer();
                                    c = db.Customer.FirstOrDefault(f => f.Id == e.Client);
                                    c.Balance = Convert.ToDecimal(cBalance) + Convert.ToDecimal(re.PaidPrice) - (re.Price + Convert.ToDecimal(e.ExtraitemPrice));
                                    db.Entry(c).State = EntityState.Modified;
                                }
                                else { massage = re.StartedAt.ToString("dddd, dd MMMM yyyy hh:mm tt") + " is not available"; }
                               
                            }

                        }
                        else
                        {
                            if (!IsExixt(e.StartedAt, e.EndAt, e.ItemId))
                            {
                                if (e.BillingStatus.ToString() == "Confirmed") { e.Color = "green"; }
                                if (e.BillingStatus.ToString() == "Pending" || e.BillingStatus.ToString() == null) { e.Color = "red"; }

                                e.Status = true;

                                e.CreateDate = DateTime.Now.ToUniversalTime().AddHours(2);
                                e.CreateUser = username;
                                if (e.Price != 0)
                                {
                                    e.ScheduleNote = "Price: " + e.Price.ToString() + " Total : " + e.TotalPrice.ToString();
                                }
                                db.Reservation.Add(e);

                                Transaction t = new Transaction();

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
                                t.VoucherType = "Reservation";

                                t.TrasactionalAmount = e.Price + Convert.ToDecimal(e.ExtraitemPrice) + Convert.ToDecimal(e.PaidPrice);
                                t.Narration = e.ScheduleTitle + " - " + e.StartedAt.ToString("dddd, dd MMMM yyyy hh:mm tt") + " to " + e.EndAt.ToString("dddd, dd MMMM yyyy hh:mm tt");
                                t.DebitAmount = Convert.ToDecimal(e.PaidPrice);
                                t.CreditAmount = e.Price + Convert.ToDecimal(e.ExtraitemPrice);
                                t.CreatedOn = DateTime.Now.ToUniversalTime().AddHours(2);
                                t.TransactionDate = e.StartedAt;
                                t.Client = e.Client;
                                t.ReservationId = e.Id;
                                db.Transaction.Add(t);

                                if (e.PaidPrice == null) { e.PaidPrice = 0; }

                                if (e.PaidPrice != 0)
                                {
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
                                    tc.CreditAmount = 0;
                                    tc.Narration = "For reservation on  " + Convert.ToDateTime(e.StartedAt).ToString("dddd, dd MMMM yyyy hh:mm tt");
                                    tc.ReservationId = e.Id;
                                    tc.VoucherType = "Cash Reservation";
                                    tc.TrasactionalAmount = e.Price + Convert.ToDecimal(e.ExtraitemPrice) + Convert.ToDecimal(e.PaidPrice); ;
                                    if (tc.DebitAmount == null) { tc.DebitAmount = 0; }
                                    if (tc.CreditAmount == null) { tc.CreditAmount = 0; }
                                    tc.TransactionDate = e.StartedAt;

                                    db.Cashbook.Add(tc);
                                    db.SaveChanges();
                                }

                                var cBalance = db.Customer.Where(x => x.Id == e.Client).Select(s => s.Balance).FirstOrDefault();

                                Customer c = new Customer();
                                c = db.Customer.FirstOrDefault(f => f.Id == e.Client);
                                c.Balance = Convert.ToDecimal(cBalance) + Convert.ToDecimal(e.PaidPrice) - (e.Price + Convert.ToDecimal(e.ExtraitemPrice));
                                db.Entry(c).State = EntityState.Modified;
                            }
                            else { massage = e.StartedAt.ToString("dddd, dd MMMM yyyy hh:mm tt") + " is not available"; }
                        }

                        await db.SaveChangesAsync();

                        savedata.Commit();
                        status = true;
                    }

                }
                catch (Exception)
                {
                    savedata.Rollback();
                    status = false;

                }

            }

            return new JsonResult { Data = new { status , massage} };
        }

        private  bool IsExixt(DateTime start , DateTime end, int item)
        {
            
            var check = db.Reservation.Any(x => x.StartedAt > start && x.StartedAt < end && x.ItemId == item && x.Status == true);


            var check2 = db.Reservation.Any(x => x.StartedAt == start && x.EndAt == end && x.ItemId == item && x.Status == true);


            var check3 = db.Reservation.Any(x => x.EndAt > start && x.EndAt <=end && x.ItemId == item && x.Status == true);

            if (check || check2 || check3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    } 
}