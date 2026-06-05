using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenter.Models
{
    public class StudentViewModel
    {
        public int? UserId { get; set; }

        public int StudentId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Avatar { get; set; }

        public bool IsActive { get; set; }
    }
}
