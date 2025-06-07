using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DocName { get; set; }
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public int EmployeeId { get; set; }
    }
}
