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
            var profile = db.Profiles.FirstOrDefault();
            return View();
        }

    }
}