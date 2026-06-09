using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Models
{
    [Table("Roles")]
    public class Roles
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string? Name { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string? Description { get; set; }
    }
}