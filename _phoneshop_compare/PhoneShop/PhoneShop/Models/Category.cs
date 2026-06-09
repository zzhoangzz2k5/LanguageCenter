using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PhoneShop.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "nvarchar(200)")]
        public string? Photo { get; set; }
    }
}
