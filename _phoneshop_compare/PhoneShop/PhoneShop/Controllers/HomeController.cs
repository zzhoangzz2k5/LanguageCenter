using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShop.DB;
using PhoneShop.Models;
using System.Diagnostics;

namespace PhoneShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly PhoneShopDbContext _context;

        public HomeController(PhoneShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            var products = await _context.Products.ToListAsync();
            var featured = products.Where(p => p.Featured == true).Take(4).ToList();

            if (featured.Count == 0)
            {
                featured = products.Take(4).ToList();
            }

            ViewBag.Categories = categories;
            ViewBag.Products = products;
            ViewBag.Featured = featured;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
