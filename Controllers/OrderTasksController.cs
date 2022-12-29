using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SystemWspomaganiaNauczania.DAL;
using SystemWspomaganiaNauczania.Models;
using SystemWspomaganiaNauczania.ViewModel;
using PagedList;

namespace SystemWspomaganiaNauczania.Controllers
{
    public class OrderTasksController : Controller
    {
        private ProjectContext db = new ProjectContext();


        // GET: OrderTasks
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            DeleteBlanc();

            var tempOrderTask = db.OrderTasks.Select(p => p);
            if (!String.IsNullOrEmpty(searchString))
                tempOrderTask = tempOrderTask.Where(s => s.Title.Contains(searchString));

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

        private void DeleteBlanc()
        {
            var wordId = db.WordInOrderTasks.Select(w => w.OrderTaskID).ToList();
            var tempOrderTask = db.OrderTasks.Select(p => p).ToList();

            if (wordId.Count() >= 0 && tempOrderTask.Count() > 0)
            {

                foreach (var item in tempOrderTask)
                {
                    if (!wordId.Contains(item.ID))
                    {
                        db.OrderTasks.Remove(item);
                        db.SaveChanges();
                    }
                }
            }
        }


        // GET: OrderTasks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderTask orderTask = db.OrderTasks.Find(id);
            Random rand = new Random();
            List<Word> taskWords = db.WordInOrderTasks.Where(w => w.OrderTaskID == id).Select(w => w.OrderTaskWord).ToList();
            for (var i = taskWords.Count - 1; i > 0; i--)
            {

                int j = int.Parse(Math.Floor(rand.NextDouble() * (i + 1)).ToString());
                var temp = taskWords[i];
                taskWords[i] = taskWords[j];
                taskWords[j] = temp;
            }


            ViewBag.taskWords = taskWords;

            if (orderTask == null)
            {
                return HttpNotFound();
            }
            Profile profile = db.Profiles.Single(p => p.Email == User.Identity.Name);
            ViewBag.profileID = profile.ID;
            ViewBag.userEmail = profile.Email;
            return View(orderTask);
        }

        // GET: OrderTasks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OrderTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Title,Description")] OrderTask orderTask)
        {
            if (ModelState.IsValid)
            {
                OrderTask tempOrderTask = new OrderTask();
                UpdateModel(tempOrderTask);

                HttpPostedFileBase file = Request.Files["imageOrderTaskFile"];
                if (file != null && file.ContentLength > 0)
                {
                    tempOrderTask.Image = file.FileName;
                    file.SaveAs(HttpContext.Server.MapPath("~/Images/") + tempOrderTask.Image);
                }
                tempOrderTask.Author = User.Identity.Name;
                db.OrderTasks.Add(tempOrderTask);
                db.SaveChanges();

                return RedirectToAction("CreateNextStep/" + tempOrderTask.ID);
            }

            return View(orderTask);
        }

        [HttpGet]
        public ActionResult CreateNextStep(int ID)
        {

            var errMsg = TempData["ErrorMessage"] as string;
            ViewBag.errMsg = errMsg;
            ViewBag.OrderTaskID = ID;
            return View(new OrderTaskWordViewModel());
        }

