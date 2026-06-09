using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShop.DB;
using PhoneShop.Models;
using System.Text.Json;

namespace PhoneShop.Controllers
{
    public class CartController : Controller
    {
        private readonly PhoneShopDbContext _context;
        private const string CART_COOKIE_KEY = "PhoneShopCart";

        public CartController(PhoneShopDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCartFromCookie()
        {
            var cookie = Request.Cookies[CART_COOKIE_KEY];
            if (string.IsNullOrEmpty(cookie))
            {
                return new List<CartItem>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<CartItem>>(cookie) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        private void SaveCartToCookie(List<CartItem> cart)
        {
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Path = "/"
            };
            var json = JsonSerializer.Serialize(cart);
            Response.Cookies.Append(CART_COOKIE_KEY, json, options);
        }

        public IActionResult Index()
        {
            var cart = GetCartFromCookie();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCartFromCookie();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name ?? "",
                    Photo = product.Photo ?? "",
                    Price = product.PriceSale ?? product.Price ?? 0,
                    Quantity = quantity
                });
            }

            SaveCartToCookie(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCartFromCookie();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                item.Quantity = quantity;
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
            }
            SaveCartToCookie(cart);

            var itemSubtotal = item != null ? item.SubTotal : 0;
            var subtotal = cart.Sum(i => i.SubTotal);

            return Json(new { success = true, itemSubtotal = itemSubtotal, subtotal = subtotal });
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCartFromCookie();
            var item = cart.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
            }
            SaveCartToCookie(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = GetCartFromCookie();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index");
            }
            return View(cart);
        }

        [HttpPost]
        public IActionResult PlaceOrder(string firstName, string lastName, string phone, string address, string email, string message)
        {
            var cart = GetCartFromCookie();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index");
            }

            Response.Cookies.Delete(CART_COOKIE_KEY);

            ViewBag.OrderName = $"{firstName} {lastName}";
            ViewBag.OrderPhone = phone;
            ViewBag.OrderAddress = address;
            ViewBag.OrderEmail = email;
            ViewBag.OrderTotal = cart.Sum(i => i.SubTotal);

            return View("Success");
        }
    }
}
