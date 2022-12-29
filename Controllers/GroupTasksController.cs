using PagedList;
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

namespace SystemWspomaganiaNauczania.Controllers
{
    public class GroupTasksController : Controller
    {
        private ProjectContext db = new ProjectContext();

        // GET: GroupTasks
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

            var tempGroupTask = from p in db.GroupTasks
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


        // GET: GroupTasks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GroupTask groupTask = db.GroupTasks.Find(id);
            if (groupTask == null)
            {
                return HttpNotFound();
            }

            List<Word> taskWords = db.GroupTaskWords.Where(w => w.GroupTaskID == id).Select(w => w.Word).ToList();
            ViewBag.taskWords = taskWords;
            Profile profile = db.Profiles.Single(p => p.Email == User.Identity.Name);

            ViewBag.profileID = profile.ID;

            ViewBag.userEmail = profile.Email;
            return View(groupTask);
        }

        // GET: GroupTasks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: GroupTasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Title,Description")] GroupTask groupTask)
        {
            if (ModelState.IsValid)
            {
                GroupTask tempGroupTask = new GroupTask();
                UpdateModel(tempGroupTask);

                HttpPostedFileBase file = Request.Files["imageGroupTaskFile"];
                if (file != null && file.ContentLength > 0)
                {
                    tempGroupTask.Image = file.FileName;
                    file.SaveAs(HttpContext.Server.MapPath("~/Images/") + tempGroupTask.Image);
                }
                tempGroupTask.Author = User.Identity.Name;
                db.GroupTasks.Add(tempGroupTask);
                db.SaveChanges();

                return RedirectToAction("CreateNextStep/" + tempGroupTask.ID);
            }

            return View(groupTask);
        }


        [HttpGet]
        public ActionResult CreateNextStep(int ID)
        {
            ViewBag.GroupTaskID = ID;
            return View(new GroupTaskWordViewModel());
        }

