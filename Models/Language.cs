using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Language
    {
        public int Id { get; set; }
        public string OKIN { get; set; }
        public string OKINLevel { get; set; }
        public int EmployeeId { get; set; }
        public string? LanguageName { get; set; }
    }
}
