using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public string Content { get; set; }
        public string RegNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string Base { get; set; }
        public int EmployeeId { get; set; }
    }
}
