using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Rebound.Models;
using System;
using System.Threading.Tasks;

[assembly: OwinStartupAttribute(typeof(Rebound.Startup))]
namespace Rebound
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user 
                    // logs in. This is a security feature which is used when you 
                    // change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator
               .OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
               validateInterval: TimeSpan.FromDays(30),
               regenerateIdentity: (manager, user)
               => user.GenerateUserIdentityAsync(manager))
                }
            });

            createRoles();
        }
        private async void createRoles(){

                ApplicationDbContext context = new ApplicationDbContext();

                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();

                string[] roleData = { "systemAdmin", "Dashboard", "Add Reservation", "Paid Reservations", "Register user", "Set Roles", "Remove Roles", "Categories View", "Categories Create", "Categories Update", "Categories Delete", "Customer View", "Customer Create", "Customer Update", "Customer Delete", "Item View", "Item Create", "Item Update", "Item Delete" , "Operator View", "Operator Create", "Operator Update", "Operator Delete" , "Bills Report", "Cash Report", "Dues Report", "Cancel Report", "Audit Report", "Transaction Create" };

                foreach (var items in roleData)
                {
                    if (!roleManager.RoleExists(items))
                    {
                        role.Id = Guid.NewGuid().ToString();
                        role.Name = items;
                        await roleManager.CreateAsync(role);

                }
                }
                var user = new ApplicationUser();
                if (await UserManager.FindByNameAsync("system@domain.com") == null)
                {
                    user.UserName = "system@domain.com";
                    user.Email = "system@domain.com";
                    user.EmailConfirmed = true;
                    user.FirstName = "System ";
                    user.LastName = "Admin";
                    user.Gender = 0;
                    user.ImageUrl = "/Uploads/assets/user.jpg";
                    string userPWD = "system*#domain";
                    var chkUser = await UserManager.CreateAsync(user, userPWD);

                    //Add default User to Role Admin   
                    if (chkUser.Succeeded)
                    {
                         await UserManager.AddToRoleAsync(user.Id, "systemAdmin");
                    }
                }
            }

        
    }
}
