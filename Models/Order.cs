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

        // Связь с трудовым договором (для приказов о приёме)
        public int? AgreementId { get; set; }

        // Дополнительные сведения для приказов о переводе
        public string? MoveType { get; set; }
        public string? NewDepartment { get; set; }
        public string? NewPosition { get; set; }
        public bool? Probation { get; set; }
    }
}
