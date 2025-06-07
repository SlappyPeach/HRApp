using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class SickLeave
    {
        public int Id { get; set; }
        public DateTime RegDate { get; set; }
        public string RegNumber { get; set; }
        public string MedStateName { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Info { get; set; }
        public string WorkFrom { get; set; }
        public int EmployeeId { get; set; }
    }
}
