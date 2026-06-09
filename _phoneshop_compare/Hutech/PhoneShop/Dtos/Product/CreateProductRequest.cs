using PhoneShop.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneShop.Dtos.Product
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Please input Product Name")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
        public decimal? Price { get; set; }
        public decimal? PriceSale { get; set; }
        public IFormFile? Photo { get; set; }
        public int? CategoryId { get; set; }
    }
}
