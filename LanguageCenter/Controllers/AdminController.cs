using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewBag.TotalPrograms = db.Programs.Count();
            ViewBag.TotalClasses = db.Classes.Count();
            ViewBag.TotalStudents = db.Students.Count();
            ViewBag.TotalTeachers = db.Teachers.Count();
            ViewBag.TotalRevenue = db.Payments.Where(x => x.PaymentStatus == "Paid" || x.PaymentStatus == "Confirmed").Sum(x => x.Amount) ?? 0;
            ViewBag.TotalRegistrations = db.Registrations.Count();
            ViewBag.TotalPayments = db.Payments.Count();
            ViewBag.TotalPlacementTests = db.PlacementTests.Count();
            ViewBag.PendingTeachers = db.UserAccounts.Count(x => x.Role == "Pending" || x.Role == "PendingTeacher");
            ViewBag.TopClasses = db.Classes.OrderByDescending(x => x.Registrations.Count).Take(5).ToList();
            ViewBag.RecentPayments = db.Payments.OrderByDescending(x => x.PaymentDate).Take(5).ToList();

            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return View();
        }

        public ActionResult RegistrationRequests()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var requests = db.UserAccounts
                             .Where(x => x.Role == "Pending" || x.Role == "PendingTeacher")
                             .ToList();

            return View(requests);
        }

        public ActionResult ApproveAsStudent(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = db.UserAccounts.FirstOrDefault(x => x.UserId == id);

            if (user != null)
            {
                user.Role = "Student";

                if (!db.Students.Any(x => x.UserId == user.UserId))
                {
                    Student student = new Student();
                    student.UserId = user.UserId;
                    db.Students.InsertOnSubmit(student);
                }

                db.SubmitChanges();
            }

            return RedirectToAction("RegistrationRequests");
        }

        public ActionResult ApproveAsTeacher(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = db.UserAccounts.FirstOrDefault(x => x.UserId == id);

            if (user != null)
            {
                user.Role = "Teacher";

                if (!db.Teachers.Any(x => x.UserId == user.UserId))
                {
                    Teacher teacher = new Teacher();
                    teacher.UserId = user.UserId;
                    teacher.Specialty = "General";
                    teacher.ExperienceYears = 0;
                    db.Teachers.InsertOnSubmit(teacher);
                }

                db.SubmitChanges();
            }

            return RedirectToAction("RegistrationRequests");
        }

        public ActionResult Students()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var students =
                from s in db.Students
                join u in db.UserAccounts on s.UserId equals u.UserId
                select new StudentViewModel
                {
                    UserId = s.UserId,
                    StudentId = s.StudentId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    Avatar = s.Avatar,
                    IsActive = (bool)u.IsActive
                };

            return View(students.ToList());
        }

        public ActionResult EditStudent(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var student =
                (from s in db.Students
                 join u in db.UserAccounts on s.UserId equals u.UserId
                 where s.StudentId == id
                 select new StudentViewModel
                 {
                     UserId = s.UserId,
                     StudentId = s.StudentId,
                     FullName = u.FullName,
                     Email = u.Email,
                     Phone = s.Phone,
                     Address = s.Address,
                     Avatar = s.Avatar,
                     IsActive = (bool)u.IsActive
                 }).FirstOrDefault();

            return View(student);
        }

        [HttpPost]
        public ActionResult EditStudent(int studentId, string fullName, string email, string phone, string address)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var student = db.Students.FirstOrDefault(x => x.StudentId == studentId);

            if (student != null)
            {
                var user = db.UserAccounts.FirstOrDefault(x => x.UserId == student.UserId);

                if (user != null)
                {
                    user.FullName = fullName;
                    user.Email = email;
                }

                student.Phone = phone;
                student.Address = address;
                db.SubmitChanges();
            }

            return RedirectToAction("Students");
        }

        public ActionResult DeactivateStudent(int id)
        {
            SetStudentStatus(id, false);
            return RedirectToAction("Students");
        }

        public ActionResult ActivateStudent(int id)
        {
            SetStudentStatus(id, true);
            return RedirectToAction("Students");
        }

        public ActionResult Teachers()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var teachers =
                from t in db.Teachers
                join u in db.UserAccounts on t.UserId equals u.UserId
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

        public ActionResult CreateTeacher()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public ActionResult CreateTeacher(string fullName, string email, string password, string specialty, int? experienceYears)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            UserAccount user = new UserAccount();
            user.FullName = fullName;
            user.Email = email;
            user.PasswordHash = HashPassword(password);
            user.Role = "Teacher";
            user.IsActive = true;
            user.CreatedDate = DateTime.Now;

            db.UserAccounts.InsertOnSubmit(user);
            db.SubmitChanges();

            Teacher teacher = new Teacher();
            teacher.UserId = user.UserId;
            teacher.Specialty = specialty;
            teacher.ExperienceYears = experienceYears ?? 0;

            db.Teachers.InsertOnSubmit(teacher);
            db.SubmitChanges();

            return RedirectToAction("Teachers");
        }

        public ActionResult EditTeacher(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var teacher =
                (from t in db.Teachers
                 join u in db.UserAccounts on t.UserId equals u.UserId
                 where t.TeacherId == id
                 select new TeacherViewModel
                 {
                     TeacherId = t.TeacherId,
                     FullName = u.FullName,
                     Email = u.Email,
                     Specialty = t.Specialty,
                     ExperienceYears = t.ExperienceYears,
                     IsActive = (bool)u.IsActive
                 }).FirstOrDefault();

            return View(teacher);
        }

        [HttpPost]
        public ActionResult EditTeacher(int teacherId, string fullName, string email, string specialty, int? experienceYears)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var teacher = db.Teachers.FirstOrDefault(x => x.TeacherId == teacherId);

            if (teacher != null)
            {
                var user = db.UserAccounts.FirstOrDefault(x => x.UserId == teacher.UserId);

                if (user != null)
                {
                    user.FullName = fullName;
                    user.Email = email;
                }

                teacher.Specialty = specialty;
                teacher.ExperienceYears = experienceYears;
                db.SubmitChanges();
            }

            return RedirectToAction("Teachers");
        }

        public ActionResult DeactivateTeacher(int id)
        {
            SetTeacherStatus(id, false);
            return RedirectToAction("Teachers");
        }

        public ActionResult ActivateTeacher(int id)
        {
            SetTeacherStatus(id, true);
            return RedirectToAction("Teachers");
        }

        public ActionResult Programs()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View(db.Programs.ToList());
        }

        public ActionResult CreateProgram()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public ActionResult CreateProgram(Program p)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            db.Programs.InsertOnSubmit(p);
            db.SubmitChanges();

            return RedirectToAction("Programs");
        }

        public ActionResult EditProgram(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var program = db.Programs.FirstOrDefault(x => x.ProgramId == id);
            return View(program);
        }

        [HttpPost]
        public ActionResult EditProgram(Program p)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var program = db.Programs.FirstOrDefault(x => x.ProgramId == p.ProgramId);

            program.ProgramName = p.ProgramName;
            program.LevelName = p.LevelName;
            program.DurationMonths = p.DurationMonths;
            program.Fee = p.Fee;
            program.Description = p.Description;
            program.ProgramStatus = p.ProgramStatus;

            db.SubmitChanges();

            return RedirectToAction("Programs");
        }

        public ActionResult DeleteProgram(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var program = db.Programs.FirstOrDefault(x => x.ProgramId == id);

            bool hasClass = db.Classes.Any(x => x.ProgramId == id);

            if (hasClass)
            {
                program.ProgramStatus = false;
                db.SubmitChanges();
                TempData["Error"] = "Program is being used by classes. Status changed to Inactive.";
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
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var program = db.Programs.FirstOrDefault(x => x.ProgramId == id);

            if (program != null)
            {
                program.ProgramStatus = true;
                db.SubmitChanges();
            }

            return RedirectToAction("Programs");
        }

        public ActionResult Classes()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View(db.Classes.OrderByDescending(x => x.StartDate).ToList());
        }

        public ActionResult CreateClass()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            LoadClassDropdowns();
            return View();
        }

        [HttpPost]
        public ActionResult CreateClass(LanguageCenter.Models.Class model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            db.Classes.InsertOnSubmit(model);
            db.SubmitChanges();

            return RedirectToAction("Classes");
        }

        public ActionResult EditClass(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            LoadClassDropdowns();
            return View(db.Classes.FirstOrDefault(x => x.ClassId == id));
        }

        [HttpPost]
        public ActionResult EditClass(LanguageCenter.Models.Class model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var classItem = db.Classes.FirstOrDefault(x => x.ClassId == model.ClassId);

            if (classItem != null)
            {
                classItem.ProgramId = model.ProgramId;
                classItem.TeacherId = model.TeacherId;
                classItem.ClassName = model.ClassName;
                classItem.Room = model.Room;
                classItem.StartDate = model.StartDate;
                classItem.EndDate = model.EndDate;
                classItem.Capacity = model.Capacity;
                classItem.Status = model.Status;
                db.SubmitChanges();
            }

            return RedirectToAction("Classes");
        }

        public ActionResult DeleteClass(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var classItem = db.Classes.FirstOrDefault(x => x.ClassId == id);

            if (classItem != null)
            {
                classItem.Status = "Inactive";
                db.SubmitChanges();
            }

            return RedirectToAction("Classes");
        }

        public ActionResult Registrations()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View(db.Registrations.OrderByDescending(x => x.RegistrationDate).ToList());
        }

        public ActionResult UpdateRegistrationStatus(int id, string status)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var registration = db.Registrations.FirstOrDefault(x => x.RegistrationId == id);

            if (registration != null)
            {
                registration.Status = status;
                db.SubmitChanges();
            }

            return RedirectToAction("Registrations");
        }

        public ActionResult Payments()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View(db.Payments.OrderByDescending(x => x.PaymentDate).ToList());
        }

        public ActionResult ConfirmPayment(int id)
        {
            return UpdatePaymentStatus(id, "Confirmed");
        }

        public ActionResult UpdatePaymentStatus(int id, string status)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var payment = db.Payments.FirstOrDefault(x => x.PaymentId == id);

            if (payment != null)
            {
                payment.PaymentStatus = status;
                payment.PaymentDate = payment.PaymentDate ?? DateTime.Now;
                db.SubmitChanges();
            }

            return RedirectToAction("Payments");
        }

        public ActionResult PlacementTests()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View(db.PlacementTests.OrderByDescending(x => x.TestDate).ToList());
        }

        [HttpPost]
        public ActionResult CreatePlacementTest(int studentId, DateTime testDate, TimeSpan testTime, string suggestedLevel)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            PlacementTest test = new PlacementTest();
            test.StudentId = studentId;
            test.TestDate = testDate;
            test.TestTime = testTime;
            test.SuggestedLevel = suggestedLevel;
            test.Status = "Scheduled";

            db.PlacementTests.InsertOnSubmit(test);
            db.SubmitChanges();

            return RedirectToAction("PlacementTests");
        }

        public ActionResult UpdatePlacementResult(int id, int? resultScore, string suggestedLevel, string status)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var test = db.PlacementTests.FirstOrDefault(x => x.PlacementTestId == id);

            if (test != null)
            {
                test.ResultScore = resultScore;
                test.SuggestedLevel = suggestedLevel;
                test.Status = status;
                db.SubmitChanges();
            }

            return RedirectToAction("PlacementTests");
        }

        private void LoadClassDropdowns()
        {
            ViewBag.Programs = db.Programs.OrderBy(x => x.ProgramName).ToList();
            ViewBag.Teachers = db.Teachers.OrderBy(x => x.UserAccount.FullName).ToList();
        }

        private void SetStudentStatus(int id, bool isActive)
        {
            var student = db.Students.FirstOrDefault(x => x.StudentId == id);

            if (student != null)
            {
                var user = db.UserAccounts.FirstOrDefault(x => x.UserId == student.UserId);

                if (user != null)
                {
                    user.IsActive = isActive;
                    db.SubmitChanges();
                }
            }
        }

        private void SetTeacherStatus(int id, bool isActive)
        {
            var teacher = db.Teachers.FirstOrDefault(x => x.TeacherId == id);

            if (teacher != null)
            {
                var user = db.UserAccounts.FirstOrDefault(x => x.UserId == teacher.UserId);

                if (user != null)
                {
                    user.IsActive = isActive;
                    db.SubmitChanges();
                }
            }
        }

        private bool IsAdmin()
        {
            return Session["Role"] != null &&
                   Session["Role"].ToString() == "Admin";
        }

        private string HashPassword(string password)
        {
            if (password == null)
                password = "";

            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
