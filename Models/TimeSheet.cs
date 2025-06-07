using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class TimeSheet
    {
        public int Id { get; set; }
        public string TimeSheetCode { get; set; }
        public int DaysPerWeek { get; set; }
        public decimal HourPrice { get; set; }
        public int HoursWeek1 { get; set; }
        public int HoursWeek2 { get; set; }
        public int EmployeeId { get; set; }
    }
}
