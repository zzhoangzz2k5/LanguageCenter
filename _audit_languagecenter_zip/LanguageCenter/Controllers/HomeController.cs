using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using LanguageCenter.Models;

namespace LanguageCenter.Controllers
{
    public class HomeController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        public ActionResult Index()
        {
            ViewBag.Programs = db.Programs.Take(6).ToList();

            ViewBag.Classes = db.Classes
                                .OrderByDescending(x => x.StartDate)
                                .Take(5)
                                .ToList();

            ViewBag.Teachers = db.UserAccounts
                                 .Where(x => x.Role == "Teacher")
                                 .Take(6)
                                 .ToList();

            // Statistics
            ViewBag.StudentCount = db.Students.Count();

            ViewBag.TeacherCount =
                db.UserAccounts.Count(x => x.Role == "Teacher");

            ViewBag.ClassCount = db.Classes.Count();

            ViewBag.ProgramCount = db.Programs.Count();

            return View();
        }

    }
}
