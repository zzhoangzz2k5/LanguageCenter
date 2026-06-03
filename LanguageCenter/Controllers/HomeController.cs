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
        var programs = db.Programs.ToList();
        return View(programs);
    }
}