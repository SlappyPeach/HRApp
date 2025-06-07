using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRApp.Models
{
    public class Employee
    {
        public int Id { get; set; }

        // Персональные данные
        public string Surename { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Sex { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string BirthPlaceOKATO { get; set; }
        public string Citizenship { get; set; }
        public string CitizenshipOKIN { get; set; }

        // Паспортные данные
        public string PassportNumber { get; set; }
        public string PassportType { get; set; }
        public string PassportPlace { get; set; }
        public DateTime PassportDate { get; set; }

        // Финансовые реквизиты
        public string INN { get; set; }
        public string SNILS { get; set; }
        public string NomerScheta { get; set; }
        public string BIK { get; set; }
        public string Korschet { get; set; }
        public string KPP { get; set; }

        // Адреса
        public string Address1 { get; set; }
        public string Index1 { get; set; }
        public DateTime Address1Date { get; set; }
        public string Address2 { get; set; }
        public string Index2 { get; set; }

        public string TelNumber { get; set; }
        public string FamilyStatus { get; set; }

        public DateTime? DismissalDate { get; set; }
        public string? ResumePath { get; set; }

        // Внешние ключи
        public int? PositionId { get; set; }
        public Position Position { get; set; }

        public int? DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
