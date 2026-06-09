using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Configuration;
using LanguageCenter.Models;
using Newtonsoft.Json;

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
            var registrationStats = db.Registrations
                                      .GroupBy(x => x.Status ?? "Unknown")
                                      .Select(x => new { Status = x.Key, Count = x.Count() })
                                      .ToList();
            ViewBag.RegistrationChartLabels = JsonConvert.SerializeObject(registrationStats.Select(x => x.Status));
            ViewBag.RegistrationChartValues = JsonConvert.SerializeObject(registrationStats.Select(x => x.Count));

            var revenueStats = db.Payments
                                 .Where(x => x.PaymentDate.HasValue && (x.PaymentStatus == "Paid" || x.PaymentStatus == "Confirmed"))
                                 .AsEnumerable()
                                 .GroupBy(x => x.PaymentDate.Value.ToString("MM/yyyy"))
                                 .Select(x => new { Month = x.Key, Total = x.Sum(y => y.Amount ?? 0) })
                                 .Take(12)
                                 .ToList();
            ViewBag.RevenueChartLabels = JsonConvert.SerializeObject(revenueStats.Select(x => x.Month));
            ViewBag.RevenueChartValues = JsonConvert.SerializeObject(revenueStats.Select(x => x.Total));

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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeactivateStudent(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetStudentStatus(id, false);
            return RedirectToAction("Students");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivateStudent(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeactivateTeacher(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            SetTeacherStatus(id, false);
            return RedirectToAction("Teachers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivateTeacher(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

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

            LoadProgramTypeDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProgram(Program p)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(p.ProgramName) || string.IsNullOrWhiteSpace(p.LevelName))
            {
                TempData["Error"] = "Program name and type are required.";
                LoadProgramTypeDropdown();
                return View(p);
            }

            db.Programs.InsertOnSubmit(p);
            db.SubmitChanges();

            return RedirectToAction("Programs");
        }

        public ActionResult EditProgram(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var program = db.Programs.FirstOrDefault(x => x.ProgramId == id);
            LoadProgramTypeDropdown();
            return View(program);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProgram(Program p)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var program = db.Programs.FirstOrDefault(x => x.ProgramId == p.ProgramId);

            if (program == null)
                return HttpNotFound();

            program.ProgramName = p.ProgramName;
            program.LevelName = p.LevelName;
            program.DurationMonths = p.DurationMonths;
            program.Fee = p.Fee;
            program.Description = p.Description;
            program.ProgramStatus = p.ProgramStatus;

            db.SubmitChanges();

            return RedirectToAction("Programs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProgram(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var program = db.Programs.FirstOrDefault(x => x.ProgramId == id);

            if (program == null)
                return HttpNotFound();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public ActionResult CreatePlacementTest(int studentId, DateTime testDate, TimeSpan testTime, string suggestedLevel)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (testDate.Date < DateTime.Today || !db.Students.Any(x => x.StudentId == studentId))
            {
                TempData["Error"] = "Please select a valid student and a current or future date.";
                return RedirectToAction("PlacementTests");
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        public ActionResult ProgramTypes()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View(LoadProgramTypes());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProgramType(string name)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var types = LoadProgramTypes();
            string normalized = (name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(normalized))
                TempData["Error"] = "Program type name is required.";
            else if (types.Any(x => x.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
                TempData["Error"] = "This program type already exists.";
            else
            {
                types.Add(new ProgramTypeViewModel { Name = normalized, IsActive = true });
                SaveProgramTypes(types);
                TempData["Message"] = "Program type created.";
            }

            return RedirectToAction("ProgramTypes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenameProgramType(string oldName, string newName)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var types = LoadProgramTypes();
            var type = types.FirstOrDefault(x => x.Name.Equals(oldName ?? "", StringComparison.OrdinalIgnoreCase));
            string normalized = (newName ?? "").Trim();

            if (type == null || string.IsNullOrWhiteSpace(normalized))
            {
                TempData["Error"] = "Program type data is invalid.";
                return RedirectToAction("ProgramTypes");
            }

            if (types.Any(x => x != type && x.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["Error"] = "This program type already exists.";
                return RedirectToAction("ProgramTypes");
            }

            foreach (var program in db.Programs.Where(x => x.LevelName == type.Name))
                program.LevelName = normalized;

            type.Name = normalized;
            db.SubmitChanges();
            SaveProgramTypes(types);
            TempData["Message"] = "Program type updated.";
            return RedirectToAction("ProgramTypes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleProgramType(string name)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var types = LoadProgramTypes();
            var type = types.FirstOrDefault(x => x.Name.Equals(name ?? "", StringComparison.OrdinalIgnoreCase));
            if (type != null)
            {
                type.IsActive = !type.IsActive;
                SaveProgramTypes(types);
            }

            return RedirectToAction("ProgramTypes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProgramType(string name)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (db.Programs.Any(x => x.LevelName == name))
            {
                TempData["Error"] = "This type is in use. Rename it or deactivate it instead.";
                return RedirectToAction("ProgramTypes");
            }

            var types = LoadProgramTypes();
            types.RemoveAll(x => x.Name.Equals(name ?? "", StringComparison.OrdinalIgnoreCase));
            SaveProgramTypes(types);
            TempData["Message"] = "Program type deleted.";
            return RedirectToAction("ProgramTypes");
        }

        public ActionResult ExportRegistrationsCsv()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var csv = new StringBuilder();
            csv.AppendLine("Student,Class,Registration Date,Status");
            foreach (var item in db.Registrations.OrderByDescending(x => x.RegistrationDate))
            {
                csv.AppendLine(string.Join(",",
                    Csv(item.Student != null && item.Student.UserAccount != null ? item.Student.UserAccount.FullName : ""),
                    Csv(item.Class != null ? item.Class.ClassName : ""),
                    Csv(item.RegistrationDate.HasValue ? item.RegistrationDate.Value.ToString("yyyy-MM-dd") : ""),
                    Csv(item.Status)));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "registrations-" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
        }

        public ActionResult ExportPaymentsCsv()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var csv = new StringBuilder();
            csv.AppendLine("Payment ID,Student,Class,Amount,Payment Date,Status");
            foreach (var item in db.Payments.OrderByDescending(x => x.PaymentDate))
            {
                csv.AppendLine(string.Join(",",
                    item.PaymentId,
                    Csv(item.Registration != null && item.Registration.Student != null ? item.Registration.Student.UserAccount.FullName : ""),
                    Csv(item.Registration != null && item.Registration.Class != null ? item.Registration.Class.ClassName : ""),
                    item.Amount ?? 0,
                    Csv(item.PaymentDate.HasValue ? item.PaymentDate.Value.ToString("yyyy-MM-dd") : ""),
                    Csv(item.PaymentStatus)));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "payments-" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
        }

        private void LoadClassDropdowns()
        {
            ViewBag.Programs = db.Programs.OrderBy(x => x.ProgramName).ToList();
            ViewBag.Teachers = db.Teachers.OrderBy(x => x.UserAccount.FullName).ToList();
        }

        private void LoadProgramTypeDropdown()
        {
            ViewBag.ProgramTypes = LoadProgramTypes().Where(x => x.IsActive).OrderBy(x => x.Name).ToList();
        }

        private System.Collections.Generic.List<ProgramTypeViewModel> LoadProgramTypes()
        {
            string path = Server.MapPath("~/App_Data/program-types.json");
            var types = JsonMetadataStore.LoadProgramTypes(path);
            var databaseTypes = db.Programs
                                  .Where(x => x.LevelName != null && x.LevelName != "")
                                  .Select(x => x.LevelName)
                                  .Distinct()
                                  .ToList();

            foreach (string name in databaseTypes)
            {
                if (!types.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    types.Add(new ProgramTypeViewModel { Name = name, IsActive = true });
            }

            foreach (var type in types)
                type.ProgramCount = db.Programs.Count(x => x.LevelName == type.Name);

            JsonMetadataStore.SaveProgramTypes(path, types);
            return types.OrderBy(x => x.Name).ToList();
        }

        private void SaveProgramTypes(System.Collections.Generic.IEnumerable<ProgramTypeViewModel> types)
        {
            JsonMetadataStore.SaveProgramTypes(Server.MapPath("~/App_Data/program-types.json"), types);
        }

        private string Csv(string value)
        {
            string safe = value ?? "";
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
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
