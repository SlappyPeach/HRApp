using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using HRApp.Data;
using HRApp.Models;
using HRApp.Helpers;
using Microsoft.Win32;
using Xceed.Words.NET;

namespace HRApp.Views
{
    public partial class DismissWindow : Window
    {
        private List<Employee> employees = new();

        public DismissWindow()
        {
            InitializeComponent();
            LoadEmployees();
            DismissDateTextBox.Text = DateTime.Today.ToShortDateString();
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

            if (!DateTime.TryParse(DismissDateTextBox.Text, out var dismissDate))
            {
                MessageBox.Show("Некорректная дата увольнения.");
                return;
            }

            string baseText = BaseTextBox.Text.Trim();

            using var context = new HRDbContext();
            var emp = context.Employees.FirstOrDefault(e => e.Id == selectedEmployee.Id);
            if (emp == null) return;

            emp.DismissalDate = dismissDate;

            var order = new Order
            {
                EmployeeId = emp.Id,
                StartDate = dismissDate,
                Content = "Приказ об увольнении",
                DocDate = DateTime.Today,
                Base = baseText,
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

            var result = MessageBox.Show($"Сотрудник уволен. Приказ № {order.RegNumber} создан. Экспортировать документ?",
                "Успех", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ExportOrder(emp, order);
            }

            Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            if (!DateTime.TryParse(DismissDateTextBox.Text, out var dismissDate))
            {
                MessageBox.Show("Некорректная дата увольнения.");
                return;
            }

            var order = new Order
            {
                EmployeeId = selectedEmployee.Id,
                StartDate = dismissDate,
                Content = "Приказ об увольнении",
                DocDate = DateTime.Today,
                Base = BaseTextBox.Text.Trim(),
                RegNumber = OrderNumberGenerator.Generate(new HRDbContext(), "УВЛ")
            };

            ExportOrder(selectedEmployee, order);
        }

        private void ExportOrder(Employee employee, Order order)
        {
            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "orderDismissal.docx");
            if (!File.Exists(templatePath))
            {
                MessageBox.Show($"Шаблон приказа не найден по пути:\n{templatePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Увольнение_{employee.Surename}.docx"
            };

            if (sfd.ShowDialog() != true)
                return;

            try
            {
                var doc = DocX.Load(templatePath);

                doc.ReplaceText("<RegNumber>", order.RegNumber);
                doc.ReplaceText("<DocDate>", order.DocDate.ToShortDateString());
                doc.ReplaceText("<StartDate>", order.StartDate.ToShortDateString());
                doc.ReplaceText("<Surename>", employee.Surename ?? string.Empty);
                doc.ReplaceText("<FirstName>", employee.FirstName ?? string.Empty);
                doc.ReplaceText("<SecondName>", employee.SecondName ?? string.Empty);

                using var context = new HRDbContext();
                string department = employee.DepartmentId != null ?
                    context.Departments.FirstOrDefault(d => d.Id == employee.DepartmentId)?.Name ?? string.Empty : string.Empty;
                string position = employee.PositionId != null ?
                    context.Positions.FirstOrDefault(p => p.Id == employee.PositionId)?.Title ?? string.Empty : string.Empty;

                doc.ReplaceText("<Department>", department);
                doc.ReplaceText("<Position>", position);
                doc.ReplaceText("<Base>", order.Base);

                doc.SaveAs(sfd.FileName);
                MessageBox.Show("Приказ сохранён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}