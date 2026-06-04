using System;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using LanguageCenter.Models;

namespace LanguageCenter.Controllers
{
    public class AccountController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            var user = db.UserAccounts
                         .FirstOrDefault(x =>
                            x.Email == email &&
                            x.PasswordHash == password &&
                            x.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // Teacher đang chờ duyệt
            if (user.Role == "PendingTeacher")
            {
                ViewBag.Error =
                    "Your teacher account is waiting for admin approval.";

                return View();
            }

            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            if (user.Role == "Teacher")
                return RedirectToAction("Index", "Teacher");

            return RedirectToAction("Index", "Student");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(
     string fullname,
     string email,
     string password,
     string confirmPassword,
     string role)
        {
            if (password != confirmPassword)
            {
                ViewBag.Message = "Password does not match";
                return View();
            }

            var check =
                db.UserAccounts
                  .FirstOrDefault(x =>
                  x.Email == email);

            if (check != null)
            {
                ViewBag.Message = "Email already exists";
                return View();
            }

            UserAccount user =
                new UserAccount();

            user.FullName = fullname;
            user.Email = email;
            user.PasswordHash = password;

            // Chỉ cho đăng ký Student hoặc Teacher chờ duyệt
            if (role == "PendingTeacher")
            {
                user.Role = "PendingTeacher";
            }
            else
            {
                user.Role = "Student";
            }

            user.IsActive = true;

            db.UserAccounts.InsertOnSubmit(user);

            db.SubmitChanges();

            if (role == "PendingTeacher")
            {
                ViewBag.Message =
                    "Teacher registration request sent. Please wait for admin approval.";

                return View();
            }

            return RedirectToAction("Login");
        }
    }
}