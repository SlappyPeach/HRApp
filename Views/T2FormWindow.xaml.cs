using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HRApp.Data;
using HRApp.Models;
using Microsoft.Win32;
using Xceed.Document.NET;
using Xceed.Words.NET;
using HRApp.Helpers;

namespace HRApp.Views
{
    public partial class T2FormWindow : Window
    {
        private int? employeeId;
        private Employee currentEmployee;
        private MilitaryRegistration militaryData;

        public ObservableCollection<Education> EducationList { get; set; } = new();
        public ObservableCollection<Family> FamilyList { get; set; } = new();
        public ObservableCollection<LanguageDisplay> LanguageList { get; set; } = new();
        public ObservableCollection<Certification> CertificationList { get; set; } = new();
        public ObservableCollection<Course> CourseList { get; set; } = new();
        public ObservableCollection<EmployeeAward> AwardList { get; set; } = new();
        public ObservableCollection<SocialBenefit> BenefitList { get; set; } = new();

        private List<ForeignLanguage> ForeignLanguagesCache;

        public T2FormWindow(int? empId = null)
        {
            InitializeComponent();
            DataContext = this;
            employeeId = empId;

            using var context = new HRDbContext();
            context.Database.EnsureCreated();

            ForeignLanguagesCache = context.ForeignLanguages.ToList();

            if (employeeId.HasValue)
            {
                LoadEmployee(employeeId.Value);
                LoadDynamicData(employeeId.Value);
                militaryData = context.MilitaryRegistrations.FirstOrDefault(m => m.EmployeeId == employeeId.Value);
            }
            else
            {
                currentEmployee = new Employee();
            }

            EducationDataGrid.ItemsSource = EducationList;
            FamilyDataGrid.ItemsSource = FamilyList;
            LanguageDataGrid.ItemsSource = LanguageList;
            CertificationDataGrid.ItemsSource = CertificationList;
            CourseDataGrid.ItemsSource = CourseList;
            AwardDataGrid.ItemsSource = AwardList;
            BenefitDataGrid.ItemsSource = BenefitList;
        }

        private void LoadEmployee(int id)
        {
            using var context = new HRDbContext();
            currentEmployee = context.Employees.FirstOrDefault(e => e.Id == id);
            if (currentEmployee == null) return;

            // Заполнение полей
            SurenameTextBox.Text = currentEmployee.Surename;
            FirstNameTextBox.Text = currentEmployee.FirstName;
            SecondNameTextBox.Text = currentEmployee.SecondName;
            BirthDateTextBox.Text = currentEmployee.BirthDate.ToShortDateString();
            BirthPlaceTextBox.Text = currentEmployee.BirthPlace;
            BirthPlaceOKATOTextBox.Text = currentEmployee.BirthPlaceOKATO;
            SexComboBox.Text = currentEmployee.Sex;
            CitizenshipTextBox.Text = currentEmployee.Citizenship;
            CitizenshipOKINTextBox.Text = currentEmployee.CitizenshipOKIN;
            PassportTypeTextBox.Text = currentEmployee.PassportType;
            PassportNumberTextBox.Text = currentEmployee.PassportNumber;
            PassportDateTextBox.Text = currentEmployee.PassportDate.ToShortDateString();
            PassportPlaceTextBox.Text = currentEmployee.PassportPlace;
            INNTextBox.Text = currentEmployee.INN;
            SNILSTextBox.Text = currentEmployee.SNILS;
            Address1TextBox.Text = currentEmployee.Address1;
            Index1TextBox.Text = currentEmployee.Index1;
            Address1DateTextBox.Text = currentEmployee.Address1Date.ToShortDateString();
            Address2TextBox.Text = currentEmployee.Address2;
            Index2TextBox.Text = currentEmployee.Index2;
            TelNumberTextBox.Text = currentEmployee.TelNumber;
            FamilyStatusTextBox.Text = currentEmployee.FamilyStatus;
            NomerSchetaTextBox.Text = currentEmployee.NomerScheta;
            BIKTextBox.Text = currentEmployee.BIK;
            KorschetTextBox.Text = currentEmployee.Korschet;
            KPPTextBox.Text = currentEmployee.KPP;

            // Получение связанных должности и подразделения
            if (currentEmployee.PositionId.HasValue)
            {
                var position = context.Positions.FirstOrDefault(p => p.Id == currentEmployee.PositionId);
                if (position != null)
                {
                    // отобрази в подходящем TextBox или ComboBox
                    PositionTextBox.Text = position.Title;
                }
            }

            if (currentEmployee.DepartmentId.HasValue)
            {
                var department = context.Departments.FirstOrDefault(d => d.Id == currentEmployee.DepartmentId);
                if (department != null)
                {
                    DepartmentTextBox.Text = department.Name;
                }
            }

        }

