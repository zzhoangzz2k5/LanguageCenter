using LanguageCenter.Models;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LanguageCenter.Controllers
{
    public class StudentController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        public ActionResult Index()
        {
            var student = GetCurrentStudentViewModel();

            if (student == null)
                return RedirectToAction("Login", "Account");

            return View(student);
        }

        [HttpPost]
        public ActionResult UpdateProfile(string fullName, string phone, string address)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            var user = db.UserAccounts.FirstOrDefault(x => x.UserId == student.UserId);

            if (user != null)
                user.FullName = fullName;

            student.Phone = phone;
            student.Address = address;

            db.SubmitChanges();
            TempData["Message"] = "Profile updated successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            var user = db.UserAccounts.FirstOrDefault(x => x.UserId == student.UserId);
            string currentHash = HashPassword(currentPassword);

            if (user == null || (user.PasswordHash != currentHash && user.PasswordHash != currentPassword))
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6 || newPassword != confirmPassword)
            {
                TempData["Error"] = "New password must be at least 6 characters and match confirmation.";
                return RedirectToAction("Index");
            }

            user.PasswordHash = HashPassword(newPassword);
            db.SubmitChanges();
            TempData["Message"] = "Password changed successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UploadAvatar(HttpPostedFileBase avatar)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            if (avatar != null && avatar.ContentLength > 0)
            {
                string extension = Path.GetExtension(avatar.FileName);
                string fileName = "student-" + student.StudentId + extension;
                string folder = Server.MapPath("~/Content/images/");

                avatar.SaveAs(Path.Combine(folder, fileName));

                student.Avatar = fileName;
                db.SubmitChanges();
                TempData["Message"] = "Avatar uploaded successfully.";
            }

            return RedirectToAction("Index");
        }

        public ActionResult RegisterClass(int? classId, int? programId)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            var classes = db.Classes.AsQueryable();

            if (programId.HasValue)
                classes = classes.Where(x => x.ProgramId == programId.Value);

            ViewBag.SelectedClassId = classId;
            return View(classes.OrderByDescending(x => x.StartDate).ToList());
        }

        [HttpPost]
        public ActionResult RegisterClass(int classId, string note)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            bool exists = db.Registrations.Any(x => x.StudentId == student.StudentId && x.ClassId == classId);

            if (!exists)
            {
                Registration registration = new Registration();
                registration.StudentId = student.StudentId;
                registration.ClassId = classId;
                registration.RegistrationDate = DateTime.Now;
                registration.Status = "Pending";

                db.Registrations.InsertOnSubmit(registration);
                db.SubmitChanges();
                TempData["Message"] = "Class registration submitted. Please wait for confirmation.";
            }
            else
            {
                TempData["Error"] = "You have already registered for this class.";
            }

            return RedirectToAction("MyClasses");
        }

        public ActionResult MyClasses()
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            var registrations = db.Registrations
                                  .Where(x => x.StudentId == student.StudentId)
                                  .OrderByDescending(x => x.RegistrationDate)
                                  .ToList();

            return View(registrations);
        }

        public ActionResult Payments()
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            var payments = db.Payments
                             .Where(x => x.Registration != null &&
                                         x.Registration.StudentId == student.StudentId)
                             .OrderByDescending(x => x.PaymentDate)
                             .ToList();

            return View(payments);
        }

        public ActionResult PlacementTests()
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            return View(db.PlacementTests
                          .Where(x => x.StudentId == student.StudentId)
                          .OrderByDescending(x => x.TestDate)
                          .ToList());
        }

        [HttpPost]
        public ActionResult PlacementTests(DateTime testDate, TimeSpan testTime, string suggestedLevel)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            PlacementTest test = new PlacementTest();
            test.StudentId = student.StudentId;
            test.TestDate = testDate;
            test.TestTime = testTime;
            test.SuggestedLevel = suggestedLevel;
            test.Status = "Pending";

            db.PlacementTests.InsertOnSubmit(test);
            db.SubmitChanges();

            TempData["Message"] = "Placement test registration submitted.";
            return RedirectToAction("PlacementTests");
        }

        public ActionResult Consultation()
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            return View(db.Consultations
                          .Where(x => x.StudentId == student.StudentId)
                          .OrderByDescending(x => x.CreatedDate)
                          .ToList());
        }

        [HttpPost]
        public ActionResult Consultation(string question, string contactInfo)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            Consultation consultation = new Consultation();
            consultation.StudentId = student.StudentId;
            consultation.Question = question;
            consultation.ContactInfo = contactInfo;
            consultation.RequestStatus = "Pending";
            consultation.CreatedDate = DateTime.Now;

            db.Consultations.InsertOnSubmit(consultation);
            db.SubmitChanges();

            TempData["Message"] = "Consultation request submitted.";
            return RedirectToAction("Consultation");
        }

        public ActionResult Activate(int id)
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

            return RedirectToAction("Index");
        }

        public ActionResult Deactivate(int id)
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

            return RedirectToAction("Index");
        }

        private Student GetCurrentStudent()
        {
            if (Session["UserId"] == null)
                return null;

            int userId = Convert.ToInt32(Session["UserId"]);
            return db.Students.FirstOrDefault(x => x.UserId == userId);
        }

        private StudentViewModel GetCurrentStudentViewModel()
        {
            if (Session["UserId"] == null)
                return null;

            int userId = Convert.ToInt32(Session["UserId"]);

            return (from s in db.Students
                    join u in db.UserAccounts on s.UserId equals u.UserId
                    where u.UserId == userId
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
