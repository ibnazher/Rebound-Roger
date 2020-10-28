using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Rebound.Models;

namespace Rebound.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Categories
        [AccessAuthorize(Roles = "Categories View")]
        public ActionResult Index()
        {
            return View(db.ItemCategory.ToList());
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemCategory itemCategory = db.ItemCategory.Find(id);
            if (itemCategory == null)
            {
                return HttpNotFound();
            }
            return View(itemCategory);
        }

        // GET: Categories/Create
        [AccessAuthorize(Roles = "Categories Create")]
        public ActionResult Create()
        {
            ViewBag.CaatList = db.ItemCategory.ToList();
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] ItemCategory itemCategory)
        {
            if (ModelState.IsValid)
            {
                db.ItemCategory.Add(itemCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CaatList = db.ItemCategory.ToList();
            return View(itemCategory);
        }

        // GET: Categories/Edit/5
        [AccessAuthorize(Roles = "Categories Update")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.CaatList = db.ItemCategory.ToList();
            ItemCategory itemCategory = db.ItemCategory.Find(id);
            if (itemCategory == null)
            {
                return HttpNotFound();
            }
            return View(itemCategory);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] ItemCategory itemCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(itemCategory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CaatList = db.ItemCategory.ToList();
            return View(itemCategory);
        }

        // GET: Categories/Delete/5
        [AccessAuthorize(Roles = "Categories Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemCategory itemCategory = db.ItemCategory.Find(id);
            if (itemCategory == null)
            {
                return HttpNotFound();
            }
            return View(itemCategory);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ItemCategory itemCategory = db.ItemCategory.Find(id);
            db.ItemCategory.Remove(itemCategory);
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
