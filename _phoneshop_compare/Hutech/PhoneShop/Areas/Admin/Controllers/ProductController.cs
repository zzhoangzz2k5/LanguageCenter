using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhoneShop.DB;
using PhoneShop.Dtos.Product;
using PhoneShop.Models;

namespace PhoneShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private PhoneShopDbContext db;
        private IWebHostEnvironment env;   // environment variable controls folders

        public ProductController(PhoneShopDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }

        public async Task<IActionResult> Index()
        {
            var prods = await db
                .Products
                .Include(p=>p.Category)
                .ToListAsync();
            return View(prods);
        }

        public async Task<IActionResult> Create()
        {
            var cates = await db.Categories
                .Select(c => new SelectListItem(
                    c.Name, c.Id.ToString())).ToListAsync();
            ViewBag.CategoryId = cates;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest p)
        {
            if (!ModelState.IsValid)
            {
                return View(p);
            }

            string? imgFilename = string.Empty;
            if (p.Photo != null && p.Photo.Length > 0)
            {
                try
                {
                    var imgFolder = Path.Combine(env.WebRootPath, "images");
                    // create the folder if it doesn't exist
                    if (!Directory.Exists(imgFolder))
                    {
                        Directory.CreateDirectory(imgFolder);
                    }

                    string? imgPath = Path.Combine(imgFolder, p.Photo.FileName);
                    using (var fs = new FileStream(imgPath, FileMode.Create))
                    {
                        await p.Photo.CopyToAsync(fs);
                    }
                    imgFilename = p.Photo.FileName;
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"Upload image error: {ex.Message}";
                    return View(p);
                }
            }

            Category? cate = null;
            if (p.CategoryId.HasValue)
            {
                cate = await db.Categories.FindAsync(p.CategoryId.Value);
            }

            var product = new Product
            {
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                PriceSale = p.PriceSale,
                Category = cate,
                Photo = imgFilename
            };

            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
