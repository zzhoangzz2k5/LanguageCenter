using LanguageCenter.Models;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace LanguageCenter.Controllers
{
    public class HomeController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        public ActionResult Index()
        {
            ViewBag.Classes = db.Classes.Take(6).ToList();

            var teachers =
            (
                from t in db.Teachers
                join u in db.UserAccounts
                on t.UserId equals u.UserId
                select new
                {
                    u.FullName,
                    t.Specialty,
                    t.ExperienceYears
                }
            ).Take(4).ToList();

            ViewBag.Teachers = teachers;

            return View(db.Programs.Take(6).ToList());
        }
    }
}