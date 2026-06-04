using LanguageCenter.Models;
using System;
using System.Configuration;
using System.Linq;
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
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(Session["UserId"]);

            var student =
                (from s in db.Students
                 join u in db.UserAccounts
                 on s.UserId equals u.UserId
                 where u.UserId == userId
                 select new StudentViewModel
                 {
                     StudentId = s.StudentId,
                     FullName = u.FullName,
                     Email = u.Email,
                     IsActive = (bool)u.IsActive
                 }).FirstOrDefault();

            return View(student);
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
    }
}