using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Rebound.Models;

namespace Rebound.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminController()
        {
        }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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


        // GET: Admin
        public JsonResult GetRole()
        {
            var rolelist = db.Roles.ToList();
            return Json(rolelist, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AccessAuthorize(Roles = "Set Roles,systemAdmin")]
        public ActionResult Setrole()
        {
            var username = User.Identity.GetUserId();
            var userlist = db.Users.Where(s => s.Id != username && s.Email != "system@domain.com").ToList();
            var rolelist = db.Roles.Where(x => x.Name != "systemAdmin");
            ViewBag.Userlist = userlist;
            ViewBag.Rolelist = new SelectList(rolelist, "Name", "Name");

            return View();
        }
        [HttpGet]
        public ActionResult SetRoletouser(Guid? id)
        {
            var UserName = db.Users.Where(x => x.Id == id.ToString()).Select(s => s.Email).FirstOrDefault();
            // IList<string> result = null;

            ApplicationUser user = db.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            var result = UserManager.GetRoles(user.Id);
            var rolelist = db.Roles.Where(x => x.Name != "systemAdmin").OrderBy(x => x.Name).Select(s => s.Name).ToList();
            var data = rolelist.Except(result.Where(x => rolelist.Contains(x))).ToList();

            ViewBag.Roles = data;

            return View();
        }
        [HttpPost]
        public ActionResult SetRoletouser(Guid? id, string[] rolelist)
        {
            try
            {
                if (rolelist != null)
                {
                    var userid = id.ToString();
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                    UserManager.AddToRoles(userid, rolelist);
                }
                return RedirectToAction("Setrole");

            }
            catch (EntityException ex)
            {
                return Content(" Select Some Data" + ex);
            }
        }
        [HttpGet]
        [AccessAuthorize(Roles = "Remove Roles, systemAdmin")]
        public ActionResult Removerole(Guid? id)
        {
            var UserName = db.Users.Where(x => x.Id == id.ToString()).Select(s => s.Email).FirstOrDefault();
            IList<string> result = null;

            ApplicationUser user = db.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            result = UserManager.GetRoles(user.Id).ToList();
            ViewBag.Roles = result;


            return View();
        }

        [HttpPost]
        public ActionResult Removerole(Guid? id, string[] rolelist)
        {
            try
            {
                if (rolelist != null)
                {
                    var userid = id.ToString();
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

                    UserManager.RemoveFromRoles(userid, rolelist);
                    ViewBag.ResultMessage = "Role removed from this user successfully !";
                }
                else
                {
                    ViewBag.ResultMessage = "This user doesn't belong to selected role.";
                }
                return RedirectToAction("Setrole");
            }
            catch (EntityException ex)
            {
                return Content(" Select Some Data" + ex);
            }

        }
        public ActionResult ProfileUpdate()
        {
            var context = new ApplicationDbContext();
            var username = User.Identity.GetUserId();
            var UserMail = User.Identity.GetUserName();
            if (!string.IsNullOrEmpty(username))
            {
                var user = context.Users.SingleOrDefault(u => u.Id == username);
                if (user != null)
                {
                    var fullName = string.Concat(new string[] { user.FirstName, " ", user.LastName });
                    String Id = user.Id;
                    ViewBag.UserFullName = fullName;
                    ViewBag.ProfileUpdateId = Id;
                }
            }
            ViewBag.UserMail = UserMail;
            ApplicationUser userDetails = db.Users.Find(username);
            if (userDetails == null)
            {
                return HttpNotFound();
            }
            return View(userDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileUpdate(ApplicationUser userDetails, HttpPostedFileBase imageUpload)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser a = new ApplicationUser();
                a = db.Users.FirstOrDefault(f => f.Id == userDetails.Id);
                a.FirstName = userDetails.FirstName;
                a.LastName = userDetails.LastName;
                a.Gender = userDetails.Gender;

                if (imageUpload != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(imageUpload.FileName);
                    string extension = Path.GetExtension(imageUpload.FileName);
                    fileName = fileName + DateTime.Now.ToUniversalTime().AddHours(2).ToString("yymmssfff") + extension;
                    a.ImageUrl = "~/Uploads/Users/" + fileName;
                    fileName = Path.Combine(Server.MapPath("~/Uploads/Users/"), fileName);
                    imageUpload.SaveAs(fileName);
                }

                db.Entry(a).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(userDetails);
        }

        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }
    }
}