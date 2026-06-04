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

            ViewBag.TotalRegistrations = db.Registrations.Count();
            ViewBag.TotalPayments = db.Payments.Count();
            ViewBag.TotalPlacementTests = db.PlacementTests.Count();

            ViewBag.PendingTeachers =
                db.UserAccounts.Count(x => x.Role == "PendingTeacher");

            Response.Cache.SetCacheability(
                System.Web.HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            return View();
        }

        public ActionResult TeacherRequests()
        {
            var teachers = db.UserAccounts
                             .Where(x => x.Role == "PendingTeacher")
                             .ToList();

            return View(teachers);
        }

        public ActionResult ApproveTeacher(int id)
        {
            var user =
                db.UserAccounts
                  .FirstOrDefault(x =>
                  x.UserId == id);

            user.Role = "Teacher";

            Teacher teacher = new Teacher();

            teacher.UserId = user.UserId;

            teacher.Specialty = "General";

            teacher.ExperienceYears = 0;

            db.Teachers.InsertOnSubmit(teacher);

            db.SubmitChanges();

            return RedirectToAction("TeacherRequests");
        }

        public ActionResult Students()
        {
            var students =
                from s in db.Students
                join u in db.UserAccounts
                on s.UserId equals u.UserId
                select new StudentViewModel
                {
                    StudentId = s.StudentId,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = (bool)u.IsActive
                };

            return View(students.ToList());
        }

        public ActionResult DeactivateStudent(int id)
        {
            var student =
                db.Students.FirstOrDefault(x => x.StudentId == id);

            if (student != null)
            {
                var user =
                    db.UserAccounts.FirstOrDefault(x =>
                    x.UserId == student.UserId);

                user.IsActive = false;

                db.SubmitChanges();
            }

            return RedirectToAction("Students");
        }

        public ActionResult ActivateStudent(int id)
        {
            var student =
                db.Students.FirstOrDefault(x => x.StudentId == id);

            if (student != null)
            {
                var user =
                    db.UserAccounts.FirstOrDefault(x =>
                    x.UserId == student.UserId);

                user.IsActive = true;

                db.SubmitChanges();
            }

            return RedirectToAction("Students");
        }

        public ActionResult Teachers()
        {
            var teachers =
                from t in db.Teachers
                join u in db.UserAccounts
                on t.UserId equals u.UserId
                select new TeacherViewModel
                {
                    TeacherId = t.TeacherId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Specialty = t.Specialty,
                    ExperienceYears = t.ExperienceYears,
                    IsActive = (bool)u.IsActive
                };

            return View(teachers.ToList());
        }

        public ActionResult DeactivateTeacher(int id)
        {
            var teacher =
                db.Teachers.FirstOrDefault(x => x.TeacherId == id);

            if (teacher != null)
            {
                var user =
                    db.UserAccounts.FirstOrDefault(x =>
                    x.UserId == teacher.UserId);

                user.IsActive = false;

                db.SubmitChanges();
            }

            return RedirectToAction("Teachers");
        }

        public ActionResult ActivateTeacher(int id)
        {
            var teacher =
                db.Teachers.FirstOrDefault(x => x.TeacherId == id);

            if (teacher != null)
            {
                var user =
                    db.UserAccounts.FirstOrDefault(x =>
                    x.UserId == teacher.UserId);

                user.IsActive = true;

                db.SubmitChanges();
            }

            return RedirectToAction("Teachers");
        }

        public ActionResult Programs()
        {
            return View(db.Programs.ToList());
        }


        public ActionResult CreateProgram()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateProgram(Program p)
        {
            db.Programs.InsertOnSubmit(p);

            db.SubmitChanges();

            return RedirectToAction("Programs");
        }


        public ActionResult EditProgram(int id)
        {
            var program =
                db.Programs.FirstOrDefault(x =>
                x.ProgramId == id);

            return View(program);
        }

        [HttpPost]
        public ActionResult EditProgram(Program p)
        {
            var program =
                db.Programs.FirstOrDefault(x =>
                x.ProgramId == p.ProgramId);

            program.ProgramName = p.ProgramName;
            program.LevelName = p.LevelName;
            program.DurationMonths = p.DurationMonths;
            program.Fee = p.Fee;
            program.Description = p.Description;

            db.SubmitChanges();

            return RedirectToAction("Programs");
        }


        public ActionResult DeleteProgram(int id)
        {
            var program =
                db.Programs.FirstOrDefault(x =>
                x.ProgramId == id);

            bool hasClass =
                db.Classes.Any(x =>
                x.ProgramId == id);

            if (hasClass)
            {
                program.ProgramStatus = false;

                db.SubmitChanges();

                TempData["Error"] =
                    "Program is being used by classes. Status changed to Inactive.";
            }
            else
            {
                db.Programs.DeleteOnSubmit(program);

                db.SubmitChanges();
            }

            return RedirectToAction("Programs");
        }

        public ActionResult ActivateProgram(int id)
        {
            var program =
                db.Programs.FirstOrDefault(x =>
                x.ProgramId == id);

            if (program != null)
            {
                program.ProgramStatus = true;

                db.SubmitChanges();
            }

            return RedirectToAction("Programs");
        }
    }
}