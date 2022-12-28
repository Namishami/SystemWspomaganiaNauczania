using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using SystemWspomaganiaNauczania.Models;

namespace SystemWspomaganiaNauczania.DAL
{
    public class ProjectInitializer:DropCreateDatabaseIfModelChanges<ProjectContext>
    {
        protected override void Seed(ProjectContext context)
        {
         
            
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
             var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
             roleManager.Create(new IdentityRole("Admin"));
             roleManager.Create(new IdentityRole("User"));
        

           var user = new ApplicationUser { UserName = "admin@admin.pl", Email ="admin@admin.pl" };
           var password = "Admin123.";
            userManager.Create(user, password);
            userManager.AddToRole(user.Id, "Admin");
              var profile = new Profile
              {
                  Email = user.Email,
                  FirstName = "Admin",
                  LastName = "Admin"
              };

              context.Profiles.Add(profile);
            var font = new Models.FontStyle
            {
                Size = 12,
                FontFace = "Times New Roman",
                Color = "Gray",
                Profile= profile,
            };
            Configuration configuration = WebConfigurationManager.OpenWebConfiguration("~");
            AppSettingsSection appSettingsSection = (AppSettingsSection)configuration.GetSection("appSettings");
            KeyValueConfigurationCollection settings = appSettingsSection.Settings;
            settings.Add("FontName", font.FontFace);
            settings.Add("FontSize", font.Size + "px");
            settings.Add("FontColor", font.Color);
            configuration.Save();
            context.FontStyles.Add(font);

            context.SaveChanges();

        }
    }
}
