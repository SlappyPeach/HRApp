using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Profession
    {
        public int Id { get; set; }
        public string ProfessionName { get; set; }
        public string WorkPlace { get; set; }
        public bool Main { get; set; }
        public int EmployeeId { get; set; }
    }
}
