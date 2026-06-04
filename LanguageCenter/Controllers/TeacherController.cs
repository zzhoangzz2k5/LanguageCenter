using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using LanguageCenter.Models;
using System.IO;
using System.Web;

namespace LanguageCenter.Controllers
{
    public class TeacherController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        // Danh sách giáo viên công khai
        public ActionResult Index()
        {
            var teachers = db.UserAccounts
                             .Where(x => x.Role == "Teacher")
                             .ToList();

            return View(teachers);
        }

        // Dashboard giáo viên
        public ActionResult Dashboard()
        {
            if (Session["Role"] == null ||
                Session["Role"].ToString() != "Teacher")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public ActionResult Profile()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];

            var teacher = db.UserAccounts
                            .FirstOrDefault(x => x.UserId == userId);

            return View(teacher);
        }

        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase photo)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserId"];

            var teacher = db.UserAccounts
                            .FirstOrDefault(x => x.UserId == userId);

            if (photo != null && photo.ContentLength > 0)
            {
                string fileName =
                    userId +
                    Path.GetExtension(photo.FileName);

                string path =
                    Server.MapPath("~/Content/images/");

                photo.SaveAs(
                    Path.Combine(path, fileName));

                teacher.Photo = fileName;

                db.SubmitChanges();
            }

            return RedirectToAction("Profile");
        }
    }
}