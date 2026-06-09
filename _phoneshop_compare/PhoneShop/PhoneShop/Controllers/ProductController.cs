using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShop.DB;

namespace PhoneShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly PhoneShopDbContext _context;

        public ProductController(PhoneShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpGet("/Product/{slug}")]
        public async Task<IActionResult> DetailsBySlug(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product == null)
            {
                return NotFound();
            }

            return View("Details", product);
        }
    }
}
