using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Education
    {
        public int Id { get; set; }
        public string EducationType { get; set; }
        public string InstitutionName { get; set; }
        public string DocName { get; set; }
        public string? DocSerial { get; set; }
        public string DocNumber { get; set; }
        public int EndYear { get; set; }
        public string Qualification { get; set; }
        public string Specialization { get; set; }
        public int EmployeeId { get; set; }
    }
}
