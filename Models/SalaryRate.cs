using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class SalaryRate
    {
        public int Id { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; }

        public decimal Amount { get; set; }
    }
}