        [HttpPost]
        public ActionResult CreateNextStep(int ID, GroupTaskWordViewModel groupTaskWordViewModel)
        {
            if (ModelState.IsValid)
            {
                CreateAreaName(groupTaskWordViewModel.AreaName);

                string firstContainerName = groupTaskWordViewModel.AreaName[0];
                int containerID = db.Container.Single(c => c.ContainerName == firstContainerName).ID;
                AddWordToTask(groupTaskWordViewModel.Wordslist1, containerID, ID);


                string secondContainerName = groupTaskWordViewModel.AreaName[1];
                containerID = db.Container.Single(c => c.ContainerName == secondContainerName).ID;
                AddWordToTask(groupTaskWordViewModel.WordsList2, containerID, ID);

                string thirdContainerName = groupTaskWordViewModel.AreaName[2];
                containerID = db.Container.Single(c => c.ContainerName == thirdContainerName).ID;
                AddWordToTask(groupTaskWordViewModel.WordsList3, containerID, ID);


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

            List<int> groupTaskWordList = db.GroupTaskWords.Where(w => w.GroupTaskID == id).Select(w => w.WordID).ToList();

            List<Word> taskWords = db.TaskWords.Where(w => groupTaskWordList.Contains(w.ID)).ToList();

            List<string> containers = db.GroupTaskWords.Where(w => w.GroupTaskID == id).Select(w => w.Container).Distinct().OrderBy(w => w.ContainerNumber).Select(w => w.ContainerName).ToList();


            if (taskWords == null || containers == null)
            {
                return HttpNotFound();
            }

            Random rand = new Random();

            for (var i = taskWords.Count - 1; i > 0; i--)
            {
                int j = int.Parse(Math.Floor(rand.NextDouble() * (i + 1)).ToString());
                var temp = taskWords[i];
                taskWords[i] = taskWords[j];
                taskWords[j] = temp;
            }



            GroupTaskWordViewModel viewModel = new GroupTaskWordViewModel();
            viewModel.Wordslist1 = taskWords;
            viewModel.AreaName = containers;
            ViewBag.TaskID = id;

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult Solve(int ID, GroupTaskWordViewModel groupTaskWordViewModel)
        {
            if (ModelState.IsValid)
            {

                var task = db.GroupTasks.Single(t => t.ID == ID);

                var groupTaskWord1 = db.GroupTaskWords.Where(w => w.GroupTaskID == ID && w.Container.ContainerNumber == 1).Select(w => w.Word.Name).ToList();
                var groupTaskWord2 = db.GroupTaskWords.Where(w => w.GroupTaskID == ID && w.Container.ContainerNumber == 2).Select(w => w.Word.Name).ToList();
                var groupTaskWord3 = db.GroupTaskWords.Where(w => w.GroupTaskID == ID && w.Container.ContainerNumber == 3).Select(w => w.Word.Name).ToList();

                if (groupTaskWordViewModel.Wordslist1 != null && groupTaskWordViewModel.Wordslist1.Count > 0 ||
                    groupTaskWordViewModel.WordsList2 != null && groupTaskWordViewModel.WordsList2.Count > 0 ||
                    groupTaskWordViewModel.WordsList3 != null && groupTaskWordViewModel.WordsList3.Count > 0)
                {
                    if (!Check(groupTaskWordViewModel.Wordslist1.Select(w => w.Name).ToList(), groupTaskWord1) ||
                        !Check(groupTaskWordViewModel.WordsList2.Select(w => w.Name).ToList(), groupTaskWord2) ||
                        !Check(groupTaskWordViewModel.WordsList3.Select(w => w.Name).ToList(), groupTaskWord3))
                    {
                        //źle
                        ViewBag.result = false;
                    }
                    else
                    {
                        //dobrze
                        ViewBag.result = true;
                    }

                }
                else
                {
                    //źle
                    ViewBag.result = false;
                }


                var profileID = db.Profiles.Single(w => w.Email == User.Identity.Name).ID;
                var tempTask = db.GroupTaskSolved.FirstOrDefault(t => t.ProfileID == profileID && t.GroupTaskID == ID);

                if (tempTask != null)
                {
                    if (tempTask.Solved != true)
                    {
                        tempTask.Solved = ViewBag.result;
                        db.SaveChanges();
                    }
                }
                else
                {
                    GroupTaskSolved groupTaskSolved = new GroupTaskSolved();
                    groupTaskSolved.GroupTaskID = ID;
                    groupTaskSolved.ProfileID = profileID;
                    groupTaskSolved.Solved = ViewBag.result;
                    db.GroupTaskSolved.Add(groupTaskSolved);
                    db.SaveChanges();
                }
                return View("Check", task);
            }
            return View();
        }

        // GET: GroupTasks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GroupTask groupTask = db.GroupTasks.Find(id);

            ViewBag.WordList1 = db.GroupTaskWords.Where(w => w.GroupTaskID == id && w.Container.ContainerNumber == 1).Select(w => w.Word).ToList();
            ViewBag.WordList2 = db.GroupTaskWords.Where(w => w.GroupTaskID == id && w.Container.ContainerNumber == 2).Select(w => w.Word).ToList();
            ViewBag.WordList3 = db.GroupTaskWords.Where(w => w.GroupTaskID == id && w.Container.ContainerNumber == 3).Select(w => w.Word).ToList();
            ViewBag.AreaName = db.GroupTaskWords.Where(w => w.GroupTaskID == id).Select(w => w.Container).Distinct().OrderBy(w => w.ContainerNumber).Select(w => w.ContainerName).ToList();

            if (groupTask == null)
            {
                return HttpNotFound();
            }
            return View(groupTask);
        }

        // POST: GroupTasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Title,Description")] GroupTask groupTask, GroupTaskWordViewModel groupTaskWord)
        {
            if (ModelState.IsValid)
            {
                GroupTask tempGroupTask = db.GroupTasks.Single(g => g.ID == groupTask.ID);
                tempGroupTask.Title = groupTask.Title;
                tempGroupTask.Description = groupTask.Description;
                HttpPostedFileBase file = Request.Files["imageGroupTaskFile"];
                if (file != null && file.ContentLength > 0)
                {
                    tempGroupTask.Image = file.FileName;
                    file.SaveAs(HttpContext.Server.MapPath("~/Images/") + tempGroupTask.Image);
                }
                tempGroupTask.Author = User.Identity.Name;
                db.SaveChanges();

                List<GroupTaskWord> taskWord = db.GroupTaskWords.Where(w => w.GroupTaskID == tempGroupTask.ID).ToList();

                db.GroupTaskWords.RemoveRange(taskWord);
                db.SaveChanges();

                CreateAreaName(groupTaskWord.AreaName);

                string firstContainerName = groupTaskWord.AreaName[0];
                int containerID = db.Container.Single(c => c.ContainerName == firstContainerName).ID;
                AddWordToTask(groupTaskWord.Wordslist1, containerID, tempGroupTask.ID);


                string secondContainerName = groupTaskWord.AreaName[1];
                containerID = db.Container.Single(c => c.ContainerName == secondContainerName).ID;
                AddWordToTask(groupTaskWord.WordsList2, containerID, tempGroupTask.ID);

                string thirdContainerName = groupTaskWord.AreaName[2];
                containerID = db.Container.Single(c => c.ContainerName == thirdContainerName).ID;
                AddWordToTask(groupTaskWord.WordsList3, containerID, tempGroupTask.ID);

                return RedirectToAction("Index");
            }
            return View(groupTask);
        }

        // GET: GroupTasks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GroupTask groupTask = db.GroupTasks.Find(id);
            if (groupTask == null)
            {
                return HttpNotFound();
            }
            return View(groupTask);
        }

