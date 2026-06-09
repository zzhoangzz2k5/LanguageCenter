using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhoneShop.DB;
using PhoneShop.Dtos.Product;
using PhoneShop.Models;
using PhoneShop.Utilities;

namespace PhoneShop.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private static readonly HashSet<string> AllowedImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly PhoneShopDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public ProductController(PhoneShopDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync();

        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View(new CreateProductRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        if (request.PriceSale.HasValue && request.Price.HasValue && request.PriceSale > request.Price)
        {
            ModelState.AddModelError(nameof(request.PriceSale), "Sale price cannot exceed the regular price.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return View(request);
        }

        string? imageFileName = null;
        if (request.Photo is { Length: > 0 })
        {
            var extension = Path.GetExtension(request.Photo.FileName);
            if (!AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(request.Photo), "Only JPG, PNG, and WebP images are allowed.");
                await LoadCategoriesAsync();
                return View(request);
            }

            imageFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var imageFolder = Path.Combine(_environment.WebRootPath, "img", "products");
            Directory.CreateDirectory(imageFolder);

            var imagePath = Path.Combine(imageFolder, imageFileName);
            await using var stream = new FileStream(imagePath, FileMode.Create);
            await request.Photo.CopyToAsync(stream);
        }

        var product = new Product
        {
            Name = request.Name,
            Slug = await CreateUniqueSlugAsync(request.Name),
            Description = request.Description,
            Price = request.Price,
            PriceSale = request.PriceSale,
            CategoryId = request.CategoryId,
            Photo = imageFileName,
            Featured = request.Featured
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync()
    {
        ViewBag.CategoryId = await _db.Categories
            .AsNoTracking()
            .Select(category => new SelectListItem(category.Name, category.Id.ToString()))
            .ToListAsync();
    }

    private async Task<string> CreateUniqueSlugAsync(string? name)
    {
        var baseSlug = SlugHelper.Generate(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _db.Products.AnyAsync(product => product.Slug == slug))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }
}
