﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HRApp.Data;
using HRApp.Models;
using Microsoft.Win32;
using Xceed.Words.NET;
using HRApp.Helpers;

namespace HRApp.Views
{
    public partial class HireWindow : Window
    {
        private Dictionary<string, decimal> positionSalaryMap = new();
        private decimal baseSalary = 0m;

        public HireWindow()
        {
            InitializeComponent();
            Loaded += HireWindow_Loaded;
        }

        private void HireWindow_Loaded(object sender, RoutedEventArgs e)
        {
            using var context = new HRDbContext();

            // Загрузка справочников
            AgreementTypeComboBox.ItemsSource = new List<string> { "Основной", "По совместительству", "Срочный договор" };
            ProbationComboBox.ItemsSource = new List<string> { "3 месяца", "6 месяцев", "Без испытательного срока" };
            RateComboBox.ItemsSource = new List<decimal> { 1m, 0.75m, 0.5m, 0.25m };
            RateComboBox.SelectionChanged += RateComboBox_SelectionChanged;
            RateComboBox.SelectedIndex = 0;

            var departments = context.Departments.Select(d => d.Name).ToList();
            DepartmentComboBox.ItemsSource = departments;
            DepartmentComboBox.SelectionChanged += DepartmentComboBox_SelectionChanged;

            // Оклад по должностям
            var positions = context.Positions.ToList();
            var salaryRates = context.SalaryRates.ToList();

            positionSalaryMap = positions
                .Join(salaryRates, p => p.Id, s => s.PositionId, (p, s) => new { p.Title, s.Amount })
                .GroupBy(x => x.Title)
                .ToDictionary(g => g.Key, g => g.First().Amount);
        }

        private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepartmentComboBox.SelectedItem is not string departmentName)
                return;

            using var context = new HRDbContext();
            var department = context.Departments.FirstOrDefault(d => d.Name == departmentName);
            if (department == null) return;

            var positions = context.Positions
                .Where(p => p.DepartmentId == department.Id)
                .Select(p => p.Title)
                .ToList();

            PositionComboBox.ItemsSource = positions;
            PositionComboBox.SelectedIndex = -1;

