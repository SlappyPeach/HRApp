using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using HRApp.Data;
using HRApp.Models;
using Microsoft.Win32;
using Xceed.Words.NET;
using HRApp.Helpers;
using System.Windows.Controls;

namespace HRApp.Views
{
    public partial class VacationWindow : Window
    {
        private List<Employee> employees;

        private static readonly string[] Months = new[]
        {
            "января", "февраля", "марта", "апреля", "мая", "июня",
            "июля", "августа", "сентября", "октября", "ноября", "декабря"
        };

        public VacationWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using var context = new HRDbContext();
            employees = context.Employees.ToList();
            EmployeeComboBox.ItemsSource = employees;
            EmployeeComboBox.DisplayMemberPath = "Surename";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            if (!DateTime.TryParse(StartDateTextBox.Text, out var startDate) ||
                !DateTime.TryParse(EndDateTextBox.Text, out var endDate))
            {
                MessageBox.Show("Неверный формат дат.");
                return;
            }

            if (endDate < startDate)
            {
                MessageBox.Show("Дата окончания раньше начала.");
                return;
            }

            if (!int.TryParse(DaysTextBox.Text, out var days) || days <= 0)
            {
                MessageBox.Show("Неверное количество дней.");
                return;
            }

            var vacationType = (VacationTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "—";
            var baseText = BaseTextBox.Text.Trim();

            using var context = new HRDbContext();

            // 1. Сохраняем отпуск
            var vacation = new Vacation
            {
                VacationType = vacationType,
                WorkFrom = startDate.ToShortDateString(),
                WorkTo = endDate.ToShortDateString(),
                CalendarDaysNumber = days,
                StartDate = startDate,
                EndDate = endDate,
                Base = baseText,
                EmployeeId = selectedEmployee.Id
            };
            context.Vacations.Add(vacation);

            // 2. Генерация приказа
            var order = new Order
            {
                EmployeeId = selectedEmployee.Id,
                StartDate = startDate,
                Content = "Приказ об отпуске",
                DocDate = DateTime.Today,
                Base = baseText,
                RegNumber = OrderNumberGenerator.Generate(context, "ОТП")
            };
            context.Orders.Add(order);

            // 3. Регистрация приказа
            context.RegisterDocuments.Add(new RegisterDocument
            {
                RegNumber = order.RegNumber,
                RegDate = DateTime.Today,
                DocType = "Приказ об отпуске"
            });

            context.SaveChanges();

            MessageBox.Show($"Отпуск сохранён. Приказ № {order.RegNumber}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "orderVacation.docx");
            if (!File.Exists(templatePath))
            {
                MessageBox.Show($"Шаблон приказа не найден по пути:\n{templatePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Отпуск_{selectedEmployee.Surename}.docx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var doc = DocX.Load(templatePath);

                    // Плейсхолдеры
                    DateTime.TryParse(StartDateTextBox.Text, out var startDate);
                    DateTime.TryParse(EndDateTextBox.Text, out var endDate);

                    using var context = new HRDbContext();

                    string department = selectedEmployee.DepartmentId != null ?
                        context.Departments.FirstOrDefault(d => d.Id == selectedEmployee.DepartmentId)?.Name ?? string.Empty : string.Empty;
                    string position = selectedEmployee.PositionId != null ?
                        context.Positions.FirstOrDefault(p => p.Id == selectedEmployee.PositionId)?.Title ?? string.Empty : string.Empty;
                    string regNumber = OrderNumberGenerator.Generate(context, "ОТП");

                    string MonthName(DateTime d) => Months[d.Month - 1];

                    // ФИО и общие данные
                    doc.ReplaceText("<surename>", selectedEmployee.Surename ?? "");
                    doc.ReplaceText("<firstname>", selectedEmployee.FirstName ?? "");
                    doc.ReplaceText("<secondname>", selectedEmployee.SecondName ?? "");
                    doc.ReplaceText("<Servicenumber>", selectedEmployee.TabNumber ?? string.Empty);
                    doc.ReplaceText("<workplacetype>", department);
                    doc.ReplaceText("<work>", position);

                    // Тип отпуска и основание
                    doc.ReplaceText("<VacationType>", (VacationTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "—");
                    doc.ReplaceText("<vacationDaysA>", DaysTextBox.Text.Trim());
                    doc.ReplaceText("<vacationDaysB>", DaysTextBox.Text.Trim());
                    doc.ReplaceText("<vacationDaysC>", DaysTextBox.Text.Trim());
                    doc.ReplaceText("<Base>", BaseTextBox.Text.Trim());

                    // Даты периода работы
                    doc.ReplaceText("<wsd>", startDate.ToString("dd"));
                    doc.ReplaceText("<wsmonth>", MonthName(startDate));
                    doc.ReplaceText("<wsy>", startDate.ToString("yy"));
                    doc.ReplaceText("<wed>", endDate.ToString("dd"));
                    doc.ReplaceText("<wemonth>", MonthName(endDate));
                    doc.ReplaceText("<wey>", endDate.ToString("yy"));

                    // Даты отпуска (части А, B, C)
                    void FillDates(string prefixStartDay, string prefixMonth, string prefixYear,
                                   string prefixEndDay, string prefixEndMonth, string prefixEndYear)
                    {
                        doc.ReplaceText(prefixStartDay, startDate.ToString("dd"));
                        doc.ReplaceText(prefixMonth, MonthName(startDate));
                        doc.ReplaceText(prefixYear, startDate.ToString("yy"));
                        doc.ReplaceText(prefixEndDay, endDate.ToString("dd"));
                        doc.ReplaceText(prefixEndMonth, MonthName(endDate));
                        doc.ReplaceText(prefixEndYear, endDate.ToString("yy"));
                    }

                    FillDates("<sda>", "<smontha>", "<sya>", "<eda>", "<emontha>", "<eya>");
                    FillDates("<sdb>", "<smonthb>", "<syb>", "<edb>", "<emonthb>", "<eyb>");
                    FillDates("<sdc>", "<smonthc>", "<syc>", "<edc>", "<emonthc>", "<eyc>");

                    // Дата и номер приказа
                    doc.ReplaceText("<datetimepicker1>", DateTime.Today.ToString("dd.MM.yyyy"));
                    doc.ReplaceText("<regDate>", regNumber);

                    doc.SaveAs(sfd.FileName);
                    MessageBox.Show("Приказ сохранён.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
