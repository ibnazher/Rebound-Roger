using Microsoft.Ajax.Utilities;
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

namespace Rebound.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }
        [AccessAuthorize(Roles = "Bills Report")]
        public ActionResult Billing()
        {
            return View();
        }
        [AccessAuthorize(Roles = "Cash Report")]
        public ActionResult CashReport()
        {
            return View();
        }
        [AccessAuthorize(Roles = "Cash Report")]
        public ActionResult MonthlyReport()
        {
            return View();
        }
        [AccessAuthorize(Roles = "Dues Report")]
        public ActionResult DueBills()
        {
            return View();
        }
        [AccessAuthorize(Roles = "Cancel Report")]
        public ActionResult CancelBills()
        {
            return View();
        }
        [AccessAuthorize(Roles = "Audit Report")]
        public ActionResult Auditreport()
        {
            return View();
        }
        [AccessAuthorize(Roles = "Audit Report")]
        public ActionResult Paidreservation()
        {
            return View();
        }


        public JsonResult GetCount(string start, string end)
        {
            CultureInfo provider = new CultureInfo("en-LB");
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);

            var j = sDate.DayOfWeek;

            var sDateY = sDate.Year;
            var sDateM = sDate.Month;
            var sDateD = sDate.Day;

            var eDateY = eDate.Year;
            var eDateM = eDate.Month;
            var eDateD = eDate.Day;


            DateTime baseDate = sDate;
            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

            var lastWeekStart = baseDate.AddDays(-((int)baseDate.DayOfWeek + 7));
            var lastWeekEnd = lastWeekStart.AddDays(7).AddSeconds(-1);

            var thisMonth = new DateTime(sDate.Year, sDate.Month, 1);
            var thisMonthEnd = new DateTime(sDate.Year, sDate.Month, DateTime.DaysInMonth(sDate.Year, sDate.Month));

            var thisYear = new DateTime(sDate.Year, 1, 1);
            var thisYearEnd = new DateTime(sDate.Year, 12, DateTime.DaysInMonth(sDate.Year, 12));

            var lastYear = new DateTime(sDate.AddYears(-1).Year, sDateM, DateTime.DaysInMonth(sDate.AddYears(-1).Year, sDateM));
            var lastYearEnd = new DateTime(sDate.AddYears(-1).Year, eDateM, DateTime.DaysInMonth(sDate.AddYears(-1).Year, eDateM));


            try
            {
                var getCustomer = db.Customer.Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate).Count();
                var tAmount = db.Reservation.Where(x => x.StartedAt >= sDate && x.EndAt <= eDate && x.Status == true).Sum(s => s.Price) + db.Reservation.Where(x => x.StartedAt >= sDate && x.EndAt <= eDate && x.Status == true && x.ExtraitemPrice != null).Sum(s => s.ExtraitemPrice);
                var mEarning = db.Cashbook.Where(q => q.VoucherType != "Cash Reservation (Monthly)" && q.VoucherType != "Monthly Reservation Cancel").Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate && x.DebitAmount != null).Sum(s => s.DebitAmount) - db.Cashbook.Where(q => q.VoucherType != "Cash Reservation (Monthly)" && q.VoucherType != "Monthly Reservation Cancel").Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate && x.CreditAmount != null).Sum(s => s.CreditAmount);

                var monthEarning = db.Cashbook.Where(q => q.VoucherType == "Cash Reservation (Monthly)" || q.VoucherType == "Monthly Reservation Cancel").Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate && x.DebitAmount != null).Sum(s => s.DebitAmount) - db.Cashbook.Where(q => q.VoucherType == "Cash Reservation (Monthly)" || q.VoucherType == "Monthly Reservation Cancel").Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate && x.CreditAmount != null).Sum(s => s.CreditAmount);

                var yEarning = db.Cashbook.Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate && x.DebitAmount != null).Sum(s => s.DebitAmount);

                int resurvationC = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).Count();

                var mPaid = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).Sum(s => (decimal?) s.PaidPrice) ?? 0;

                var tMonth = db.Cashbook.Where(x => x.TransactionDate >= sDate && x.TransactionDate <= eDate && x.DebitAmount != null).ToList();

                var monthlistT = tMonth.OrderBy(o => o.TransactionDate).GroupBy(g => g.TransactionDate.Month).Select(s =>
                            new {
                                month = s.FirstOrDefault().TransactionDate.ToString("MMM", provider)
                            }).Select(z => z.month).ToArray();

                var sumlistT = tMonth.OrderBy(o => o.TransactionDate).GroupBy(g => g.TransactionDate.Month).Select(s =>
                            new {
                                sum = s.Sum(a => a.DebitAmount - a.CreditAmount),
                            }).Select(z => z.sum).ToArray();

                var tMonth2 = db.Cashbook.Where(x => x.TransactionDate >= lastYear && x.TransactionDate <= lastYearEnd && x.DebitAmount != null).ToList();

                var sumlistT2 = tMonth2.OrderBy(o => o.TransactionDate).GroupBy(g => g.TransactionDate.Month).Select(s =>
                            new
                            {
                                sum = s.Sum(a => a.DebitAmount - a.CreditAmount),
                            }).Select(z => z.sum).ToArray();

                var tWeek = db.Cashbook.Where(x => x.TransactionDate >= thisWeekStart && x.TransactionDate <= thisWeekEnd && x.DebitAmount != null).ToList();
                var weeklistT = tWeek.OrderBy(o => o.TransactionDate).GroupBy(g => g.TransactionDate.Day).Select(s =>
                            new {
                                day = s.FirstOrDefault().TransactionDate.ToString("ddd", provider)
                            }).Select(z => z.day).ToArray();
                var weeksumlistT = tWeek.OrderBy(o => o.TransactionDate).GroupBy(g => g.TransactionDate.Day).Select(s =>
                            new {
                                sum = s.Sum(a => a.DebitAmount - a.CreditAmount),
                            }).Select(z => z.sum).ToArray();

                var tWeek2 = db.Cashbook.Where(x => x.TransactionDate >= lastWeekStart && x.TransactionDate <= lastWeekEnd && x.DebitAmount != null).ToList();

                var weeksumlistT2 = tWeek2.OrderBy(o => o.TransactionDate).GroupBy(g => g.TransactionDate.Day).Select(s =>
                            new
                            {
                                sum = s.Sum(a => a.DebitAmount - a.CreditAmount),
                            }).Select(z => z.sum).ToArray();

                var rMonth = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).ToList();

                var monthlistR = rMonth.OrderBy(o => o.StartedAt).GroupBy(g => g.StartedAt.Month).Select(s =>
                            new {
                                month = s.FirstOrDefault().StartedAt.ToString("MMM", provider)
                            }).Select(z => z.month).ToArray();

                var countlistR = rMonth.OrderBy(o => o.StartedAt).GroupBy(g => g.StartedAt.Month).Select(s =>
                            new {
                                count = s.Count(),
                            }).Select(z => z.count).ToArray();

                var rMonth2 = db.Reservation.Where(x => x.StartedAt >= lastYear && x.StartedAt <= lastYearEnd && x.Status == true).ToList();

                var countlistR2 = rMonth2.OrderBy(o => o.StartedAt).GroupBy(g => g.StartedAt.Month).Select(s =>
                            new
                            {
                                count = s.Count(),
                            }).Select(z => z.count).ToArray();

                var rWeek = db.Reservation.Where(x => x.StartedAt >= thisWeekStart && x.StartedAt <= thisWeekEnd && x.Status == true).ToList();
                var weeklistR = rWeek.OrderBy(o => o.StartedAt).GroupBy(g => g.StartedAt.Day).Select(s =>
                            new {
                                day = s.FirstOrDefault().StartedAt.ToString("ddd", provider)
                            }).Select(z => z.day).ToArray();
                var weeksumlistR = rWeek.OrderBy(o => o.StartedAt).GroupBy(g => g.StartedAt.Day).Select(s =>
                            new {
                                count = s.Count(),
                            }).Select(z => z.count).ToArray();

                var rWeek2 = db.Reservation.Where(x => x.StartedAt >= lastWeekStart && x.StartedAt <= lastWeekEnd && x.Status == true).ToList();

                var weeksumlistR2 = rWeek2.OrderBy(o => o.StartedAt).GroupBy(g => g.StartedAt.Day).Select(s =>
                            new
                            {
                                count = s.Count(),
                            }).Select(z => z.count).ToArray();

                var itemq = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).GroupBy(g => g.ItemId).Select(s => new { count = s.Count() }).Select(z => z.count).ToArray();
                var itemv = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).GroupBy(g => g.ItemId).Select(s =>
                new {
                    sum = s.Sum(l => l.Price),
                }).Select(z => z.sum).ToArray();
                var itemn = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).GroupBy(g => g.ItemId).Select(s =>
                new {
                    name = s.FirstOrDefault().Item.Name
                }).Select(z => z.name).ToArray();

                var ballrentD = db.Reservation.Where(x => x.StartedAt >= thisMonth && x.StartedAt <= thisMonthEnd && x.Status == true && x.IsBallRent == true).GroupBy(g => g.StartedAt.Month).Select(s => new { day = s.FirstOrDefault().StartedAt.Month, count = s.Count() }).Select(a => a.day).ToArray();

                var ballrentC = db.Reservation.Where(x => x.StartedAt >= thisMonth && x.StartedAt <= thisMonthEnd && x.Status == true && x.IsBallRent == true).GroupBy(g => g.StartedAt.Month).Select(s => new { day = s.FirstOrDefault().StartedAt.Month, count = s.Count() }).Select(a => a.count).ToArray();
                var getBall = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true && x.IsBallRent == true).Count();

                return Json(new { Customer = getCustomer, mPaid, monthEarning, tAmount, getBall, mEarning, yEarning, resurvationC, monthlistT, sumlistT, sumlistT2, weeklistT, weeksumlistT, weeksumlistT2, monthlistR, countlistR, countlistR2, weeklistR, weeksumlistR, weeksumlistR2, itemq, itemv, itemn, ballrentD, ballrentC, satus = true , start, end}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new {satus = false}, JsonRequestBehavior.AllowGet);
            }


           
        }
        public JsonResult GetBills(string start, string end)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                CultureInfo provider = new CultureInfo("en-LB");
                DateTime sDate = Convert.ToDateTime(start);
                DateTime eDate = Convert.ToDateTime(end);
                eDate = eDate.AddDays(1).AddSeconds(-1);
                var result = db.Transaction.Where(x=>x.TransactionDate >= sDate && x.TransactionDate <= eDate).Select(s => new {
                    s.TransactionDate,
                    s.VoucherNo,
                    s.VoucherType,
                    s.TrasactionalAmount,
                    s.DebitAmount,
                    s.CreditAmount,
                    s.Client,
                    s.ReservationId,
                    s.Narration,
                    s.UserId,
                    s.Id,
                    Customer = s.Customer.FirstName + " " + s.Customer.LastName,
                    Balance = db.Transaction.Where(x=>x.CreatedOn<= s.CreatedOn).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.CreditAmount),
                    Cbalance = db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.CreditAmount)
                }).ToList();
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetCash(string start, string end)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                CultureInfo provider = new CultureInfo("en-LB");
                DateTime sDate = Convert.ToDateTime(start);
                DateTime eDate = Convert.ToDateTime(end);
                eDate = eDate.AddDays(1).AddSeconds(-1);
                var result = db.Cashbook.Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate).Where(q=>q.VoucherType != "Cash Reservation (Monthly)" && q.VoucherType != "Monthly Reservation Cancel").Select(s => new {
                    s.TransactionDate,
                    s.CreatedOn,
                    s.VoucherNo,
                    s.VoucherType,
                    s.TrasactionalAmount,
                    s.DebitAmount,
                    s.CreditAmount,
                    s.Client,
                    s.ReservationId,
                    s.Narration,
                    s.UserId,
                    s.Id,
                    
                    Customer = s.Customer.FirstName + " " + s.Customer.LastName,
                    Balance = db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.CreditAmount),
                    Cbalance = db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.CreditAmount)
                }).ToList();
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetMonthly(string start, string end)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                CultureInfo provider = new CultureInfo("en-LB");
                DateTime sDate = Convert.ToDateTime(start);
                DateTime eDate = Convert.ToDateTime(end);
                eDate = eDate.AddDays(1).AddSeconds(-1);
                var result = db.Cashbook.Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate).Where(q => q.VoucherType == "Cash Reservation (Monthly)" || q.VoucherType == "Monthly Reservation Cancel").Select(s => new {
                    s.TransactionDate,
                    s.CreatedOn,
                    s.VoucherNo,
                    s.VoucherType,
                    s.TrasactionalAmount,
                    s.DebitAmount,
                    s.CreditAmount,
                    s.Client,
                    s.ReservationId,
                    s.Narration,
                    s.UserId,
                    s.Id,

                    Customer = s.Customer.FirstName + " " + s.Customer.LastName,
                    Balance = db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.CreditAmount),
                    Cbalance = db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.CreditAmount)
                }).ToList();
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetReserve(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var date = DateTime.Now.ToUniversalTime().AddHours(2);
            var result =  db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).Select(s => new
            {   id = s.Id,
                date = s.StartedAt,
                start = s.StartedAt,
                end = s.EndAt,
                totalTime = new { start = s.StartedAt,end = s.EndAt },
                item = s.Item.Name,
                mobile = s.Customer.Mobile,
                customer = s.Customer.Code + "-" + s.Customer.FirstName + " " + s.Customer.LastName,
                amount = s.Price + s.ExtraitemPrice,
                ball = s.IsBallRent == true ? "Rental" : "None",
                status = s.StartedAt <= date ? "Active" : "Upcomming",
                billing = s.BillingStatus == BillingStatus.Confirmed ? "Confirmed" : "Pending",

            }).OrderBy(o=>o.date).ToList();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPaidReserve(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var date = DateTime.Now.ToUniversalTime().AddHours(2);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true && x.PaidPrice > 0).Select(s => new
            {
                id = s.Id,
                date = s.StartedAt,
                start = s.StartedAt,
                end = s.EndAt,
                totalTime = new { start = s.StartedAt, end = s.EndAt },
                item = s.Item.Name,
                mobile = s.Customer.Mobile,
                customer = s.Customer.Code + "-" + s.Customer.FirstName + " " + s.Customer.LastName,
                amount = s.Price + s.ExtraitemPrice,
                paid = s.PaidPrice,
                ball = s.IsBallRent == true ? "Rental" : "None",
                status = s.StartedAt <= date ? "Active" : "Upcomming",
                billing = s.BillingStatus == BillingStatus.Confirmed ? "Confirmed" : "Pending",

            }).OrderBy(o => o.date).ToList();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetReserveSummary(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var date = DateTime.Now.ToUniversalTime().AddHours(2);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).Count();
            var price = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).Sum(s => (decimal?) s.Price) ?? 0;
            var empice = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).Sum(s=>(decimal?)s.ExtraitemPrice) ?? 0;
            var amount = price + empice;
            var paid = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).Sum(s => (decimal?)s.PaidPrice) ?? 0;
            var due = amount - paid;
            return Json(new { total = result, amount, paid, due }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTopItem(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).GroupBy(g => g.ItemId).Select(s => new
            {
                name = s.FirstOrDefault().Item.Name,
                count = s.Count()
            }).OrderBy(o=>o.count).Take(10).ToList();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTopitemsrevenue(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).GroupBy(g => g.ItemId).Select(s => new
            {
                name = s.FirstOrDefault().Item.Name,
                count = s.Sum(a=>a.Price)
            }).OrderBy(o => o.count).Take(10).ToList();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTopCustomerbyRes(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).GroupBy(g => g.Client).Select(s => new
            {
                name = s.FirstOrDefault().Customer.FirstName + " " + s.FirstOrDefault().Customer.LastName,
                email = s.FirstOrDefault().Customer.Email,
                mobile = s.FirstOrDefault().Customer.Mobile,
                code = s.FirstOrDefault().Customer.Code,
                count = s.Count(),
            }).OrderByDescending(o => o.count).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTopCustomerbyAmount(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true).GroupBy(g => g.Client).Select(s => new
            {
                name = s.FirstOrDefault().Customer.FirstName + " " + s.FirstOrDefault().Customer.LastName,
                email = s.FirstOrDefault().Customer.Email,
                mobile = s.FirstOrDefault().Customer.Mobile,
                code = s.FirstOrDefault().Customer.Code,
                count = s.Sum(a=>a.Price),
            }).OrderByDescending(o => o.count).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTopCustomerbyPaid(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == true && x.PaidPrice != null && x.PaidPrice != 0).GroupBy(g => g.Client).Select(s => new
            {
                name = s.FirstOrDefault().Customer.FirstName + " " + s.FirstOrDefault().Customer.LastName,
                email = s.FirstOrDefault().Customer.Email,
                mobile = s.FirstOrDefault().Customer.Mobile,
                code = s.FirstOrDefault().Customer.Code,
                count = s.Sum(a => a.PaidPrice),
            }).OrderByDescending(o => o.count).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTopCustomerbyCancel(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.Status == false && x.CancelDate != null).GroupBy(g => g.Client).Select(s => new
            {
                name = s.FirstOrDefault().Customer.FirstName + " " + s.FirstOrDefault().Customer.LastName,
                email = s.FirstOrDefault().Customer.Email,
                mobile = s.FirstOrDefault().Customer.Mobile,
                code = s.FirstOrDefault().Customer.Code,
                count = s.Count(),
            }).OrderByDescending(o => o.count).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult CancelBillsdata(string start, string end)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                DateTime sDate = Convert.ToDateTime(start);
                DateTime eDate = Convert.ToDateTime(end);
                eDate = eDate.AddDays(1).AddSeconds(-1);
                var result = db.Transaction.Where(x => x.TransactionDate >= sDate && x.TransactionDate <= eDate && x.VoucherType == "Cancel Reservation").Select(s => new {
                    s.TransactionDate,
                    s.VoucherNo,
                    s.VoucherType,
                    s.TrasactionalAmount,
                    s.DebitAmount,
                    s.CreditAmount,
                    s.Client,
                    s.ReservationId,
                    s.Narration,
                    s.Id,
                    Customer = s.Customer.FirstName + " " + s.Customer.LastName,
                    Balance = db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.CreditAmount),
                    Cbalance = db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.CreditAmount),
                    User = s.UserId,
                }).ToList();
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DueBillData()
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var result = db.Customer.Select(s => new
                {
                    s.Address,
                    s.Code,
                    FullName = s.FirstName + " " + s.LastName,
                    s.Mobile,
                    s.PaymentType,
                    s.Id,
                    s.Balance

                }).Where(w=>w.Balance<0).ToList();

                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPendingreservationData(int? id)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                //&& (a.Price + Convert.ToDecimal(a.ExtraitemPrice)) != a.PaidPrice
                var resultdata = db.Reservation.Where(a => a.Client == id  && a.Status == true ).Select(s => new
                {
                    id = s.Id,
                    billingStatus = s.BillingStatus,
                    satrt = s.StartedAt,
                    end = s.EndAt,
                    items = s.Item.Name,
                    price = s.Price,
                    eprice = s.ExtraitemPrice,
                    paid = s.PaidPrice,
                    ballrent = s.IsBallRent,
                    title = s.ScheduleTitle,
                    totalprice = s.Price + (decimal) s.ExtraitemPrice,
                    client = s.Client

                }).Where(x=>x.totalprice != x.paid).OrderBy(o=>o.satrt).ToList();

                return Json(resultdata, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<JsonResult> UpdateEventPay(int? id, decimal? amount, BillingStatus bstatus, string note)
        {
            var status = "Data can't posted";
            using (var savedata = db.Database.BeginTransaction())
            {
                try
                {
                        var username = User.Identity.GetUserName();
                        CultureInfo provider = new CultureInfo("en-LB");

                        var startDate= db.Reservation.Where(x => x.Id == id).Select(a => a.StartedAt).FirstOrDefault();
                        var getId = db.Transaction.Where(x => x.ReservationId == id).OrderByDescending(o => o.CreatedOn).Take(1).Select(l => l.Id).FirstOrDefault();
                        var getDebit = db.Transaction.Where(x => x.Id == getId).Select(a => a.DebitAmount).FirstOrDefault();
                        var getCredit = db.Transaction.Where(x => x.Id == getId).Select(a => a.CreditAmount).FirstOrDefault();

                        var getPrice = db.Reservation.Where(x => x.Id == id).Select(a => a.Price).FirstOrDefault();
                        var getEPrice = db.Reservation.Where(x => x.Id == id).Select(a => a.ExtraitemPrice).FirstOrDefault();
                        var getPaid = db.Reservation.Where(x => x.Id == id).Select(a => a.PaidPrice).FirstOrDefault();
                        if (getPaid == null) { decimal? v = getPaid = 0; }

                        var getClientId = db.Reservation.Where(x => x.Id == id).Select(a => a.Client).FirstOrDefault();

                        var cBalance = db.Customer.Where(x => x.Id == getClientId).Select(k => k.Balance).FirstOrDefault();
                        if (cBalance == null) { decimal? v = cBalance = 0; }
                        Customer c = new Customer();
                        c = db.Customer.FirstOrDefault(f => f.Id == getClientId);


                        Transaction t = new Transaction();
                        t = db.Transaction.FirstOrDefault(f => f.Id == getId);
                        t.UserId = username;
                        t.VoucherType = "Update Reservation";



                        if (amount != null)
                        {
                                t.DebitAmount = getDebit + amount;
                                c.Balance = cBalance + amount;
                                db.Entry(c).State = EntityState.Modified;


                                Cashbook tc = new Cashbook();

                                tc.Id = Guid.NewGuid();
                                tc.CreatedOn = startDate;
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
                                tc.DebitAmount = amount;
                                tc.CreditAmount = 0;
                                tc.Narration = note;
                                tc.ReservationId = id;
                                tc.VoucherType = "Cash Reservation (Monthly)";
                                tc.TrasactionalAmount = Convert.ToDecimal(amount) ;
                                tc.TransactionDate = Convert.ToDateTime(startDate);

                                db.Cashbook.Add(tc);
                            

                        }
                        db.Entry(t).State = EntityState.Modified;
                        Reservation s = new Reservation(); 
                        s = db.Reservation.FirstOrDefault(f => f.Id == id);
                        s.PaidPrice = amount + getPaid; 
                        s.UpdateUser = username;

                        s.BillingStatus = bstatus;
                        if (bstatus.ToString() == "Confirmed") { s.Color = "green"; }
                        if (bstatus.ToString() == "Pending") { s.Color = "red"; }

                    db.Entry(s).State = EntityState.Modified;

                        await db.SaveChangesAsync();

                        savedata.Commit();
                        status = "The payment has been created successfully"; 
                }
                catch (Exception)
                {
                    savedata.Rollback();
                    status = "Data can't posted"; 

                }

            }

            return new JsonResult { Data = new { status } };
        }
        public JsonResult AuditReserveCreate(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate).Select(s => new
            {
                date = s.StartedAt,
                start = s.StartedAt,
                end = s.EndAt,
                user = s.CreateUser,
                customer = s.Customer.Code + "-" + s.Customer.FirstName + " " + s.Customer.LastName,
                amount = s.Price,
                paid = s.PaidPrice,
                create = s.CreateDate,
                ballrent = s.ExtraitemPrice,
                ball = s.IsBallRent == true ? "Rental" : "None",
                narrration = s.Item.Name + " - "+ s.StartedAt.ToString() +" to "+ s.EndAt.ToString() ,

            }).OrderBy(o => o.date).ToList();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AuditReserveUpdate(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.UpdateUser != null).Select(s => new
            {
                date = s.StartedAt,
                start = s.StartedAt,
                end = s.EndAt,
                user = s.UpdateUser,
                customer = s.Customer.Code + "-" + s.Customer.FirstName + " " + s.Customer.LastName,
                amount = s.Price,
                paid = s.PaidPrice,
                create = s.CreateDate,
                ballrent = s.ExtraitemPrice,
                ball = s.IsBallRent == true ? "Rental" : "None",
                narrration = s.Item.Name + " - " + s.StartedAt.ToString() + " to " + s.EndAt.ToString(),

            }).OrderBy(o => o.date).ToList();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AuditReserveCancel(string start, string end)
        {
            DateTime sDate = Convert.ToDateTime(start);
            DateTime eDate = Convert.ToDateTime(end);
            eDate = eDate.AddDays(1).AddSeconds(-1);
            var result = db.Reservation.Where(x => x.StartedAt >= sDate && x.StartedAt <= eDate && x.CreateDate != null && x.Status == false  ).Select(s => new
            {
                date = s.StartedAt,
                start = s.StartedAt,
                end = s.EndAt,
                user = s.CancelUser,
                customer = s.Customer.Code + "-" + s.Customer.FirstName + " " + s.Customer.LastName,
                amount = s.Price,
                paid = s.PaidPrice,
                create = s.CancelDate,
                ballrent = s.ExtraitemPrice,
                ball = s.IsBallRent == true ? "Rental" : "None",
                narrration = s.Item.Name + " - " + s.StartedAt.ToString() + " to " + s.EndAt.ToString(),

            }).OrderBy(o => o.date).ToList();
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AuditBillsbydate(string start, string end)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                DateTime sDate = Convert.ToDateTime(start);
                DateTime eDate = Convert.ToDateTime(end);
                eDate = eDate.AddDays(1).AddSeconds(-1);
                var result = db.Transaction.Where(x => x.TransactionDate >= sDate && x.TransactionDate <= eDate).Select(s => new {
                    s.TransactionDate,
                    s.CreatedOn,
                    voucher = s.VoucherNo + " - "+ s.VoucherType,
                    s.TrasactionalAmount,
                    s.DebitAmount,
                    s.CreditAmount,
                    s.Client,
                    s.ReservationId,
                    s.Narration,
                    s.UserId,
                    s.Id,
                    Customer = s.Customer.FirstName + " " + s.Customer.LastName,
                    Balance = db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.CreditAmount),
                    Cbalance = db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.CreditAmount)
                }).ToList();
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AuditBillsbycash(string start, string end)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                DateTime sDate = Convert.ToDateTime(start);
                DateTime eDate = Convert.ToDateTime(end);
                eDate = eDate.AddDays(1).AddSeconds(-1);
                var result = db.Transaction.Where(x => x.CreatedOn >= sDate && x.CreatedOn <= eDate).Select(s => new {
                    s.TransactionDate,
                    s.CreatedOn,
                    voucher = s.VoucherNo + " - " + s.VoucherType,
                    s.TrasactionalAmount,
                    s.DebitAmount,
                    s.CreditAmount,
                    s.Client,
                    s.ReservationId,
                    s.Narration,
                    s.UserId,
                    s.Id,
                    Customer = s.Customer.FirstName + " " + s.Customer.LastName,
                    Balance = db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= s.CreatedOn).Sum(a => a.CreditAmount),
                    Cbalance = db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.DebitAmount) - db.Transaction.Where(x => x.CreatedOn <= eDate && x.Client == s.Client).Sum(a => a.CreditAmount)
                }).ToList();
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}