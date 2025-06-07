using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class EmploymentHistory
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string WorkPlaceName { get; set; }
        public string Speciality { get; set; }
        public int EmployeeId { get; set; }
    }
}
