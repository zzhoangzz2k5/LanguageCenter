using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string? Name { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string? Email { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string? Password { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string? Phone { get; set; }

        [Column(TypeName = "Text")]
        public string? Address { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string? Photo { get; set; }

        [ForeignKey("Roles")]
        public int? RoleId { get; set; }
        public Roles? Role { get; set; }
    }
}