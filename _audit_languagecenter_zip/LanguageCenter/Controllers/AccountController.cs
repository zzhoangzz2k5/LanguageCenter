using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        public ActionResult Login(string email, string password, bool rememberMe = false)
        {
            string hashedPassword = HashPassword(password);

            var user = db.UserAccounts
                         .FirstOrDefault(x =>
                            x.Email == email &&
                            (x.PasswordHash == hashedPassword || x.PasswordHash == password) &&
                            x.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Email or password is incorrect.";
                return View();
            }

            if (user.Role == "Pending" || user.Role == "PendingTeacher")
            {
                ViewBag.Error =
                    "Your account is waiting for admin approval and role assignment.";

                return View();
            }

            Session["UserId"] = user.UserId;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;

            if (rememberMe)
            {
                Response.Cookies["RememberEmail"].Value = user.Email;
                Response.Cookies["RememberEmail"].Expires = DateTime.Now.AddDays(14);
            }

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            if (user.Role == "Teacher")
                return RedirectToAction("Dashboard", "Teacher");

            return RedirectToAction("Index", "Student");
        }

        public ActionResult ForgotPassword()
        {
            return View();
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
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(fullname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Message = "Please enter all required information.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Message = "Password confirmation does not match.";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Message = "Password must be at least 6 characters.";
                return View();
            }

            var check =
                db.UserAccounts
                  .FirstOrDefault(x =>
                  x.Email == email);

            if (check != null)
            {
                ViewBag.Message = "Email already exists.";
                return View();
            }

            UserAccount user =
                new UserAccount();

            user.FullName = fullname.Trim();
            user.Email = email.Trim();
            user.PasswordHash = HashPassword(password);
            user.Role = "Pending";
            user.IsActive = true;
            user.CreatedDate = DateTime.Now;

            db.UserAccounts.InsertOnSubmit(user);
            db.SubmitChanges();

            ViewBag.Message =
                "Registration successful! Your request was sent to admin. Please wait for approval.";

            return View();
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
