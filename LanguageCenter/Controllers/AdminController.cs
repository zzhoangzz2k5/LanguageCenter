using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using LanguageCenter.Models;

namespace LanguageCenter.Controllers
{
    public class AdminController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        public ActionResult Index()
        {
            if (Session["Role"] == null ||
                Session["Role"].ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalPrograms = db.Programs.Count();
            ViewBag.TotalClasses = db.Classes.Count();
            ViewBag.TotalStudents = db.Students.Count();
            ViewBag.TotalTeachers = db.Teachers.Count();

            return View();
        }
    }
}