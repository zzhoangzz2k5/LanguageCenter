namespace PhoneShop.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;
    }
}
