using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Models
{
    [Table("Orders")]
    public class Orders
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime OrderDate { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string? ShippingPhone { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string? ShippingAddress { get; set; }

        public int? Status { get; set; }

        [ForeignKey("Users")]
        public int UsersId { get; set; }
        public Users? Users { get; set; }


    }
}