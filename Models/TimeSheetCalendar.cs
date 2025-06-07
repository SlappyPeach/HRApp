using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class TimeSheetCalendar
    {
        public int Id { get; set; }
        public string TimeSheetCode { get; set; }
        public string RegType { get; set; }
        public DateTime Day { get; set; }
    }
}
