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
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string fullName, string phone, string address)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Full name is required.";
                return RedirectToAction("Index");
            }

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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public ActionResult UploadAvatar(HttpPostedFileBase avatar)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            string error = ValidateImage(avatar);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Index");
            }

            string extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            string fileName = "student-" + student.StudentId + extension;
            string folder = Server.MapPath("~/Content/images/");
            Directory.CreateDirectory(folder);

            avatar.SaveAs(Path.Combine(folder, fileName));

            student.Avatar = fileName;
            db.SubmitChanges();
            TempData["Message"] = "Avatar uploaded successfully.";

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
        [ValidateAntiForgeryToken]
        public ActionResult RegisterClass(int classId, string note)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            var classItem = db.Classes.FirstOrDefault(x => x.ClassId == classId);
            if (classItem == null)
                return HttpNotFound();

            if (classItem.Status == "Inactive" || classItem.Status == "Closed" || classItem.Status == "Full")
            {
                TempData["Error"] = "This class is not open for registration.";
                return RedirectToAction("RegisterClass");
            }

            int occupied = db.Registrations.Count(x => x.ClassId == classId && x.Status != "Cancelled");
            if (classItem.Capacity.HasValue && occupied >= classItem.Capacity.Value)
            {
                TempData["Error"] = "This class has reached its capacity.";
                return RedirectToAction("RegisterClass");
            }

            bool exists = db.Registrations.Any(x => x.StudentId == student.StudentId && x.ClassId == classId && x.Status != "Cancelled");

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
        [ValidateAntiForgeryToken]
        public ActionResult PlacementTests(DateTime testDate, TimeSpan testTime, string suggestedLevel)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            if (testDate.Date < DateTime.Today)
            {
                TempData["Error"] = "Test date cannot be in the past.";
                return RedirectToAction("PlacementTests");
            }

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
        [ValidateAntiForgeryToken]
        public ActionResult Consultation(string question, string contactInfo)
        {
            var student = GetCurrentStudent();

            if (student == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(contactInfo))
            {
                TempData["Error"] = "Question and contact information are required.";
                return RedirectToAction("Consultation");
            }

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

        private Student GetCurrentStudent()
        {
            if (Session["UserId"] == null ||
                Session["Role"] == null ||
                Session["Role"].ToString() != "Student")
                return null;

            int userId = Convert.ToInt32(Session["UserId"]);
            return db.Students.FirstOrDefault(x => x.UserId == userId);
        }

        private StudentViewModel GetCurrentStudentViewModel()
        {
            if (Session["UserId"] == null ||
                Session["Role"] == null ||
                Session["Role"].ToString() != "Student")
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

        private string ValidateImage(HttpPostedFileBase image)
        {
            if (image == null || image.ContentLength <= 0)
                return "Please select an image.";

            if (image.ContentLength > 5 * 1024 * 1024)
                return "The image size must not exceed 5 MB.";

            string extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            string[] allowed = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            return allowed.Contains(extension) ? null : "Only JPG, PNG, GIF, and WEBP images are allowed.";
        }
    }
}
