using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SystemWspomaganiaNauczania.DAL;
using SystemWspomaganiaNauczania.Models;

namespace SystemWspomaganiaNauczania.Controllers
{
    public class CommentsController : Controller
    {
        private ProjectContext db = new ProjectContext();

        // GET: Comments
        public ActionResult Index()
        {
            var comments = db.Comments.Include(c => c.Profile);
            return View(comments.ToList());
        }
    
        // GET: Comments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        public ActionResult Create(int? ID)
        {
            Profile profile = db.Profiles.Single(p => p.Email == User.Identity.Name);
            ViewBag.ProfileID = profile.ID;
            ViewBag.QuestionID = ID;
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Content,ProfileID,OrderTaskID,GroupTaskID")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Author = User.Identity.Name;
                db.Comments.Add(comment);
                db.SaveChanges();

                if (comment.OrderTaskID != null)
                {
                    return RedirectToAction("Details", "OrderTasks", new { ID = comment.OrderTaskID });

                }else if (comment.GroupTaskID != null)
                {
                    return RedirectToAction("Details", "GroupTasks", new { ID = comment.GroupTaskID });
                }
                return RedirectToAction("Index","Home");
            }

           
            return View(comment);
        }

        // GET: Comments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProfileID = new SelectList(db.Profiles, "ID", "Email", comment.ProfileID);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Content,ProfileID,QuestionID")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProfileID = new SelectList(db.Profiles, "ID", "Email", comment.ProfileID);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            int? commentGroupTaskID = comment.GroupTaskID;
            int? commentOrderTaskID = comment.OrderTaskID;

            db.Comments.Remove(comment);
            db.SaveChanges();
            if (commentOrderTaskID != null)
            {
                return RedirectToAction("Details", "OrderTasks", new { ID = commentOrderTaskID });
            }
            else if (commentGroupTaskID != null)
            {
                return RedirectToAction("Details", "GroupTasks", new { ID = commentGroupTaskID });
            }
            return RedirectToAction("Index", "Home");
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