        [HttpPost]
        public ActionResult CreateNextStep(int ID, OrderTaskWordViewModel orderTaskWordViewModel)
        {
            if (ModelState.IsValid)
            {
                if (orderTaskWordViewModel.Names.All(w => w.Name == null))
                {
                    TempData["ErrorMessage"] = "Nie dodano żadnego wyrazu";
                    return RedirectToAction("CreateNextStep", ID);
                }
                else
                {
                    AddWordToTask(orderTaskWordViewModel.Names, ID);
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public ActionResult Solve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<int> orderTaskWordList = db.WordInOrderTasks.Where(w => w.OrderTaskID == id).Select(w => w.OrderTaskWordID).ToList();

            List<Word> taskWords = db.TaskWords.Where(w => orderTaskWordList.Contains(w.ID)).ToList();

            Random rand = new Random();

            for (var i = taskWords.Count - 1; i > 0; i--)
            {
                int j = int.Parse(Math.Floor(rand.NextDouble() * (i + 1)).ToString());
                var temp = taskWords[i];
                taskWords[i] = taskWords[j];
                taskWords[j] = temp;
            }

            if (taskWords == null)
            {
                return HttpNotFound();
            }

            OrderTaskWordViewModel viewModel = new OrderTaskWordViewModel();
            viewModel.Names = taskWords;
            ViewBag.TaskID = id;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Solve(int id, OrderTaskWordViewModel orderTaskWordViewModel)
        {
            if (ModelState.IsValid)
            {
                //zakładamy że jest poprawnie rozwiązane
                var correct = true;

                List<Word> orderTaskWordList = db.WordInOrderTasks.Where(w => w.OrderTaskID == id).Select(w => w.OrderTaskWord).ToList();
                OrderTask taskWord = db.OrderTasks.Single(w => w.ID == id);


                for (int i = 0; i < orderTaskWordList.Count; i++)
                {
                    if (orderTaskWordList[i].Name != orderTaskWordViewModel.Names[i].Name)
                    {
                        //zostało rozwiązane źle
                        correct = false;
                        break;
                    }
                }
                ViewBag.result = correct;

                var profileID = db.Profiles.Single(w => w.Email == User.Identity.Name).ID;
                var tempTask = db.OrderTaskSolved.FirstOrDefault(t => t.ProfileID == profileID && t.OrderTaskID == id);

                if (tempTask != null)
                {
                    if (tempTask.Solved != true)
                    {
                        tempTask.Solved = correct;
                        db.SaveChanges();
                    }
                }
                else
                {
                    OrderTaskSolved orderTaskSolved = new OrderTaskSolved();
                    orderTaskSolved.OrderTaskID = id;
                    orderTaskSolved.ProfileID = profileID;
                    orderTaskSolved.Solved = correct;
                    db.OrderTaskSolved.Add(orderTaskSolved);
                    db.SaveChanges();
                }

                return View("Check", taskWord);


            }
            return View();

        }

        // GET: OrderTasks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderTask orderTask = db.OrderTasks.Find(id);
            List<Word> taskWords = db.WordInOrderTasks.Where(w => w.OrderTaskID == id).Select(w => w.OrderTaskWord).ToList();
            ViewBag.taskWords = taskWords;
            if (orderTask == null)
            {
                return HttpNotFound();
            }
            return View(orderTask);
        }

        // POST: OrderTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Title,Description,Image")] OrderTask orderTask, OrderTaskWordViewModel orderTaskWord)
        {
            if (ModelState.IsValid)
            {
                if (orderTask.ID == 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                OrderTask tempOrderTask = db.OrderTasks.Single(g => g.ID == orderTask.ID);
                tempOrderTask.Title = orderTask.Title;
                tempOrderTask.Description = orderTask.Description;

                HttpPostedFileBase file = Request.Files["imageOrderTaskFile"];
                if (file != null && file.ContentLength > 0)
                {
                    tempOrderTask.Image = file.FileName;
                    file.SaveAs(HttpContext.Server.MapPath("~/Images/") + tempOrderTask.Image);
                }
                tempOrderTask.Author = User.Identity.Name;
                db.SaveChanges();

                List<WordInOrderTask> wordInOrderTasks = db.WordInOrderTasks.Where(w => w.OrderTaskID == tempOrderTask.ID).ToList();
                db.WordInOrderTasks.RemoveRange(wordInOrderTasks);
                db.SaveChanges();

                AddWordToTask(orderTaskWord.Names, tempOrderTask.ID);

                return RedirectToAction("Index");
            }
            return View(orderTask);
        }

        // GET: OrderTasks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderTask orderTask = db.OrderTasks.Find(id);
            if (orderTask == null)
            {
                return HttpNotFound();
            }
            return View(orderTask);
        }

        // POST: OrderTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderTask orderTask = db.OrderTasks.Find(id);
            List<WordInOrderTask> wordInOrderTask = db.WordInOrderTasks.Where(w => w.OrderTaskID == id).ToList();
            List<Comment> comments = db.Comments.Where(c => c.OrderTaskID == id).ToList();
            db.WordInOrderTasks.RemoveRange(wordInOrderTask);
            db.Comments.RemoveRange(comments);
            db.OrderTasks.Remove(orderTask);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void AddWordToTask(List<Word> wordList, int taskID)
        {
            if (wordList != null)
            {
                foreach (var item in wordList)
                {
                    if (db.TaskWords.Any(w => w.Name == item.Name))
                    {
                        int wordID = db.TaskWords.Single(c => c.Name == item.Name).ID;

                        WordInOrderTask wordInOrderTask = new WordInOrderTask();
                        wordInOrderTask.OrderTaskID = taskID;
                        wordInOrderTask.OrderTaskWordID = wordID;
                        db.WordInOrderTasks.Add(wordInOrderTask);
                        db.SaveChanges();
                    }
                    else
                    {
                        if (item.Name != null)
                        {
                            Word word = new Word();
                            word.Name = item.Name;
                            db.TaskWords.Add(word);
                            db.SaveChanges();

                            WordInOrderTask wordInOrderTask = new WordInOrderTask();
                            wordInOrderTask.OrderTaskID = taskID;
                            wordInOrderTask.OrderTaskWordID = word.ID;
                            db.WordInOrderTasks.Add(wordInOrderTask);
                            db.SaveChanges();
                        }
                    }
                }
            }
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
