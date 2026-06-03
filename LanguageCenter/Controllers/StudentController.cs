using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using LanguageCenter.Models;

namespace LanguageCenter.Controllers
{
    public class StudentController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        public ActionResult Index()
        {
            if (Session["Role"] == null ||
                Session["Role"].ToString() != "Student")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}