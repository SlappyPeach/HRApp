using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class SocialBenefit
    {
        public int Id { get; set; }
        public string Type { get; set; }          // Вид льготы
        public string Document { get; set; }      // Документ-основание
        public DateTime Date { get; set; }        // Дата назначения
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }
    }
}