        private void LoadDynamicData(int empId)
        {
            using var context = new HRDbContext();

            EducationList.Clear();
            foreach (var edu in context.Educations.Where(e => e.EmployeeId == empId))
                EducationList.Add(edu);

            FamilyList.Clear();
            foreach (var fam in context.Families.Where(f => f.EmployeeId == empId))
                FamilyList.Add(fam);

            LanguageList.Clear();
            foreach (var lang in context.Languages.Where(l => l.EmployeeId == empId))
            {
                var foreignLang = ForeignLanguagesCache.FirstOrDefault(f => f.OKIN == lang.OKIN);
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

        private void AddEducation_Click(object sender, RoutedEventArgs e)
        {
            EducationList.Add(new Education { EducationType = "Тип", InstitutionName = "Учреждение", DocName = "Документ", EndYear = DateTime.Now.Year });
        }

        private void DeleteEducation_Click(object sender, RoutedEventArgs e)
        {
            if (EducationDataGrid.SelectedItem is Education edu)
                EducationList.Remove(edu);
        }

        private void AddFamily_Click(object sender, RoutedEventArgs e)
        {
            FamilyList.Add(new Family { Relation = "Родство", FIO = "ФИО", BirthYear = DateTime.Now.Year - 30 });
        }

        private void DeleteFamily_Click(object sender, RoutedEventArgs e)
        {
            if (FamilyDataGrid.SelectedItem is Family fam)
                FamilyList.Remove(fam);
        }

        private void AddLanguage_Click(object sender, RoutedEventArgs e)
        {
            LanguageList.Add(new LanguageDisplay { LanguageName = "Английский", LanguageLevel = "Intermediate" });
        }

        private void DeleteLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (LanguageDataGrid.SelectedItem is LanguageDisplay lang)
                LanguageList.Remove(lang);
        }

        private void AddCertification_Click(object sender, RoutedEventArgs e)
        {
            if (currentEmployee == null || currentEmployee.Id == 0)
            {
                MessageBox.Show("Сначала сохраните данные сотрудника.");
                return;
            }

            var certWindow = new CertificationWindow(currentEmployee.Id);
            certWindow.ShowDialog();

            // Перезагрузим список после закрытия окна
            LoadDynamicData(currentEmployee.Id);
        }

        private void DeleteCertification_Click(object sender, RoutedEventArgs e)
        {
            if (CertificationDataGrid.SelectedItem is Certification selectedCert)
            {
                var result = MessageBox.Show("Удалить выбранную аттестацию?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using var context = new HRDbContext();
                    var cert = context.Certifications.FirstOrDefault(c => c.Id == selectedCert.Id);
                    if (cert != null)
                    {
                        context.Certifications.Remove(cert);
                        context.SaveChanges();
                        LoadDynamicData(currentEmployee.Id);
                    }
                }
            }
        }

        private void AddCourse_Click(object sender, RoutedEventArgs e)
        {
            if (currentEmployee == null || currentEmployee.Id == 0)
            {
                MessageBox.Show("Сначала сохраните данные сотрудника.");
                return;
            }

            var courseWindow = new CourseWindow(currentEmployee.Id);
            courseWindow.ShowDialog();

            LoadDynamicData(currentEmployee.Id); // обновляем после закрытия
        }

        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
        {
            if (CourseDataGrid.SelectedItem is Course selectedCourse)
            {
                var result = MessageBox.Show("Удалить выбранный курс?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using var context = new HRDbContext();
                    var course = context.Courses.FirstOrDefault(c => c.Id == selectedCourse.Id);
                    if (course != null)
                    {
                        context.Courses.Remove(course);
                        context.SaveChanges();
                        LoadDynamicData(currentEmployee.Id);
                    }
                }
            }
        }

        private void AddAward_Click(object sender, RoutedEventArgs e)
        {
            if (currentEmployee == null || currentEmployee.Id == 0)

            {
                MessageBox.Show("Сначала сохраните данные сотрудника.");
                return;
            }
            
            var awardsWindow = new AwardsWindow(currentEmployee.Id);
            awardsWindow.ShowDialog();
            LoadDynamicData(currentEmployee.Id);
        }

        private void DeleteAward_Click(object sender, RoutedEventArgs e)
        {
            if (AwardDataGrid.SelectedItem is EmployeeAward selected)
            {
                using var context = new HRDbContext();
                var item = context.EmployeeAwards.FirstOrDefault(a => a.Id == selected.Id);
                if (item != null)
                {
                    context.EmployeeAwards.Remove(item);
                    context.SaveChanges();
                    LoadDynamicData(currentEmployee.Id);
                }
            }
        }

        private void AddBenefit_Click(object sender, RoutedEventArgs e)
        {
            var type = Microsoft.VisualBasic.Interaction.InputBox("Вид льготы:", "Добавить льготу");
            var document = Microsoft.VisualBasic.Interaction.InputBox("Документ:", "Добавить льготу");
            var dateText = Microsoft.VisualBasic.Interaction.InputBox("Дата предоставления (дд.мм.гггг):", "Добавить льготу");

            if (DateTime.TryParse(dateText, out var date) && !string.IsNullOrWhiteSpace(type))
            {
                using var context = new HRDbContext();
                var benefit = new SocialBenefit { Type = type, Document = document, Date = date, EmployeeId = currentEmployee.Id };
                context.SocialBenefits.Add(benefit);
                context.SaveChanges();
                LoadDynamicData(currentEmployee.Id);
            }
        }

        private void DeleteBenefit_Click(object sender, RoutedEventArgs e)
        {
            if (BenefitDataGrid.SelectedItem is SocialBenefit selected)
            {
                using var context = new HRDbContext();
                var item = context.SocialBenefits.FirstOrDefault(b => b.Id == selected.Id);
                if (item != null)
                {
                    context.SocialBenefits.Remove(item);
                    context.SaveChanges();
                    LoadDynamicData(currentEmployee.Id);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;

            // Заполнение модели из полей
            currentEmployee.Surename = SurenameTextBox.Text.Trim();
            currentEmployee.FirstName = FirstNameTextBox.Text.Trim();
            currentEmployee.SecondName = SecondNameTextBox.Text.Trim();
            currentEmployee.BirthPlace = BirthPlaceTextBox.Text.Trim();
            currentEmployee.BirthPlaceOKATO = BirthPlaceOKATOTextBox.Text.Trim();
            currentEmployee.Sex = (SexComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            currentEmployee.Citizenship = CitizenshipTextBox.Text.Trim();
            currentEmployee.CitizenshipOKIN = CitizenshipOKINTextBox.Text.Trim();
            currentEmployee.PassportType = PassportTypeTextBox.Text.Trim();
            currentEmployee.PassportNumber = PassportNumberTextBox.Text.Trim();
            currentEmployee.PassportPlace = PassportPlaceTextBox.Text.Trim();
            currentEmployee.INN = INNTextBox.Text.Trim();
            currentEmployee.SNILS = SNILSTextBox.Text.Trim();
            currentEmployee.Address1 = Address1TextBox.Text.Trim();
            currentEmployee.Index1 = Index1TextBox.Text.Trim();
            currentEmployee.Address2 = Address2TextBox.Text.Trim();
            currentEmployee.Index2 = Index2TextBox.Text.Trim();
            currentEmployee.TelNumber = TelNumberTextBox.Text.Trim();
            currentEmployee.FamilyStatus = FamilyStatusTextBox.Text.Trim();
            currentEmployee.NomerScheta = NomerSchetaTextBox.Text.Trim();
            currentEmployee.BIK = BIKTextBox.Text.Trim();
            currentEmployee.Korschet = KorschetTextBox.Text.Trim();
            currentEmployee.KPP = KPPTextBox.Text.Trim();

            DateTime.TryParse(BirthDateTextBox.Text, out var birthDate);
            currentEmployee.BirthDate = birthDate;

            DateTime.TryParse(PassportDateTextBox.Text, out var passDate);
            currentEmployee.PassportDate = passDate;

            DateTime.TryParse(Address1DateTextBox.Text, out var regDate);
            currentEmployee.Address1Date = regDate;

            using var context = new HRDbContext();
            if (currentEmployee.Id == 0)
            {
                context.Employees.Add(currentEmployee);
            }
            else
            {
                var existing = context.Employees.FirstOrDefault(e => e.Id == currentEmployee.Id);
                if (existing != null)
                {
                    context.Entry(existing).CurrentValues.SetValues(currentEmployee);
                }
            }

            SaveDynamicData(context);
            context.SaveChanges();
            MessageBox.Show("Сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();

        }

        private void SaveDynamicData(HRDbContext context)
        {
            var oldEdu = context.Educations.Where(e => e.EmployeeId == currentEmployee.Id).ToList();
            context.Educations.RemoveRange(oldEdu);
            foreach (var edu in EducationList)
            {
                edu.EmployeeId = currentEmployee.Id;
                context.Educations.Add(edu);
            }

            var oldFam = context.Families.Where(f => f.EmployeeId == currentEmployee.Id).ToList();
            context.Families.RemoveRange(oldFam);
            foreach (var fam in FamilyList)
            {
                fam.EmployeeId = currentEmployee.Id;
                context.Families.Add(fam);
            }

            var oldLang = context.Languages.Where(l => l.EmployeeId == currentEmployee.Id).ToList();
            context.Languages.RemoveRange(oldLang);
            foreach (var lang in LanguageList)
            {
                var foreign = ForeignLanguagesCache.FirstOrDefault(f => f.LanguageName == lang.LanguageName);
                context.Languages.Add(new Language
                {
                    OKIN = foreign?.OKIN ?? "",
                    OKINLevel = lang.LanguageLevel,
                    EmployeeId = currentEmployee.Id
                });
            }
        }

        private void Dismiss_Click(object sender, RoutedEventArgs e)
        {
            currentEmployee.DismissalDate = DateTime.Today;

            using var context = new HRDbContext();
            var existing = context.Employees.FirstOrDefault(e => e.Id == currentEmployee.Id);

            if (existing != null)
            {
                existing.DismissalDate = DateTime.Today;

                // Создаем приказ об увольнении
                var order = new Order
                {
                    EmployeeId = existing.Id,
                    StartDate = DateTime.Today,
                    Content = "Приказ об увольнении",
                    DocDate = DateTime.Today,
                    Base = "Увольнение по инициативе работодателя", // или основание из UI
                    RegNumber = OrderNumberGenerator.Generate(context, "УВЛ")
                };
                context.Orders.Add(order);

                // Регистрируем приказ в журнале
                context.RegisterDocuments.Add(new RegisterDocument
                {
                    RegNumber = order.RegNumber,
                    RegDate = DateTime.Today,
                    DocType = "Приказ об увольнении"
                });

                context.SaveChanges();
                MessageBox.Show($"Сотрудник уволен. Приказ № {order.RegNumber} создан и зарегистрирован.", "Успех");
                this.Close();
            }
        }


        private void ExportDocx_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "Word Files (*.docx)|*.docx", FileName = "T2Form.docx" };
            if (sfd.ShowDialog() != true) return;

            string template = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "table_T-2.docx");
            if (!File.Exists(template))
            {
                MessageBox.Show($"Шаблон не найден по пути:\n{template}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var doc = DocX.Load(template);

            // --- Базовая информация ---
            doc.ReplaceText("<Surename>", currentEmployee.Surename ?? "");
            doc.ReplaceText("<FirstName>", currentEmployee.FirstName ?? "");
            doc.ReplaceText("<SecondName>", currentEmployee.SecondName ?? "");
            doc.ReplaceText("<BirthDate>", currentEmployee.BirthDate.ToShortDateString());
            doc.ReplaceText("<Sex>", currentEmployee.Sex ?? "");
            doc.ReplaceText("<BirthPlace>", currentEmployee.BirthPlace ?? "");
            doc.ReplaceText("<Citizenship>", currentEmployee.Citizenship ?? "");
            doc.ReplaceText("<PassportNumber>", currentEmployee.PassportNumber ?? "");
            doc.ReplaceText("<PassportDate>", currentEmployee.PassportDate.ToShortDateString());
            doc.ReplaceText("<PassportPlace>", currentEmployee.PassportPlace ?? "");
            doc.ReplaceText("<INN>", currentEmployee.INN ?? "");
            doc.ReplaceText("<SNILS>", currentEmployee.SNILS ?? "");
            doc.ReplaceText("<Address1>", currentEmployee.Address1 ?? "");
            doc.ReplaceText("<Index1>", currentEmployee.Index1 ?? "");
            doc.ReplaceText("<Address1Date>", currentEmployee.Address1Date.ToShortDateString());
            doc.ReplaceText("<Address2>", currentEmployee.Address2 ?? "");
            doc.ReplaceText("<Index2>", currentEmployee.Index2 ?? "");
            doc.ReplaceText("<TelNumber>", currentEmployee.TelNumber ?? "");
            doc.ReplaceText("<FamilyStatus>", currentEmployee.FamilyStatus ?? "");
            doc.ReplaceText("<DismissalDate>", currentEmployee.DismissalDate?.ToShortDateString() ?? "—");
            doc.ReplaceText("<BankAccount>", currentEmployee.NomerScheta ?? "");
            doc.ReplaceText("<BIK>", currentEmployee.BIK ?? "");
            doc.ReplaceText("<Korschet>", currentEmployee.Korschet ?? "");
            doc.ReplaceText("<KPP>", currentEmployee.KPP ?? "");

            if (militaryData != null)
            {
                doc.ReplaceText("<StockCategory>", militaryData.StockCategory ?? "");
                doc.ReplaceText("<Rank>", militaryData.Rank ?? "");
                doc.ReplaceText("<Compound>", militaryData.Compound ?? "");
                doc.ReplaceText("<VYS>", militaryData.VYS ?? "");
                doc.ReplaceText("<MilitaryService>", militaryData.MilitaryService ?? "");
                doc.ReplaceText("<MillitaryOffice>", militaryData.MillitaryOffice ?? "");
                doc.ReplaceText("<AccountingGroup>", militaryData.AccountingGroup ?? "");
                doc.ReplaceText("<SpecAccounting>", militaryData.SpecAccounting ?? "");
                doc.ReplaceText("<DeregistrationNote>", militaryData.DeregistrationNote ?? "");
            }

            // Получение должности и подразделения
            using var context = new HRDbContext();

            string positionTitle = "";
            if (currentEmployee.PositionId.HasValue)
            {
                var pos = context.Positions.FirstOrDefault(p => p.Id == currentEmployee.PositionId);
                if (pos != null)
                    positionTitle = pos.Title;
            }

            string departmentName = "";
            if (currentEmployee.DepartmentId.HasValue)
            {
                var dep = context.Departments.FirstOrDefault(d => d.Id == currentEmployee.DepartmentId);
                if (dep != null)
                    departmentName = dep.Name;
            }

            doc.ReplaceText("<Position>", positionTitle);
            doc.ReplaceText("<Department>", departmentName);


            // --- Таблицы ---
            InsertTableAfterPlaceholder(doc, "<<<LANG_TABLE>>>", new[] { "Язык", "Уровень" },
                LanguageList.Select(l => new[] { l.LanguageName, l.LanguageLevel }));

            InsertTableAfterPlaceholder(doc, "<<<EDU_TABLE>>>", new[] { "Тип", "Учреждение", "Документ", "Год" },
                EducationList.Select(e => new[] { e.EducationType, e.InstitutionName, e.DocName, e.EndYear.ToString() }));

            InsertTableAfterPlaceholder(doc, "<<<FAM_TABLE>>>", new[] { "Родство", "ФИО", "Год рождения", "Пол", "Налоговый вычет" },
                FamilyList.Select(f => new[] { f.Relation, f.FIO, f.BirthYear.ToString(), f.Gender ?? "", f.HasTaxBenefit ? "Да" : "Нет" }));

            InsertTableAfterPlaceholder(doc, "<<<CERTIFICATION_TABLE>>>",
                new[] { "Дата аттестации", "Решение", "Категория", "Документ", "Дата следующей" },
                CertificationList.Select(c => new[] {
            c.Date.ToShortDateString(), c.Resolution, c.Category,
            $"{c.DocNumber} от {c.DocDate.ToShortDateString()}",
            c.NextDate.ToShortDateString()
                }));

            InsertTableAfterPlaceholder(doc, "<<<COURSE_TABLE>>>",
                new[] { "Тип", "Учреждение", "Документ", "Дата начала", "Дата окончания" },
                CourseList.Select(c => new[] {
            c.CourseType, c.Institution, c.Certificate,
            c.StartDate.ToShortDateString(), c.EndDate.ToShortDateString()
                }));

            InsertTableAfterPlaceholder(doc, "<<<AWARD_TABLE>>>",
                new[] { "Дата", "Номер", "Подразделение", "Тип" },
                AwardList.Select(a => new[]
                {
                    a.AwardDate.ToShortDateString(),
                    a.AwardNumber,
                    a.Department,
                    a.AwardType
                }));

            InsertTableAfterPlaceholder(doc, "<<<BENEFIT_TABLE>>>",
                new[] { "Вид льготы", "Документ", "Дата" },
                BenefitList.Select(b => new[] { b.Type, b.Document, b.Date.ToShortDateString() }));

            // --- Сохранение ---
            doc.SaveAs(sfd.FileName);
            MessageBox.Show("Экспорт завершён.");
        }

        private void InsertTableAfterPlaceholder(DocX doc, string placeholder, string[] headers, IEnumerable<string[]> rows)
        {
            var ph = doc.Paragraphs.FirstOrDefault(p => p.Text.Contains(placeholder));
            if (ph == null) return;

            ph.ReplaceText(placeholder, string.Empty);

            var table = doc.AddTable(rows.Count() + 1, headers.Length);
            table.Design = TableDesign.TableGrid;

            for (int i = 0; i < headers.Length; i++)
                table.Rows[0].Cells[i].Paragraphs[0].Append(headers[i]);

            int rowIndex = 1;
            foreach (var row in rows)
            {
                for (int col = 0; col < headers.Length && col < row.Length; col++)
                    table.Rows[rowIndex].Cells[col].Paragraphs[0].Append(row[col]);
                rowIndex++;
            }

            ph.InsertTableAfterSelf(table);
            if (string.IsNullOrWhiteSpace(ph.Text))
                ph.Remove();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private bool ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(SurenameTextBox.Text))
            {
                MessageBox.Show("Фамилия обязательна.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(BirthPlaceOKATOTextBox.Text))
            {
                MessageBox.Show("ОКАТО обязательно.");
                return false;
            }
            if (!long.TryParse(INNTextBox.Text, out _))
            {
                MessageBox.Show("ИНН должен содержать только цифры.");
                return false;
            }
            return true;
        }
    }

    public class LanguageDisplay
    {
        public string LanguageName { get; set; }
        public string LanguageLevel { get; set; }
    }
}
