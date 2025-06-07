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
                    doc.ReplaceText("<Surename>", selectedEmployee.Surename);
                    doc.ReplaceText("<FirstName>", selectedEmployee.FirstName);
                    doc.ReplaceText("<SecondName>", selectedEmployee.SecondName);
                    doc.ReplaceText("<StartDate>", StartDateTextBox.Text.Trim());
                    doc.ReplaceText("<EndDate>", EndDateTextBox.Text.Trim());
                    doc.ReplaceText("<Days>", DaysTextBox.Text.Trim());
                    doc.ReplaceText("<Base>", BaseTextBox.Text.Trim());
                    doc.ReplaceText("<VacationType>", (VacationTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "—");
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