        // POST: GroupTasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            GroupTask groupTask = db.GroupTasks.Find(id);
            List<GroupTaskWord> groupTasks = db.GroupTaskWords.Where(w => w.GroupTaskID == id).ToList();
            List<Comment> comments = db.Comments.Where(c => c.GroupTaskID == id).ToList();
            db.GroupTaskWords.RemoveRange(groupTasks);
            db.Comments.RemoveRange(comments);
            db.GroupTasks.Remove(groupTask);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        private bool Check(List<string> first, List<string> second)
        {
            var result = true;
            if (first != null && second != null)
            {
                foreach (var item in first)
                {
                    if (second.Contains(item))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
        }
        private void CreateAreaName(List<string> areaName)
        {
            int iterator = 1;
            foreach (var item in areaName)
            {
                if (!db.Container.Any(w => w.ContainerName == item && w.ContainerNumber == iterator))
                {
                    Container container = new Container();
                    container.ContainerNumber = iterator;
                    container.ContainerName = item;
                    db.Container.Add(container);
                    db.SaveChanges();
                }
                iterator++;
            }
        }
        private void AddWordToTask(List<Word> wordList, int containerID, int taskID)
        {
            if (wordList != null)
            {
                foreach (var item in wordList)
                {
                    if (db.TaskWords.Any(w => w.Name == item.Name))
                    {
                        int wordID = db.TaskWords.Single(c => c.Name == item.Name).ID;

                        GroupTaskWord groupTaskWord = new GroupTaskWord();
                        groupTaskWord.WordID = wordID;
                        groupTaskWord.GroupTaskID = taskID;
                        groupTaskWord.ContainerID = containerID;
                        db.GroupTaskWords.Add(groupTaskWord);
                        db.SaveChanges();
                    }
                    else
                    {
                        Word word = new Word();
                        word.Name = item.Name;
                        db.TaskWords.Add(word);
                        db.SaveChanges();


                        GroupTaskWord groupTaskWord = new GroupTaskWord();
                        groupTaskWord.WordID = word.ID;
                        groupTaskWord.GroupTaskID = taskID;
                        groupTaskWord.ContainerID = containerID;
                        db.GroupTaskWords.Add(groupTaskWord);
                        db.SaveChanges();

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
