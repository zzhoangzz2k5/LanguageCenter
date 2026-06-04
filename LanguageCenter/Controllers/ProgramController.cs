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

        public ActionResult Index(string search)
        {
            var programs = db.Programs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                programs = programs.Where(x =>
                    x.ProgramName.Contains(search));
            }

            return View(programs.ToList());
        }

        public ActionResult Detail(int id)
        {
            var program =
                db.Programs.FirstOrDefault(x =>
                x.ProgramId == id);

            return View(program);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Program p)
        {
            db.Programs.InsertOnSubmit(p);

            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var p = db.Programs
                      .FirstOrDefault(x =>
                      x.ProgramId == id);

            return View(p);
        }

        [HttpPost]
        public ActionResult Edit(Program model)
        {
            var p = db.Programs
                      .FirstOrDefault(x =>
                      x.ProgramId == model.ProgramId);

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
            var p = db.Programs
                      .FirstOrDefault(x =>
                      x.ProgramId == id);

            db.Programs.DeleteOnSubmit(p);

            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}