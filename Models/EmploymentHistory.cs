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
        public string WorkPlaceName { get; set; }        
        public string Position { get; set; }
        public string Content { get; set; }
        public string Reason { get; set; }
        public int EmployeeId { get; set; }
    }
}
