using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using LanguageCenter.Models;

namespace LanguageCenter.Controllers
{
    public class ProgramController : Controller
    {
        LanguageCenterDataContext db =
            new LanguageCenterDataContext(
            ConfigurationManager.ConnectionStrings["LanguageCenterConnectionString"].ConnectionString);

        public ActionResult Index(string search, string level, int page = 1)
        {
            var programs = db.Programs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                programs = programs.Where(x =>
                    x.ProgramName.Contains(search));
            }

            if (!string.IsNullOrEmpty(level))
            {
                programs = programs.Where(x => x.LevelName == level);
            }

            int pageSize = 6;
            int totalItems = programs.Count();
            int totalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1)
                page = 1;

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            ViewBag.Search = search;
            ViewBag.Level = level;
            ViewBag.Levels = db.Programs
                               .Where(x => x.LevelName != null)
                               .Select(x => x.LevelName)
                               .Distinct()
                               .OrderBy(x => x)
                               .ToList();
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(programs
                .OrderBy(x => x.ProgramName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList());
        }

        public ActionResult Detail(int id)
        {
            var program =
                db.Programs.FirstOrDefault(x =>
                x.ProgramId == id);

            if (program == null)
                return HttpNotFound();

            ViewBag.RelatedClasses = db.Classes
                                      .Where(x => x.ProgramId == id)
                                      .OrderByDescending(x => x.StartDate)
                                      .Take(5)
                                      .ToList();

            ViewBag.RelatedPrograms = db.Programs
                                       .Where(x => x.ProgramId != id && x.LevelName == program.LevelName)
                                       .Take(3)
                                       .ToList();

            return View(program);
        }

        public ActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public ActionResult Create(Program p)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            db.Programs.InsertOnSubmit(p);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var p = db.Programs
                      .FirstOrDefault(x => x.ProgramId == id);

            return View(p);
        }

        [HttpPost]
        public ActionResult Edit(Program model)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var p = db.Programs
                      .FirstOrDefault(x => x.ProgramId == model.ProgramId);

            p.ProgramName = model.ProgramName;
            p.LevelName = model.LevelName;
            p.DurationMonths = model.DurationMonths;
            p.Fee = model.Fee;
            p.Description = model.Description;

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Index", "Home");

            var p = db.Programs
                      .FirstOrDefault(x => x.ProgramId == id);

            db.Programs.DeleteOnSubmit(p);

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        private bool IsAdmin()
        {
            return Session["Role"] != null &&
                   Session["Role"].ToString() == "Admin";
        }
    }
}
