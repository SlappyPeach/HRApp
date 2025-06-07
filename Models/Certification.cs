using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Certification
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Resolution { get; set; }
        public string Category { get; set; }
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string Base { get; set; }
        public DateTime NextDate { get; set; }
        public int EmployeeId { get; set; }
    }
}
