using System.Web.Mvc;

namespace LanguageCenter.Controllers
{
    public class TeacherController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Role"] == null ||
                Session["Role"].ToString() != "Teacher")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}