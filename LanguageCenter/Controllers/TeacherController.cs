using System;
using System.IO;
using System.Linq;
using System.Text;
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

            var ownedClassIds = db.Classes
                                  .Where(x => x.TeacherId == teacher.TeacherId)
                                  .Select(x => x.ClassId)
                                  .ToList();
            var metadata = LoadMaterialMetadata();

            foreach (string path in Directory.GetFiles(folder))
            {
                string fileName = Path.GetFileName(path);
                int fileClassId;

                if (!TryGetClassId(fileName, out fileClassId) || metadata.Any(x => x.FileName == fileName))
                    continue;

                metadata.Add(new MaterialViewModel
                {
                    ClassId = fileClassId,
                    FileName = fileName,
                    DisplayName = GetOriginalMaterialName(fileName),
                    IsActive = true,
                    UploadedAt = System.IO.File.GetCreationTime(path)
                });
            }

            SaveMaterialMetadata(metadata);

            var materials = metadata
                .Where(x => ownedClassIds.Contains(x.ClassId))
                .Where(x => !classId.HasValue || x.ClassId == classId.Value)
                .Where(x => System.IO.File.Exists(Path.Combine(folder, x.FileName)))
                .OrderByDescending(x => x.UploadedAt)
                .ToList();

            return View(materials);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadMaterial(int classId, HttpPostedFileBase document)
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            bool ownsClass = db.Classes.Any(x => x.ClassId == classId && x.TeacherId == teacher.TeacherId);

            if (!ownsClass)
                return HttpNotFound();

            string error = ValidateUpload(document, false);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Materials", new { classId = classId });
            }

            string folder = Server.MapPath("~/Content/materials/");
            Directory.CreateDirectory(folder);

            string originalName = Path.GetFileName(document.FileName);
            string fileName = "class-" + classId + "-" + Guid.NewGuid().ToString("N") + Path.GetExtension(originalName).ToLowerInvariant();
            document.SaveAs(Path.Combine(folder, fileName));

            var metadata = LoadMaterialMetadata();
            metadata.Add(new MaterialViewModel
            {
                ClassId = classId,
                FileName = fileName,
                DisplayName = Path.GetFileNameWithoutExtension(originalName),
                IsActive = true,
                UploadedAt = DateTime.Now
            });
            SaveMaterialMetadata(metadata);
            TempData["Message"] = "Material uploaded successfully.";

            return RedirectToAction("Materials", new { classId = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMaterial(string fileName, string displayName, bool isActive, int? classId)
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            var metadata = LoadMaterialMetadata();
            var material = metadata.FirstOrDefault(x => x.FileName == Path.GetFileName(fileName));

            if (material == null || !TeacherOwnsClass(teacher.TeacherId, material.ClassId))
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                TempData["Error"] = "Material name is required.";
                return RedirectToAction("Materials", new { classId = classId });
            }

            material.DisplayName = displayName.Trim();
            material.IsActive = isActive;
            SaveMaterialMetadata(metadata);
            TempData["Message"] = "Material updated successfully.";

            return RedirectToAction("Materials", new { classId = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMaterial(string fileName, int? classId)
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            string safeName = Path.GetFileName(fileName);
            var metadata = LoadMaterialMetadata();
            var material = metadata.FirstOrDefault(x => x.FileName == safeName);

            if (material == null || !TeacherOwnsClass(teacher.TeacherId, material.ClassId))
                return HttpNotFound();

            string path = Path.Combine(Server.MapPath("~/Content/materials/"), safeName);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            metadata.Remove(material);
            SaveMaterialMetadata(metadata);
            TempData["Message"] = "Material deleted successfully.";

            return RedirectToAction("Materials", new { classId = classId });
        }

        public ActionResult PlacementResults()
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            var studentIds = GetTeacherStudentIds(teacher.TeacherId);
            return View(db.PlacementTests
                          .Where(x => x.StudentId.HasValue && studentIds.Contains(x.StudentId.Value))
                          .OrderByDescending(x => x.TestDate)
                          .ToList());
        }

        public ActionResult Feedback()
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            var studentIds = GetTeacherStudentIds(teacher.TeacherId);
            return View(db.Consultations
                          .Where(x => x.StudentId.HasValue && studentIds.Contains(x.StudentId.Value))
                          .OrderByDescending(x => x.CreatedDate)
                          .ToList());
        }

        public ActionResult Report()
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            var classes = db.Classes
                            .Where(x => x.TeacherId == teacher.TeacherId)
                            .OrderBy(x => x.ClassName)
                            .ToList();

            ViewBag.TotalClasses = classes.Count;
            ViewBag.TotalRegistrations = db.Registrations.Count(x => x.Class != null && x.Class.TeacherId == teacher.TeacherId);
            ViewBag.ConfirmedRegistrations = db.Registrations.Count(x => x.Class != null && x.Class.TeacherId == teacher.TeacherId && x.Status == "Confirmed");
            ViewBag.PendingRegistrations = db.Registrations.Count(x => x.Class != null && x.Class.TeacherId == teacher.TeacherId && x.Status == "Pending");

            return View(classes);
        }

        public ActionResult ExportReportCsv()
        {
            var teacher = GetCurrentTeacher();

            if (teacher == null)
                return RedirectToAction("Login", "Account");

            var classes = db.Classes
                            .Where(x => x.TeacherId == teacher.TeacherId)
                            .OrderBy(x => x.ClassName)
                            .ToList();
            var csv = new StringBuilder();
            csv.AppendLine("Class,Program,Start Date,End Date,Students,Status");

            foreach (var item in classes)
            {
                csv.AppendLine(string.Join(",",
                    Csv(item.ClassName),
                    Csv(item.Program == null ? "" : item.Program.ProgramName),
                    Csv(item.StartDate.HasValue ? item.StartDate.Value.ToString("yyyy-MM-dd") : ""),
                    Csv(item.EndDate.HasValue ? item.EndDate.Value.ToString("yyyy-MM-dd") : ""),
                    item.Registrations.Count,
                    Csv(item.Status)));
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "teacher-report-" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
        }

        public new ActionResult Profile()
        {
            var currentTeacher = GetCurrentTeacher();
            if (currentTeacher == null)
                return RedirectToAction("Login", "Account");

            var teacher = db.UserAccounts
                            .FirstOrDefault(x => x.UserId == currentTeacher.UserId);

            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadPhoto(HttpPostedFileBase photo)
        {
            var currentTeacher = GetCurrentTeacher();
            if (currentTeacher == null)
                return RedirectToAction("Login", "Account");

            var teacher = db.UserAccounts
                            .FirstOrDefault(x => x.UserId == currentTeacher.UserId);

            string error = ValidateUpload(photo, true);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Profile");
            }

            if (teacher != null)
            {
                string fileName =
                    "teacher-" +
                    currentTeacher.UserId +
                    Path.GetExtension(photo.FileName).ToLowerInvariant();

                string path =
                    Server.MapPath("~/Content/images/");

                Directory.CreateDirectory(path);
                photo.SaveAs(
                    Path.Combine(path, fileName));

                teacher.Photo = fileName;

                db.SubmitChanges();
            }

            return RedirectToAction("Profile");
        }

        private System.Collections.Generic.List<int> GetTeacherStudentIds(int teacherId)
        {
            return db.Registrations
                     .Where(x => x.Class != null && x.Class.TeacherId == teacherId && x.StudentId.HasValue)
                     .Select(x => x.StudentId.Value)
                     .Distinct()
                     .ToList();
        }

        private bool TeacherOwnsClass(int teacherId, int classId)
        {
            return db.Classes.Any(x => x.ClassId == classId && x.TeacherId == teacherId);
        }

        private string MaterialMetadataPath()
        {
            return Server.MapPath("~/App_Data/materials.json");
        }

        private System.Collections.Generic.List<MaterialViewModel> LoadMaterialMetadata()
        {
            return JsonMetadataStore.LoadMaterials(MaterialMetadataPath());
        }

        private void SaveMaterialMetadata(System.Collections.Generic.IEnumerable<MaterialViewModel> items)
        {
            JsonMetadataStore.SaveMaterials(MaterialMetadataPath(), items);
        }

        private bool TryGetClassId(string fileName, out int classId)
        {
            classId = 0;
            if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith("class-", StringComparison.OrdinalIgnoreCase))
                return false;

            int separator = fileName.IndexOf('-', 6);
            return separator > 6 && int.TryParse(fileName.Substring(6, separator - 6), out classId);
        }

        private string GetOriginalMaterialName(string fileName)
        {
            int classId;
            if (!TryGetClassId(fileName, out classId))
                return Path.GetFileNameWithoutExtension(fileName);

            int separator = fileName.IndexOf('-', 6);
            return Path.GetFileNameWithoutExtension(fileName.Substring(separator + 1));
        }

        private string ValidateUpload(HttpPostedFileBase file, bool imageOnly)
        {
            if (file == null || file.ContentLength <= 0)
                return "Please select a file.";

            if (file.ContentLength > 10 * 1024 * 1024)
                return "The file size must not exceed 10 MB.";

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string[] allowed = imageOnly
                ? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }
                : new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".zip", ".jpg", ".jpeg", ".png" };

            return allowed.Contains(extension) ? null : "This file type is not allowed.";
        }

        private string Csv(string value)
        {
            string safe = value ?? "";
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
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
