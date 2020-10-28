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
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Rebound.Models;

namespace Rebound.Controllers
{
    [Authorize]
    public class OperatorsController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public OperatorsController()
        {
        }

        public OperatorsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Operators
        [AccessAuthorize(Roles = "Operator View")]
        public ActionResult Index()
        {
            return View(db.Operators.ToList());
        }

        // GET: Operators/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Operators operators = db.Operators.Find(id);
            if (operators == null)
            {
                return HttpNotFound();
            }
            return View(operators);
        }

        // GET: Operators/Create
        [AccessAuthorize(Roles = "Operator Create")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Operators/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(OperatorsVm model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, Gender = model.Gender };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    Operators operators = new Operators();
                    operators.FirstName = model.FirstName;
                    operators.LastName = model.LastName;
                    operators.Mobile = model.Mobile;
                    operators.Phone = model.Phone;
                    operators.Email = model.Email;
                    operators.Gender = model.Gender;
                    operators.Status = true;
                    operators.AppUserId = user.Id;
                    db.Operators.Add(operators);
                    db.SaveChanges();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User name is already exist.");
                    return View(model);
                }
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Operators/Edit/5
        [AccessAuthorize(Roles = "Operator Update")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Operators operators = db.Operators.Find(id);
            if (operators == null)
            {
                return HttpNotFound();
            }
            return View(operators);
        }

        // POST: Operators/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Operators operators)
        {
            if (ModelState.IsValid)
            {
                Operators o = new Operators();
                o = db.Operators.FirstOrDefault(f => f.Id == operators.Id);
                o.FirstName = operators.FirstName;
                o.LastName = operators.LastName;
                o.Gender = operators.Gender;
                o.Phone = operators.Phone;
                o.Mobile = operators.Mobile;

                db.Entry(o).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(operators);
        }

        // GET: Operators/Delete/5
        [AccessAuthorize(Roles = "Operator Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Operators operators = db.Operators.Find(id);
            if (operators == null)
            {
                return HttpNotFound();
            }
            return View(operators);
        }
        public JsonResult GetOperators()
        {
            var result = db.Operators.Select(s => new
            {
                name = s.FirstName + " " + s.LastName,
                email = s.Email,
                mobile = s.Mobile,
                phone = s.Phone,
                id = s.Id

            }).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }


        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
                if (id == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var  uid =  db.Operators.Where(x=>x.Id == id).Select(s=>s.AppUserId).FirstOrDefault();

            var UserName = db.Users.Where(x => x.Id == uid.ToString()).Select(s => s.Email).FirstOrDefault();
            // IList<string> result = null;

            ApplicationUser user = db.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));


           // var user = await _userManager.FindByIdAsync(uid);
                var logins = user.Logins;
                var rolesForUser = await UserManager.GetRolesAsync(uid);

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var login in logins.ToList())
                        {
                            await UserManager.RemoveLoginAsync(login.UserId, new UserLoginInfo(login.LoginProvider, login.ProviderKey));
                        }

                        if (rolesForUser.Count() > 0)
                        {
                            foreach (var item in rolesForUser.ToList())
                            {
                                // item should be the name of the role
                                var result = await UserManager.RemoveFromRoleAsync(user.Id, item);
                            }
                        }

                        await UserManager.DeleteAsync(user);

                        Operators operators = db.Operators.Find(id);
                        db.Operators.Remove(operators);

                        await db.SaveChangesAsync();

                        transaction.Commit();
                        

                    }
                    catch (Exception)
                    {

                    transaction.Rollback();
                    }

                   
                }

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
