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

namespace HRApp.Views
{
    public partial class BusinessTripWindow : Window
    {
        private List<Employee> employees;

        private static readonly string[] Months = new[]
        {
            "января", "февраля", "марта", "апреля", "мая", "июня",
            "июля", "августа", "сентября", "октября", "ноября", "декабря"
        };

        public BusinessTripWindow()
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
                MessageBox.Show("Дата окончания раньше даты начала.");
                return;
            }

            var destination = DestinationTextBox.Text.Trim();
            var purpose = PurposeTextBox.Text.Trim();

            using var context = new HRDbContext();

            // 1. Сохраняем запись о командировке
            var trip = new BusinessTrip
            {
                TripStartDate = startDate,
                TripEndDate = endDate,
                Destination = destination,
                Purpose = purpose,
                EmployeeId = selectedEmployee.Id
            };
            context.BusinessTrips.Add(trip);

            // 2. Генерируем приказ
            var order = new Order
            {
                EmployeeId = selectedEmployee.Id,
                StartDate = startDate,
                Content = "Приказ о командировке",
                DocDate = DateTime.Today,
                Base = purpose,
                RegNumber = OrderNumberGenerator.Generate(context, "КМД")
            };
            context.Orders.Add(order);

            // 3. Регистрируем приказ
            context.RegisterDocuments.Add(new RegisterDocument
            {
                RegNumber = order.RegNumber,
                RegDate = DateTime.Today,
                DocType = "Приказ о командировке"
            });

            context.SaveChanges();

            MessageBox.Show($"Командировка зарегистрирована. Приказ № {order.RegNumber}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "orderBusinessTrip.docx");
            if (!File.Exists(templatePath))
            {
                MessageBox.Show($"Шаблон приказа не найден по пути:\n{templatePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Командировка_{selectedEmployee.Surename}.docx"
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
                    string regNumber = OrderNumberGenerator.Generate(context, "КМД");

                    string MonthName(DateTime d) => Months[d.Month - 1];

                    // ФИО и общие данные
                    doc.ReplaceText("<Esurename>", selectedEmployee.Surename ?? string.Empty);
                    doc.ReplaceText("<Efirstname>", selectedEmployee.FirstName ?? string.Empty);
                    doc.ReplaceText("<Esecondname>", selectedEmployee.SecondName ?? string.Empty);
                    doc.ReplaceText("<Servicenumber>", selectedEmployee.TabNumber ?? string.Empty);
                    doc.ReplaceText("<workplacetype>", department);
                    doc.ReplaceText("<work>", position);

                    // Сведения о командировке
                    doc.ReplaceText("<aimPlace>", DestinationTextBox.Text.Trim());
                    doc.ReplaceText("<aim>", PurposeTextBox.Text.Trim());
                    doc.ReplaceText("<tripDays>", ((endDate - startDate).Days + 1).ToString());
                    doc.ReplaceText("<sd>", startDate.ToString("dd"));
                    doc.ReplaceText("<smonth>", MonthName(startDate));
                    doc.ReplaceText("<sy>", startDate.ToString("yy"));
                    doc.ReplaceText("<ed>", endDate.ToString("dd"));
                    doc.ReplaceText("<emonth>", MonthName(endDate));
                    doc.ReplaceText("<ey>", endDate.ToString("yy"));
                    doc.ReplaceText("<orederInfo>", PurposeTextBox.Text.Trim());
                    doc.ReplaceText("<Dwork>", "Директор");

                    // Номер приказа и дата
                    doc.ReplaceText("<regNumber>", regNumber);
                    doc.ReplaceText("<DateTime.Now>", DateTime.Today.ToString("dd.MM.yyyy"));

                    doc.SaveAs(sfd.FileName);
                    MessageBox.Show("Приказ сохранён.");

                    GenerateTripCertificate(selectedEmployee, position, department,
                        startDate, endDate, DestinationTextBox.Text.Trim(),
                        PurposeTextBox.Text.Trim(), regNumber);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}");
                }
            }
        }

        private void GenerateTripCertificate(Employee employee, string position,
            string department, DateTime startDate, DateTime endDate,
            string destination, string purpose, string orderNumber)
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Templates", "travelCertificate_T10.docx");
            if (!File.Exists(templatePath))
            {
                MessageBox.Show($"Шаблон удостоверения не найден по пути:\n{templatePath}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"T10_{employee.Surename}.docx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var doc = DocX.Load(templatePath);

                    string MonthName(DateTime d) => Months[d.Month - 1];

                    doc.ReplaceText("<Esurename>", employee.Surename ?? string.Empty);
                    doc.ReplaceText("<Efirstname>", employee.FirstName ?? string.Empty);
                    doc.ReplaceText("<Esecondname>", employee.SecondName ?? string.Empty);
                    doc.ReplaceText("<Servicenumber>", employee.TabNumber ?? string.Empty);
                    doc.ReplaceText("<workplacetype>", department);
                    doc.ReplaceText("<work>", position);
                    doc.ReplaceText("<tripDays>", ((endDate - startDate).Days + 1).ToString());
                    doc.ReplaceText("<sd>", startDate.ToString("dd"));
                    doc.ReplaceText("<smonth>", MonthName(startDate));
                    doc.ReplaceText("<sy>", startDate.ToString("yy"));
                    doc.ReplaceText("<ed>", endDate.ToString("dd"));
                    doc.ReplaceText("<emonth>", MonthName(endDate));
                    doc.ReplaceText("<ey>", endDate.ToString("yy"));
                    doc.ReplaceText("<regNumber>", orderNumber);
                    doc.ReplaceText("<DateTime.Now>", DateTime.Today.ToString("dd.MM.yyyy"));

                    doc.SaveAs(sfd.FileName);
                    MessageBox.Show("Удостоверение сохранено.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта удостоверения: {ex.Message}");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
