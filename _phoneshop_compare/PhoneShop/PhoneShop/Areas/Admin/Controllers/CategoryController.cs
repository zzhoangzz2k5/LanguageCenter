using Microsoft.AspNetCore.Mvc;
using PhoneShop.DB;
using PhoneShop.Models;
using Microsoft.EntityFrameworkCore;

namespace PhoneShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly PhoneShopDbContext _context;

        public CategoryController(PhoneShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToArrayAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category model, IFormFile? Photo)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? fileName = null;
            if (Photo != null && Photo.Length > 0)
            {
                fileName = Path.GetFileName(Photo.FileName);
                // persist file to disk if desired (not implemented)
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                Photo = fileName
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
