using System.Linq;
using System.Web.Mvc;
using SystemWspomaganiaNauczania.DAL;

namespace SystemWspomaganiaNauczania.Controllers
{
    public class HomeController : Controller
    {
        ProjectContext db = new ProjectContext();
        public ActionResult Index()
        {
            db.Profiles.FirstOrDefault();
            return View();
        }
        public ActionResult InformationPage()
        {
            return View();
        }
    }
}