using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShop.DB;
using PhoneShop.Models;
using System.Diagnostics;

namespace PhoneShop.Controllers
{

    public class HomeController : Controller
    {
        readonly PhoneShopDbContext _ctx;

        public HomeController(PhoneShopDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IActionResult> Index()
        {
            var featured = await _ctx.Products
                .Where(p=>p.Featured!.Value)
                .Take(10)
                .ToListAsync();
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

        public IActionResult Products()
        {
            return View();
        }
    }
}
