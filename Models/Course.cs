using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseType { get; set; } // Повышение, переподготовка и т.п.
        public string Institution { get; set; }
        public string Certificate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }
    }

}
