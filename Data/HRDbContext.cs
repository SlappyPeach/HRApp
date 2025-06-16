using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRApp.Models;

namespace HRApp.Data
{
    public class HRDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Register> Registers { get; set; }
        public DbSet<AddEducation> AddEducations { get; set; }
        public DbSet<AdditionalInfo> AdditionalInfos { get; set; }
        public DbSet<Agreement> Agreements { get; set; }
        public DbSet<BusinessTrip> BusinessTrips { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<CitizenshipOKIN> CitizenshipOKINs { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<EmploymentHistory> EmploymentHistories { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<ForeignLanguage> ForeignLanguages { get; set; }
        public DbSet<IncomingDocument> IncomingDocuments { get; set; }
        public DbSet<InternalDocument> InternalDocuments { get; set; }
        public DbSet<LanguageLevel> LanguageLevels { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Marriage> Marriages { get; set; }
        public DbSet<MilitaryRegistration> MilitaryRegistrations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OutgoingDocument> OutgoingDocuments { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<Profession> Professions { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<QualificationTraining> QualificationTrainings { get; set; }
        public DbSet<RegisterDocument> RegisterDocuments { get; set; }
        public DbSet<Retraining> Retrainings { get; set; }
        public DbSet<SickLeave> SickLeaves { get; set; }
        public DbSet<TimeSheet> TimeSheets { get; set; }
        public DbSet<TimeSheetCalendar> TimeSheetCalendars { get; set; }
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Award> Awards { get; set; }
        public DbSet<SocialBenefit> SocialBenefits { get; set; }
        public DbSet<AbsenceRecord> AbsenceRecords { get; set; }

        public DbSet<Position> Positions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<SalaryRate> SalaryRates { get; set; }

        public DbSet<EmployeeAward> EmployeeAwards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HRApp");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string dbPath = Path.Combine(folder, "hrdatabase.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ключи по строковым ID
            modelBuilder.Entity<CitizenshipOKIN>().HasKey(c => c.OKIN);
            modelBuilder.Entity<ForeignLanguage>().HasKey(f => f.OKIN);
            modelBuilder.Entity<LanguageLevel>().HasKey(l => l.OKIN);
            modelBuilder.Entity<Marriage>().HasKey(m => m.OKIN);

            // Admin
            modelBuilder.Entity<Register>().HasData(new Register
            {
                Id = 1,
                LoginUser = "Admin",
                PasswordUser = "admin"
            });

            // Справочники
            modelBuilder.Entity<Position>().ToTable("Positions");
            modelBuilder.Entity<Department>().ToTable("Departments");
            modelBuilder.Entity<SalaryRate>().ToTable("SalaryRates");

            modelBuilder.Entity<Position>().HasIndex(p => p.Title).IsUnique();
            modelBuilder.Entity<Department>().HasIndex(d => d.Name).IsUnique();

            modelBuilder.Entity<SalaryRate>()
                .HasOne(s => s.Position)
                .WithMany(p => p.SalaryRates)
                .HasForeignKey(s => s.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связи с Employee
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Position)
                .WithMany()
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<EmployeeAward>()
                .HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Position>()
                .HasOne(p => p.Department)
                .WithMany()
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Административно-управленческий персонал" },
                new Department { Id = 2, Name = "Отдел бухгалтерии" },
                new Department { Id = 3, Name = "Педагогический персонал" },
                new Department { Id = 4, Name = "Учебно-вспомогательный персонал" },
                new Department { Id = 5, Name = "Обслуживающий персонал" },
                new Department { Id = 6, Name = "Общежитие" }
            );

            modelBuilder.Entity<Position>().HasData(
                // Административно-управленческий персонал
                new Position { Id = 1, Title = "Директор", DepartmentId = 1 },
                new Position { Id = 2, Title = "Заместитель директора", DepartmentId = 1 },
                new Position { Id = 3, Title = "Специалист по кадрам", DepartmentId = 1 },
                new Position { Id = 4, Title = "Специалист по охране труда", DepartmentId = 1 },

                // Бухгалтерия
                new Position { Id = 5, Title = "Главный бухгалтер", DepartmentId = 2 },
                new Position { Id = 6, Title = "Бухгалтер", DepartmentId = 2 },
                new Position { Id = 7, Title = "Экономист", DepartmentId = 2 },
                new Position { Id = 8, Title = "Кассир", DepartmentId = 2 },
                new Position { Id = 9, Title = "Специалист по закупкам", DepartmentId = 2 },

                // Педагогический персонал
                new Position { Id = 10, Title = "Учитель", DepartmentId = 3 },
                new Position { Id = 11, Title = "Педагог-психолог", DepartmentId = 3 },
                new Position { Id = 12, Title = "Педагог-организатор", DepartmentId = 3 },
                new Position { Id = 13, Title = "Педагог-дополнительного образования", DepartmentId = 3 },
                new Position { Id = 14, Title = "Учитель-логопед", DepartmentId = 3 },
                new Position { Id = 15, Title = "Методист", DepartmentId = 3 },
                new Position { Id = 16, Title = "Воспитатель", DepartmentId = 3 },
                new Position { Id = 17, Title = "Старший вожатый", DepartmentId = 3 },

                // Учебно-вспомогательный персонал
                new Position { Id = 18, Title = "Заведующий библиотеки", DepartmentId = 4 },
                new Position { Id = 19, Title = "Библиотекарь", DepartmentId = 4 },
                new Position { Id = 20, Title = "Заведующий научной лабораторией", DepartmentId = 4 },
                new Position { Id = 21, Title = "Инженер-программист", DepartmentId = 4 },
                new Position { Id = 22, Title = "Секретарь учебной части", DepartmentId = 4 },
                new Position { Id = 23, Title = "Лаборант", DepartmentId = 4 },
                new Position { Id = 24, Title = "Медицинская сестра", DepartmentId = 4 },

                // Обслуживающий персонал
                new Position { Id = 25, Title = "Водитель автомобиля", DepartmentId = 5 },
                new Position { Id = 26, Title = "Электромонтер", DepartmentId = 5 },
                new Position { Id = 27, Title = "Слесарь-сантехник", DepartmentId = 5 },
                new Position { Id = 28, Title = "Рабочий по обслуживанию зданий", DepartmentId = 5 },
                new Position { Id = 29, Title = "Вахтер", DepartmentId = 5 },
                new Position { Id = 30, Title = "Сторож", DepartmentId = 5 },
                new Position { Id = 31, Title = "Садовник", DepartmentId = 5 },
                new Position { Id = 32, Title = "Уборщик помещений", DepartmentId = 5 },
                new Position { Id = 33, Title = "Уборщик территорий", DepartmentId = 5 },

                // Общежитие
                new Position { Id = 34, Title = "Заведующий общежитием", DepartmentId = 6 },
                new Position { Id = 35, Title = "Рабочий по комплексному обслуживанию", DepartmentId = 6 },
                new Position { Id = 36, Title = "Вахтер", DepartmentId = 6 },
                new Position { Id = 37, Title = "Сторож", DepartmentId = 6 },
                new Position { Id = 38, Title = "Уборщик помещений", DepartmentId = 6 }
            );

            modelBuilder.Entity<SalaryRate>().HasData(
                new SalaryRate { Id = 1, PositionId = 1, Amount = 62090 },
                new SalaryRate { Id = 2, PositionId = 2, Amount = 47275 },
                new SalaryRate { Id = 3, PositionId = 3, Amount = 34120 },
                new SalaryRate { Id = 4, PositionId = 4, Amount = 34580 },
                new SalaryRate { Id = 5, PositionId = 5, Amount = 56607 },
                new SalaryRate { Id = 6, PositionId = 6, Amount = 41716 },
                new SalaryRate { Id = 7, PositionId = 7, Amount = 42206 },
                new SalaryRate { Id = 8, PositionId = 8, Amount = 30445 },
                new SalaryRate { Id = 9, PositionId = 9, Amount = 37245 },
                new SalaryRate { Id = 10, PositionId = 10, Amount = 35678 },
                new SalaryRate { Id = 11, PositionId = 11, Amount = 34870 },
                new SalaryRate { Id = 12, PositionId = 12, Amount = 31237 },
                new SalaryRate { Id = 13, PositionId = 13, Amount = 33000 },
                new SalaryRate { Id = 14, PositionId = 14, Amount = 36110 },
                new SalaryRate { Id = 15, PositionId = 15, Amount = 36500 },
                new SalaryRate { Id = 16, PositionId = 16, Amount = 29400 },
                new SalaryRate { Id = 17, PositionId = 17, Amount = 31500 },
                new SalaryRate { Id = 18, PositionId = 18, Amount = 32000 },
                new SalaryRate { Id = 19, PositionId = 19, Amount = 28950 },
                new SalaryRate { Id = 20, PositionId = 20, Amount = 37500 },
                new SalaryRate { Id = 21, PositionId = 21, Amount = 41500 },
                new SalaryRate { Id = 22, PositionId = 22, Amount = 29000 },
                new SalaryRate { Id = 23, PositionId = 23, Amount = 25650 },
                new SalaryRate { Id = 24, PositionId = 24, Amount = 30200 },
                new SalaryRate { Id = 25, PositionId = 25, Amount = 26500 },
                new SalaryRate { Id = 26, PositionId = 26, Amount = 28000 },
                new SalaryRate { Id = 27, PositionId = 27, Amount = 27000 },
                new SalaryRate { Id = 28, PositionId = 28, Amount = 26000 },
                new SalaryRate { Id = 29, PositionId = 29, Amount = 22000 },
                new SalaryRate { Id = 30, PositionId = 30, Amount = 23000 },
                new SalaryRate { Id = 31, PositionId = 31, Amount = 25000 },
                new SalaryRate { Id = 32, PositionId = 32, Amount = 21000 },
                new SalaryRate { Id = 33, PositionId = 33, Amount = 20000 },
                new SalaryRate { Id = 34, PositionId = 34, Amount = 30000 },
                new SalaryRate { Id = 35, PositionId = 35, Amount = 24000 },
                new SalaryRate { Id = 36, PositionId = 36, Amount = 22000 },
                new SalaryRate { Id = 37, PositionId = 37, Amount = 23000 },
                new SalaryRate { Id = 38, PositionId = 38, Amount = 21000 }
            );

        }
    }
}


