using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HRApp.Commands;
using HRApp.Data;
using HRApp.Models;
using HRApp.Helpers;

using Xceed.Document.NET;
using Xceed.Words.NET;
namespace HRApp.ViewModels
{
    public class T2FormViewModel : INotifyPropertyChanged
    {
        private Employee employee;
        private MilitaryRegistration? military;
        private readonly List<ForeignLanguage> foreignLanguages;

        public Employee Employee
        {
            get => employee;
            set { employee = value; OnPropertyChanged(nameof(Employee)); }
        }

        public MilitaryRegistration? MilitaryData
        {
            get => military;
            set { military = value; OnPropertyChanged(nameof(MilitaryData)); }
        }

        public ObservableCollection<Education> EducationList { get; } = new();
        public ObservableCollection<Family> FamilyList { get; } = new();
        public ObservableCollection<LanguageDisplay> LanguageList { get; } = new();
        public ObservableCollection<Certification> CertificationList { get; } = new();
        public ObservableCollection<Course> CourseList { get; } = new();
        public ObservableCollection<EmployeeAward> AwardList { get; } = new();
        public ObservableCollection<SocialBenefit> BenefitList { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand DismissCommand { get; }
        public ICommand ExportCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public T2FormViewModel(int? empId = null)
        {
            using var context = new HRDbContext();
            context.Database.EnsureCreated();

            foreignLanguages = context.ForeignLanguages.ToList();

            if (empId.HasValue)
            {
                LoadEmployee(context, empId.Value);
                LoadDynamicData(context, empId.Value);
                MilitaryData = context.MilitaryRegistrations.FirstOrDefault(m => m.EmployeeId == empId.Value);
            }
            else
            {
                Employee = new Employee();
            }

            SaveCommand = new RelayCommand(o => Save(o));
            DismissCommand = new RelayCommand(o => Dismiss(o), o => Employee.Id != 0);
            ExportCommand = new RelayCommand(o => Export(o));
        }

        private void LoadEmployee(HRDbContext context, int id)
        {
            Employee = context.Employees.FirstOrDefault(e => e.Id == id) ?? new Employee();
        }

        private void LoadDynamicData(HRDbContext context, int empId)
        {
            EducationList.Clear();
            foreach (var edu in context.Educations.Where(e => e.EmployeeId == empId))
                EducationList.Add(edu);

            FamilyList.Clear();
            foreach (var fam in context.Families.Where(f => f.EmployeeId == empId))
                FamilyList.Add(fam);

            LanguageList.Clear();
            foreach (var lang in context.Languages.Where(l => l.EmployeeId == empId))
            {
                var foreignLang = foreignLanguages.FirstOrDefault(f => f.OKIN == lang.OKIN);
                string langName = foreignLang?.LanguageName ?? "Неизвестно";
                LanguageList.Add(new LanguageDisplay { LanguageName = langName, LanguageLevel = lang.OKINLevel });
            }

            CertificationList.Clear();
            foreach (var cert in context.Certifications.Where(c => c.EmployeeId == empId))
                CertificationList.Add(cert);

            CourseList.Clear();
            foreach (var course in context.Courses.Where(c => c.EmployeeId == empId))
                CourseList.Add(course);

            AwardList.Clear();
            foreach (var award in context.EmployeeAwards.Where(a => a.EmployeeId == empId))
                AwardList.Add(award);

            BenefitList.Clear();
            foreach (var benefit in context.SocialBenefits.Where(b => b.EmployeeId == empId))
                BenefitList.Add(benefit);
        }

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(Employee.Surename))
            {
                MessageBox.Show("Фамилия обязательна.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Employee.BirthPlaceOKATO))
            {
                MessageBox.Show("ОКАТО обязательно.");
                return false;
            }
            if (!long.TryParse(Employee.INN, out _))
            {
                MessageBox.Show("ИНН должен содержать только цифры.");
                return false;
            }
            return true;
        }

        private void Save(object? param)
        {
            if (!ValidateFields()) return;

            using var context = new HRDbContext();
            if (Employee.Id == 0)
            {
                context.Employees.Add(Employee);
            }
            else
            {
                var existing = context.Employees.FirstOrDefault(e => e.Id == Employee.Id);
                if (existing != null)
                {
                    context.Entry(existing).CurrentValues.SetValues(Employee);
                }
            }

            SaveDynamicData(context);
            context.SaveChanges();
            MessageBox.Show("Сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            if (param is Window w) w.Close();
        }

        private void SaveDynamicData(HRDbContext context)
        {
            var oldEdu = context.Educations.Where(e => e.EmployeeId == Employee.Id).ToList();
            context.Educations.RemoveRange(oldEdu);
            foreach (var edu in EducationList)
            {
                edu.EmployeeId = Employee.Id;
                context.Educations.Add(edu);
            }

            var oldFam = context.Families.Where(f => f.EmployeeId == Employee.Id).ToList();
            context.Families.RemoveRange(oldFam);
            foreach (var fam in FamilyList)
            {
                fam.EmployeeId = Employee.Id;
                context.Families.Add(fam);
            }

            var oldLang = context.Languages.Where(l => l.EmployeeId == Employee.Id).ToList();
            context.Languages.RemoveRange(oldLang);
            foreach (var lang in LanguageList)
            {
                var foreign = foreignLanguages.FirstOrDefault(f => f.LanguageName == lang.LanguageName);
                context.Languages.Add(new Language
                {
                    OKIN = foreign?.OKIN ?? "",
                    OKINLevel = lang.LanguageLevel,
                    EmployeeId = Employee.Id
                });
            }
        }

        private void Dismiss(object? param)
        {
            Employee.DismissalDate = DateTime.Today;

            using var context = new HRDbContext();
            var existing = context.Employees.FirstOrDefault(e => e.Id == Employee.Id);

            if (existing != null)
            {
                existing.DismissalDate = DateTime.Today;

                var order = new Order
                {
                    EmployeeId = existing.Id,
                    StartDate = DateTime.Today,
                    Content = "Приказ об увольнении",
                    DocDate = DateTime.Today,
                    Base = "Увольнение по инициативе работодателя",
                    RegNumber = OrderNumberGenerator.Generate(context, "УВЛ")
                };
                context.Orders.Add(order);

                context.RegisterDocuments.Add(new RegisterDocument
                {
                    RegNumber = order.RegNumber,
                    RegDate = DateTime.Today,
                    DocType = "Приказ об увольнении"
                });

                context.SaveChanges();
                MessageBox.Show($"Сотрудник уволен. Приказ № {order.RegNumber} создан и зарегистрирован.", "Успех");
                if (param is Window w) w.Close();
            }
        }

        private void Export(object? param)
        {
            var sfd = new Microsoft.Win32.SaveFileDialog { Filter = "Word Files (*.docx)|*.docx", FileName = "T2Form.docx" };
            if (sfd.ShowDialog() != true) return;

            string template = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "table_T-2.docx");
            if (!System.IO.File.Exists(template))
            {
                MessageBox.Show($"Шаблон не найден по пути:\n{template}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var doc = DocX.Load(template);

            doc.ReplaceText("<Surename>", Employee.Surename ?? "");
            doc.ReplaceText("<FirstName>", Employee.FirstName ?? "");
            doc.ReplaceText("<SecondName>", Employee.SecondName ?? "");
            doc.ReplaceText("<BirthDate>", Employee.BirthDate.ToShortDateString());
            doc.ReplaceText("<Sex>", Employee.Sex ?? "");
            doc.ReplaceText("<BirthPlace>", Employee.BirthPlace ?? "");
            doc.ReplaceText("<Citizenship>", Employee.Citizenship ?? "");
            doc.ReplaceText("<PassportNumber>", Employee.PassportNumber ?? "");
            doc.ReplaceText("<PassportDate>", Employee.PassportDate.ToShortDateString());
            doc.ReplaceText("<PassportPlace>", Employee.PassportPlace ?? "");
            doc.ReplaceText("<INN>", Employee.INN ?? "");
            doc.ReplaceText("<SNILS>", Employee.SNILS ?? "");
            doc.ReplaceText("<Address1>", Employee.Address1 ?? "");
            doc.ReplaceText("<Index1>", Employee.Index1 ?? "");
            doc.ReplaceText("<Address1Date>", Employee.Address1Date.ToShortDateString());
            doc.ReplaceText("<Address2>", Employee.Address2 ?? "");
            doc.ReplaceText("<Index2>", Employee.Index2 ?? "");
            doc.ReplaceText("<TelNumber>", Employee.TelNumber ?? "");
            doc.ReplaceText("<FamilyStatus>", Employee.FamilyStatus ?? "");
            doc.ReplaceText("<DismissalDate>", Employee.DismissalDate?.ToShortDateString() ?? "—");
            doc.ReplaceText("<BankAccount>", Employee.NomerScheta ?? "");
            doc.ReplaceText("<BIK>", Employee.BIK ?? "");
            doc.ReplaceText("<Korschet>", Employee.Korschet ?? "");
            doc.ReplaceText("<KPP>", Employee.KPP ?? "");

            if (MilitaryData != null)
            {
                doc.ReplaceText("<StockCategory>", MilitaryData.StockCategory ?? "");
                doc.ReplaceText("<Rank>", MilitaryData.Rank ?? "");
                doc.ReplaceText("<Compound>", MilitaryData.Compound ?? "");
                doc.ReplaceText("<VYS>", MilitaryData.VYS ?? "");
                doc.ReplaceText("<MilitaryService>", MilitaryData.MilitaryService ?? "");
                doc.ReplaceText("<MillitaryOffice>", MilitaryData.MillitaryOffice ?? "");
                doc.ReplaceText("<AccountingGroup>", MilitaryData.AccountingGroup ?? "");
                doc.ReplaceText("<SpecAccounting>", MilitaryData.SpecAccounting ?? "");
                doc.ReplaceText("<DeregistrationNote>", MilitaryData.DeregistrationNote ?? "");
            }

            using var context = new HRDbContext();
            string positionTitle = "";
            if (Employee.PositionId.HasValue)
            {
                var pos = context.Positions.FirstOrDefault(p => p.Id == Employee.PositionId);
                if (pos != null)
                    positionTitle = pos.Title;
            }

            string departmentName = "";
            if (Employee.DepartmentId.HasValue)
            {
                var dep = context.Departments.FirstOrDefault(d => d.Id == Employee.DepartmentId);
                if (dep != null)
                    departmentName = dep.Name;
            }

            doc.ReplaceText("<Position>", positionTitle);
            doc.ReplaceText("<Department>", departmentName);

            InsertTableAtBookmark(doc, "LANG_TABLE",
                new[] { "Язык", "Уровень" },
                LanguageList.Select(l => new[] { l.LanguageName, l.LanguageLevel }));

            InsertTableAtBookmark(doc, "EDU_TABLE",
                new[] { "Тип", "Учреждение", "Документ", "Год" },
                EducationList.Select(e => new[] { e.EducationType, e.InstitutionName, e.DocName, e.EndYear.ToString() }));

            InsertTableAtBookmark(doc, "FAM_TABLE",
                new[] { "Родство", "ФИО", "Год рождения", "Пол", "Налоговый вычет" },
                FamilyList.Select(f => new[] { f.Relation, f.FIO, f.BirthYear.ToString(), f.Gender ?? "", f.HasTaxBenefit ? "Да" : "Нет" }));

            InsertTableAtBookmark(doc, "CERT_TABLE",
                new[] { "Дата аттестации", "Решение", "Категория", "Документ", "Дата следующей" },
                CertificationList.Select(c => new[]
                {
                    c.Date.ToShortDateString(), c.Resolution, c.Category,
                    $"{c.DocNumber} от {c.DocDate.ToShortDateString()}",
                    c.NextDate.ToShortDateString()
                }));

            InsertTableAtBookmark(doc, "COURSE_TABLE",
                new[] { "Тип", "Учреждение", "Документ", "Дата начала", "Дата окончания" },
                CourseList.Select(c => new[]
                {
                    c.CourseType, c.Institution, c.Certificate,
                    c.StartDate.ToShortDateString(), c.EndDate.ToShortDateString()
                }));

            InsertTableAtBookmark(doc, "AWARD_TABLE",
                new[] { "Дата", "Номер", "Подразделение", "Тип" },
                AwardList.Select(a => new[]
                {
                    a.AwardDate.ToShortDateString(),
                    a.AwardNumber,
                    a.Department,
                    a.AwardType
                }));

            InsertTableAtBookmark(doc, "BENEFIT_TABLE",
                new[] { "Вид льготы", "Документ", "Дата" },
                BenefitList.Select(b => new[] { b.Type, b.Document, b.Date.ToShortDateString() }));

            doc.SaveAs(sfd.FileName);
            MessageBox.Show("Экспорт завершён.");
        }

        private void InsertTableAtBookmark(DocX doc, string bookmarkName, string[] headers, IEnumerable<string[]> rows)
        {
            var bm = doc.Bookmarks.FirstOrDefault(b => b.Name == bookmarkName);
            if (bm == null) return;

            var table = doc.AddTable(rows.Count() + 1, headers.Length);
            table.Design = TableDesign.TableGrid;

            for (int c = 0; c < headers.Length; c++)
                table.Rows[0].Cells[c].Paragraphs[0].Append(headers[c]);

            int r = 1;
            foreach (var row in rows)
            {
                for (int c = 0; c < headers.Length && c < row.Length; c++)
                    table.Rows[r].Cells[c].Paragraphs[0].Append(row[c] ?? string.Empty);
                r++;
            }

            Paragraph host = bm.Paragraph;
            host.InsertTableAfterSelf(table);
            bm.Remove();
        }
    }

    public class LanguageDisplay
    {
        public string LanguageName { get; set; }
        public string LanguageLevel { get; set; }
    }
}