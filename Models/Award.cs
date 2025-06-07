using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Award
    {
        public int Id { get; set; }
        public string Title { get; set; }         // Название награды
        public int Year { get; set; }             // Год получения
        public string GrantedBy { get; set; }     // Кем выдана
        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }
    }
}

