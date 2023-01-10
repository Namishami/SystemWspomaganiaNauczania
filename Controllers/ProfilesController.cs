using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PagedList;
using System;
using System.Configuration;
using System.Data;
using System.EnterpriseServices;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Services.Protocols;
using SystemWspomaganiaNauczania.DAL;
using SystemWspomaganiaNauczania.Exceptions;
using SystemWspomaganiaNauczania.Models;
using SystemWspomaganiaNauczania.ViewModel;

namespace SystemWspomaganiaNauczania.Controllers
{
    public class ProfilesController : Controller
    {
        private ProjectContext db = new ProjectContext();
        private ApplicationUserManager _userManager;

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

        [SecurityRole("Admin")]
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var tempProfiles = from p in db.Profiles
                                select p;
            if (!String.IsNullOrEmpty(searchString))
            {
                tempProfiles = tempProfiles.Where(s => s.FirstName.Contains(searchString) || s.LastName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tempProfiles = tempProfiles.OrderBy(s => s.FirstName);
                    break;
                default:
                    tempProfiles = tempProfiles.OrderBy(s => s.FirstName);
                    break;
            }

            int pageSize = 12;
            int pageNumber = (page ?? 1);


            return View(tempProfiles.ToPagedList(pageNumber, pageSize));
        }

        // GET: Profiles/Details/5
        public ActionResult Details(int? id)
        {
            var profile = db.Profiles.Single(p => p.ID == id);
            return View(profile);
        }

        public ActionResult Options()
        {
            var profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);
            if (profile == null)
                throw new NullProfileException("Nie znaleziono zalogowanego użytkownika");
            var style = db.FontStyles.FirstOrDefault(s => s.Profile.Email == profile.Email);
            return View(style);
        }

        [HttpPost]
        public ActionResult Options([Bind(Include = "ID,Size,FontFace,Color")] Models.FontStyle fontStyle)
        {
            if (ModelState.IsValid)
            {
                var profile = db.Profiles.FirstOrDefault(p => p.Email == User.Identity.Name);
                if (profile == null)
                    throw new NullProfileException("Nie znaleziono zalogowanego użytkownika");

                var style = db.FontStyles.FirstOrDefault(s => s.Profile.Email == profile.Email);
                if (style == null)
                {
                    fontStyle.Profile = profile;
                    db.FontStyles.Add(fontStyle);
                }
                else
                {
                    style.FontFace = fontStyle.FontFace;
                    style.Size = fontStyle.Size;
                    style.Color = fontStyle.Color;
                }
                Configuration configuration = WebConfigurationManager.OpenWebConfiguration("~");
                AppSettingsSection appSettingsSection = (AppSettingsSection)configuration.GetSection("appSettings");
                KeyValueConfigurationCollection settings = appSettingsSection.Settings;

                var allKeys = settings.AllKeys;

                if (!allKeys.Any(p => p.Equals("FontName")) || !allKeys.Any(p => p.Equals("FontSize")) || !allKeys.Any(p => p.Equals("FontColor")))
                {
                    settings.Add("FontName", fontStyle.FontFace);
                    settings.Add("FontSize", fontStyle.Size + "px");
                    settings.Add("FontColor", fontStyle.Color);
                }
                else
                {
                    settings["FontName"].Value = fontStyle.FontFace;
                    settings["FontSize"].Value = fontStyle.Size + "px";
                    settings["FontColor"].Value = fontStyle.Color;

                }
                configuration.Save();
                db.SaveChanges();
                return RedirectToAction("../Home/Index");
            }
            return View(fontStyle);
        }

