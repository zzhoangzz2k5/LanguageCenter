using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LanguageCenter.Models
{
    public class TeacherViewModel
    {
        public int TeacherId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Specialty { get; set; }

        public int? ExperienceYears { get; set; }

        public bool IsActive { get; set; }
    }
}