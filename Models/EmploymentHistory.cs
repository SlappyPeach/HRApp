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
        public int RecordNumber { get; set; }
        public DateTime Date { get; set; }
        public string WorkPlaceName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
    }
}
