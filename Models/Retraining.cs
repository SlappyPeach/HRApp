using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Retraining
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Speciality { get; set; }
        public string Institution { get; set; }
        public string DocName { get; set; }
        public string DocNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string Base { get; set; }
        public int EmployeeId { get; set; }
    }
}
