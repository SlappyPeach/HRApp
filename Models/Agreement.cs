using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Agreement
    {
        public int Id { get; set; }
        public string RegNumber { get; set; }
        public DateTime AgreementDate { get; set; }
        public DateTime AgreementEndDate { get; set; }
        public bool Probation { get; set; }
        public string PaySystem { get; set; }
        public decimal Salary { get; set; }
        public string Base { get; set; }
        public string FileName { get; set; }
        public int EmployeeId { get; set; }
    }
}
