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

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Reports_T-10a.docx");
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
                    doc.ReplaceText("<Surename>", selectedEmployee.Surename);
                    doc.ReplaceText("<FirstName>", selectedEmployee.FirstName);
                    doc.ReplaceText("<SecondName>", selectedEmployee.SecondName);
                    doc.ReplaceText("<Destination>", DestinationTextBox.Text.Trim());
                    doc.ReplaceText("<Purpose>", PurposeTextBox.Text.Trim());
                    doc.ReplaceText("<StartDate>", StartDateTextBox.Text.Trim());
                    doc.ReplaceText("<EndDate>", EndDateTextBox.Text.Trim());
                    doc.ReplaceText("<DocDate>", DateTime.Today.ToShortDateString());

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
