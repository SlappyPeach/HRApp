using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Vacation
    {
        public int Id { get; set; }
        public string VacationType { get; set; }
        public string WorkFrom { get; set; }
        public string WorkTo { get; set; }
        public int CalendarDaysNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Base { get; set; }
        public int EmployeeId { get; set; }
    }
}
