using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class EmployeeAward
    {
        public int Id { get; set; }
        public DateTime AwardDate { get; set; }      // Дата награждения
        public string AwardNumber { get; set; }      // Номер документа
        public string Department { get; set; }       // МКШ, Администрация, Мин.Образование
        public string AwardType { get; set; }        // Грамота, благодарность, ценный подарок
        public int EmployeeId { get; set; }          // Ссылка на сотрудника
        public Employee Employee { get; set; }       // Навигационное свойство
    }

}
