using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class MilitaryRegistration
    {
        public int Id { get; set; }
        public string StockCategory { get; set; }
        public string Rank { get; set; }
        public string Compound { get; set; }
        public string VYS { get; set; }
        public string MilitaryService { get; set; }
        public string MillitaryOffice { get; set; }
        public string AccountingGroup { get; set; }
        public string SpecAccounting { get; set; }
        public string DeregistrationNote { get; set; }
        public int EmployeeId { get; set; }
    }
}
