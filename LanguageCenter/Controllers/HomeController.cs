using LanguageCenter.Models;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

public class HomeController : Controller
{
    LanguageCenterDataContext db =
        new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

    public ActionResult Index()
    {
        var programs = db.Programs.Take(6).ToList();
        var classes = db.Classes.Take(6).ToList();
        var teachers = db.Teachers.Take(4).ToList();

        ViewBag.Classes = classes;
        ViewBag.Teachers = teachers;

        return View(programs);
    }
}