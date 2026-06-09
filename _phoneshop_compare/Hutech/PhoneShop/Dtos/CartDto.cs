namespace PhoneShop.Dtos
{
    public class CartDto
    {
        public PhoneShop.Models.Product? Item { get; set; }
        public int Quantity { get; set; }
    }
}