        // GET: Profiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profile profile = db.Profiles.Find(id);
            if (profile == null)
            {
                return HttpNotFound();
            }
            return View(profile);
        }

        // POST: Profiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Email,FirstName,LastName")] Profile profile)
        {
            if (ModelState.IsValid)
            {
                Profile tempProfile = db.Profiles.Single(p => p.ID == profile.ID);
                tempProfile.FirstName = profile.FirstName;
                tempProfile.LastName = profile.LastName;

                HttpPostedFileBase file = Request.Files["ProfileImage"];
                if (file != null && file.ContentLength > 0)
                {
                    tempProfile.Image = file.FileName;
                    file.SaveAs(HttpContext.Server.MapPath("~/Images/") + tempProfile.Image);
                }
                db.SaveChanges();
                return RedirectToAction("Details");
            }
            return View(profile);
        }

        public ActionResult MyOrderTask(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var tempOrderTask = from p in db.OrderTasks
                                where p.Author == User.Identity.Name
                                select p;
            if (!String.IsNullOrEmpty(searchString))
            {
                tempOrderTask = tempOrderTask.Where(s => s.Title.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tempOrderTask = tempOrderTask.OrderBy(s => s.Title);
                    break;
                default:
                    tempOrderTask = tempOrderTask.OrderBy(s => s.Title);
                    break;
            }

            int pageSize = 12;
            int pageNumber = (page ?? 1);


            return View(tempOrderTask.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult MyGroupTask(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var tempGroupTask = from p in db.GroupTasks
                                where p.Author == User.Identity.Name
                                select p;
            if (!String.IsNullOrEmpty(searchString))
            {
                tempGroupTask = tempGroupTask.Where(s => s.Title.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tempGroupTask = tempGroupTask.OrderBy(s => s.Title);
                    break;
                default:
                    tempGroupTask = tempGroupTask.OrderBy(s => s.Title);
                    break;
            }

            int pageSize = 12;
            int pageNumber = (page ?? 1);

            return View(tempGroupTask.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult OrderTaskSolved(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;


            var tempOrderTask = db.OrderTaskSolved.Where(p => p.Profile.Email == User.Identity.Name).Select(p => new SolvedViewModel { OrderTask = p.OrderTask, Solved = p.Solved });


            if (!String.IsNullOrEmpty(searchString))
            {
                tempOrderTask = tempOrderTask.Where(s => s.OrderTask.Title.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tempOrderTask = tempOrderTask.OrderBy(s => s.OrderTask.Title);
                    break;
                default:
                    tempOrderTask = tempOrderTask.OrderBy(s => s.OrderTask.Title);
                    break;
            }

            int pageSize = 12;
            int pageNumber = (page ?? 1);

            return View(tempOrderTask.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult GroupTaskSolved(string sortOrder, string currentFilter, string searchString, int? page)
        {

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var tempGroupTask = db.GroupTaskSolved.Where(p => p.Profile.Email == User.Identity.Name).Select(p => new SolvedViewModel { GroupTask = p.GroupTask, Solved = p.Solved });
            if (!String.IsNullOrEmpty(searchString))
            {
                tempGroupTask = tempGroupTask.Where(s => s.GroupTask.Title.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tempGroupTask = tempGroupTask.OrderBy(s => s.GroupTask.Title);
                    break;
                default:
                    tempGroupTask = tempGroupTask.OrderBy(s => s.GroupTask.Title);
                    break;
            }

            int pageSize = 12;
            int pageNumber = (page ?? 1);

            return View(tempGroupTask.ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            Profile profile = db.Profiles.Single(p => p.ID == id);
            if (profile == null)
                throw new NullProfileException();
            return View(profile);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (ModelState.IsValid)
            {
                Profile tempProfile = db.Profiles.Single(p => p.ID == id);
                if(tempProfile != null)
                {
                    UserManager.Delete(UserManager.FindByEmail(tempProfile.Email));
                    var fonts = db.FontStyles.FirstOrDefault(p => p.Profile.ID == tempProfile.ID);
                    db.FontStyles.Remove(fonts);
                    db.Profiles.Remove(tempProfile);
                    db.SaveChanges();
                    return RedirectToAction("Index");
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
