using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Rebound.Models;

namespace Rebound.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Items
        [AccessAuthorize(Roles = "Item View")]
        public ActionResult Index()
        {
            var items = db.Items.Include(i => i.ItemCategory);
            return View(items.ToList());
        }

        // GET: Items/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: Items/Create
        [AccessAuthorize(Roles = "Item Create")]
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.ItemCategory, "Id", "Name");
            ViewBag.ItemList = db.Items.Include(i => i.ItemCategory).ToList();
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Item item, HttpPostedFileBase Image)
        {
            if (ModelState.IsValid)
            {
                if (Image != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(Image.FileName);
                    string extension = Path.GetExtension(Image.FileName);
                    fileName = fileName + DateTime.Now.ToUniversalTime().AddHours(2).ToString("yymmssfff") + extension;
                    item.Image = "~/Uploads/Items/" + fileName;
                    fileName = Path.Combine(Server.MapPath("~/Uploads/Items/"), fileName);
                    Image.SaveAs(fileName);
                }

                item.Published = true;
                db.Items.Add(item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.ItemCategory, "Id", "Name", item.CategoryId);
            ViewBag.ItemList = db.Items.Include(i => i.ItemCategory).ToList();
            return View(item);
        }

        // GET: Items/Edit/5
        [AccessAuthorize(Roles = "Item Update")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.ItemCategory, "Id", "Name", item.CategoryId);
            ViewBag.ItemList = db.Items.Include(i => i.ItemCategory).ToList();
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Item item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                db.Entry(item).Property(p => p.Image).IsModified = false;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.ItemCategory, "Id", "Name", item.CategoryId);
            ViewBag.ItemList = db.Items.Include(i => i.ItemCategory).ToList();
            return View(item);
        }

        // GET: Items/Delete/5
        [AccessAuthorize(Roles = "Item Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Item item = db.Items.Find(id);
            db.Items.Remove(item);
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
