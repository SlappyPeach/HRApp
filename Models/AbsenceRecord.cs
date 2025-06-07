using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class AbsenceRecord
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public int Day { get; set; } // от 1 до 31
        public string Status { get; set; } = ""; // "Я", "О", "Б", "К", "Н"

        public Employee? Employee { get; set; }
    }
}