            LoadGrid.Visibility = Visibility.Collapsed;
            CategoryGrid.Visibility = Visibility.Collapsed;
            LoadTextBox.Text = "";
            RateComboBox.SelectedIndex = 0;
        }

        private void PositionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PositionComboBox.SelectedItem is not string positionTitle)
                return;

            string selectedDepartment = DepartmentComboBox.SelectedItem?.ToString() ?? "";

            bool isPedagogical = selectedDepartment == "Педагогический персонал";
            LoadGrid.Visibility = isPedagogical ? Visibility.Visible : Visibility.Collapsed;
            CategoryGrid.Visibility = isPedagogical ? Visibility.Visible : Visibility.Collapsed;

            if (positionSalaryMap.TryGetValue(positionTitle, out var salary))
            {
                baseSalary = salary;
                UpdateSalaryDisplay();
            }
            else
            {
                baseSalary = 0m;
                SalaryTextBox.Text = "";
            }
        }

        private void RateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSalaryDisplay();
        }

        private void UpdateSalaryDisplay()
        {
            if (!decimal.TryParse(RateComboBox.SelectedItem?.ToString(), out var rate))
                rate = 1m;

            SalaryTextBox.Text = (baseSalary * rate).ToString("F2");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DateTime.TryParse(WorkStartDateTextBox.Text, out var workStartDate))
            {
                MessageBox.Show("Некорректная дата начала работы.");
                return;
            }

            using var context = new HRDbContext();

            var selectedPositionTitle = PositionComboBox.Text.Trim();
            var position = context.Positions.FirstOrDefault(p => p.Title == selectedPositionTitle);

            var selectedDepartmentName = DepartmentComboBox.Text.Trim();
            var department = context.Departments.FirstOrDefault(d => d.Name == selectedDepartmentName);

            var employee = new Employee
            {
                Surename = SurenameTextBox.Text.Trim(),
                FirstName = FirstNameTextBox.Text.Trim(),
                SecondName = SecondNameTextBox.Text.Trim(),
                Sex = (SexComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Не указан",
                BirthDate = DateTime.TryParse(BirthDateTextBox.Text, out var birth) ? birth : DateTime.Today,
                BirthPlace = "-",
                BirthPlaceOKATO = "-",
                Citizenship = CitizenshipTextBox.Text.Trim(),
                CitizenshipOKIN = "-",
                PassportNumber = "-",
                PassportType = "-",
                PassportPlace = "-",
                PassportDate = DateTime.Today,
                INN = "-",
                SNILS = "-",
                NomerScheta = "-",
                TabNumber = TabNumberTextBox.Text.Trim(),
                BIK = "-",
                Korschet = "-",
                KPP = "-",
                Address1 = AddressTextBox.Text.Trim(),
                Address2 = "-",
                Index1 = "-",
                Index2 = "-",
                Address1Date = DateTime.Today,
                TelNumber = PhoneTextBox.Text.Trim(),
                FamilyStatus = "-",
                PositionId = position?.Id,
                DepartmentId = department?.Id
            };

            context.Employees.Add(employee);
            context.SaveChanges();

            bool pedagogical = DepartmentComboBox.Text == "Педагогический персонал";
            decimal.TryParse(RateComboBox.Text, out var genRate);

            var agreement = new Agreement
            {
                RegNumber = $"AGR-{DateTime.Now:yyyyMMddHHmmss}",
                AgreementDate = workStartDate,
                AgreementEndDate = workStartDate.AddYears(1),
                Probation = !ProbationComboBox.Text.Contains("Без"),
                PaySystem = AgreementTypeComboBox.Text,
                Salary = decimal.TryParse(SalaryTextBox.Text, out var salary) ? salary : 0,
                GeneralRate = genRate,
                Category = pedagogical ? CategoryComboBox.Text : null,
                Base = "-",
                FileName = "",
                EmployeeId = employee.Id
            };

            context.Agreements.Add(agreement);
            context.SaveChanges();

            var order = new Order
            {
                EmployeeId = employee.Id,
                StartDate = workStartDate,
                Content = "Приказ о приёме",
                DocDate = DateTime.Today,
                Base = "-",
                RegNumber = OrderNumberGenerator.Generate(context, "ПРИ"),
                AgreementId = agreement.Id
            };

            context.Orders.Add(order);
            context.RegisterDocuments.Add(new RegisterDocument
            {
                RegNumber = order.RegNumber,
                RegDate = DateTime.Today,
                DocType = "Приказ о приёме"
            });

            context.SaveChanges();

            // Новая запись в трудовой книжке
            var historyEntry = new EmploymentHistory
            {
                RecordNumber = context.EmploymentHistories
                    .Where(h => h.EmployeeId == employee.Id)
                    .Count() + 1,
                Date = workStartDate,
                WorkPlaceName = "Организация",
                Position = PositionComboBox.Text,
                Content = "Принят на работу",
                Reason = order.RegNumber,
                EmployeeId = employee.Id
            };

            context.EmploymentHistories.Add(historyEntry);
            context.SaveChanges();

            GenerateContractForEmployee(employee, agreement);
            GenerateOrderForEmployee(employee, order);
            MessageBox.Show("Сотрудник успешно принят.");

            var t2Window = new T2FormWindow(employee.Id);
            t2Window.ShowDialog();

            Close();
        }

        private void GenerateContractForEmployee(Employee employee, Agreement agreement)
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "dogovor.docx");
            string outputPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\HRApp\Dogovors\ТрудовойДоговор_{employee.Surename}_{employee.FirstName}.docx";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            if (!File.Exists(templatePath))
            {
                MessageBox.Show("Шаблон трудового договора не найден.");
                return;
            }

            var doc = DocX.Load(templatePath);

            doc.ReplaceText("<Esurename>", employee.Surename ?? "");
            doc.ReplaceText("<Efirstname>", employee.FirstName ?? "");
            doc.ReplaceText("<Esecondname>", employee.SecondName ?? "");
            doc.ReplaceText("<regNumber>", agreement.RegNumber);
            doc.ReplaceText("<docDate>", agreement.AgreementDate.ToString("dd.MM.yyyy"));

            doc.ReplaceText("<work>", PositionComboBox.Text);
            doc.ReplaceText("<department>", DepartmentComboBox.Text);
            doc.ReplaceText("<oklad>", SalaryTextBox.Text);
            doc.ReplaceText("<salary>", SalaryTextBox.Text);
            doc.ReplaceText("<agreementType>", AgreementTypeComboBox.Text);
            doc.ReplaceText("<paySystem>", AgreementTypeComboBox.Text);
            doc.ReplaceText("<Probation>", ProbationComboBox.Text);
            doc.ReplaceText("<workDate>", WorkStartDateTextBox.Text);
            doc.ReplaceText("<payday>", "дважды в месяц");
            doc.ReplaceText("<personalAcc>", employee.NomerScheta ?? "");
            doc.ReplaceText("<BIK>", employee.BIK ?? "");
            doc.ReplaceText("<additionalAcc>", employee.Korschet ?? "");
            doc.ReplaceText("<INN>", employee.INN ?? "");
            doc.ReplaceText("<KPP>", employee.KPP ?? "");
            doc.ReplaceText("<DateTime.Now>", DateTime.Now.ToString("dd.MM.yyyy"));

            // Только для "Педагогический персонал"
            bool isPedagogical = DepartmentComboBox.Text == "Педагогический персонал";
            doc.ReplaceText("<load>", isPedagogical ? LoadTextBox.Text : "");
            doc.ReplaceText("<rate>", isPedagogical ? RateComboBox.Text : "");
            doc.ReplaceText("<Category>", isPedagogical ? CategoryComboBox.Text : "");
            doc.ReplaceText("<GeneralRate>", RateComboBox.Text);
            doc.SaveAs(outputPath);
            MessageBox.Show($"Трудовой договор сохранён:\n{outputPath}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateOrderForEmployee(Employee employee, Order order)
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "orderAgreement.docx");
            string outputPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\HRApp\Orders\Приказ_{employee.Surename}_{employee.FirstName}.docx";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            if (!File.Exists(templatePath))
            {
                MessageBox.Show("Шаблон приказа не найден.");
                return;
            }

            var doc = DocX.Load(templatePath);

            doc.ReplaceText("<DocNumber>", order.RegNumber ?? "");
            doc.ReplaceText("<DocDate>", order.DocDate.ToString("dd.MM.yyyy"));
            doc.ReplaceText("<Surename>", employee.Surename ?? "");
            doc.ReplaceText("<FirstName>", employee.FirstName ?? "");
            doc.ReplaceText("<SecondName>", employee.SecondName ?? "");
            doc.ReplaceText("<Department>", DepartmentComboBox.Text);
            doc.ReplaceText("<Position>", PositionComboBox.Text);
            doc.ReplaceText("<Base>", order.Base ?? "-");
            doc.ReplaceText("<Salary>", SalaryTextBox.Text);
            doc.ReplaceText("<Probation>", ProbationComboBox.Text);
            doc.ReplaceText("<HireDate>", WorkStartDateTextBox.Text);
            doc.ReplaceText("<TabNumber>", employee.TabNumber ?? "");
            doc.ReplaceText("<GeneralRate>", RateComboBox.Text);
            doc.ReplaceText("<Category>", DepartmentComboBox.Text == "Педагогический персонал" ? CategoryComboBox.Text : "");
            doc.SaveAs(outputPath);
            MessageBox.Show($"Приказ о приёме сохранён:\n{outputPath}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
                
    }
}
