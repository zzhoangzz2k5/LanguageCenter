using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenter.Models
{
    public class StudentViewModel
    {
        public int StudentId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }
    }
}