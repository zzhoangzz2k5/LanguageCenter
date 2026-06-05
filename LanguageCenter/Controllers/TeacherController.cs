using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using LanguageCenter.Models;

namespace LanguageCenter.Controllers
{
    public class TeacherController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        public ActionResult Index()
        {
            var teachers = db.UserAccounts
                             .Where(x => x.Role == "Teacher")
                             .ToList();

            return View(teachers);
        }

        public ActionResult Dashboard()
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            var classes = db.Classes.Where(x => x.TeacherId == teacher.TeacherId).ToList();

            ViewBag.TotalClasses = classes.Count;
            ViewBag.TotalStudents = db.Registrations.Count(x => x.Class != null && x.Class.TeacherId == teacher.TeacherId);
            ViewBag.TeachingSchedule = classes.OrderBy(x => x.StartDate).Take(5).ToList();
            ViewBag.RecentActivities = db.Registrations
                                         .Where(x => x.Class != null && x.Class.TeacherId == teacher.TeacherId)
                                         .OrderByDescending(x => x.RegistrationDate)
                                         .Take(5)
                                         .ToList();

            return View(teacher);
        }

        public ActionResult MyClasses()
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            return View(db.Classes
                          .Where(x => x.TeacherId == teacher.TeacherId)
                          .OrderBy(x => x.StartDate)
                          .ToList());
        }

        public ActionResult ClassStudents(int id)
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            var classItem = db.Classes.FirstOrDefault(x => x.ClassId == id && x.TeacherId == teacher.TeacherId);

            if (classItem == null)
                return HttpNotFound();

            ViewBag.ClassName = classItem.ClassName;

            return View(db.Registrations
                          .Where(x => x.ClassId == id)
                          .OrderByDescending(x => x.RegistrationDate)
                          .ToList());
        }

        public ActionResult Materials(int? classId)
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Classes = db.Classes
                                .Where(x => x.TeacherId == teacher.TeacherId)
                                .OrderBy(x => x.ClassName)
                                .ToList();
            ViewBag.SelectedClassId = classId;

            string folder = Server.MapPath("~/Content/materials/");
            Directory.CreateDirectory(folder);

            string prefix = classId.HasValue ? "class-" + classId.Value + "-" : "";
            var files = Directory.GetFiles(folder)
                                 .Where(x => string.IsNullOrEmpty(prefix) || Path.GetFileName(x).StartsWith(prefix))
                                 .Select(Path.GetFileName)
                                 .OrderBy(x => x)
                                 .ToList();

            return View(files);
        }

        [HttpPost]
        public ActionResult UploadMaterial(int classId, HttpPostedFileBase document)
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            bool ownsClass = db.Classes.Any(x => x.ClassId == classId && x.TeacherId == teacher.TeacherId);

            if (!ownsClass)
                return HttpNotFound();

            if (document != null && document.ContentLength > 0)
            {
                string folder = Server.MapPath("~/Content/materials/");
                Directory.CreateDirectory(folder);

                string fileName = "class-" + classId + "-" + Path.GetFileName(document.FileName);
                document.SaveAs(Path.Combine(folder, fileName));
                TempData["Message"] = "Material uploaded successfully.";
            }

            return RedirectToAction("Materials", new { classId = classId });
        }

        public ActionResult DeleteMaterial(string fileName, int? classId)
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            if (!string.IsNullOrEmpty(fileName))
            {
                string safeName = Path.GetFileName(fileName);
                string path = Path.Combine(Server.MapPath("~/Content/materials/"), safeName);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            return RedirectToAction("Materials", new { classId = classId });
        }

        public ActionResult PlacementResults()
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            return View(db.PlacementTests
                          .OrderByDescending(x => x.TestDate)
                          .ToList());
        }

        public ActionResult Feedback()
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            return View(db.Consultations
                          .OrderByDescending(x => x.CreatedDate)
                          .ToList());
        }

        public new ActionResult Profile()
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
                    "teacher-" +
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

        private Teacher GetCurrentTeacher()
        {
            if (Session["Role"] == null ||
                Session["Role"].ToString() != "Teacher" ||
                Session["UserId"] == null)
            {
                return null;
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            return db.Teachers.FirstOrDefault(x => x.UserId == userId);
        }
    }
}
