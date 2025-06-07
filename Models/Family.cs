using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Family
    {
        public int Id { get; set; }
        public string Relation { get; set; }
        public string FIO { get; set; }
        public int BirthYear { get; set; }
        public string Gender { get; set; }
        public bool HasTaxBenefit { get; set; }
        public int EmployeeId { get; set; }
    }
}
