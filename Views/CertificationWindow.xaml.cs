using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HRApp.Data;
using HRApp.Models;

namespace HRApp.Views
{
    public partial class CertificationWindow : Window
    {
        private List<Employee> employees;

        public CertificationWindow()
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

            if (!DateTime.TryParse(DateTextBox.Text, out var certDate) ||
                !DateTime.TryParse(DocDateTextBox.Text, out var docDate) ||
                !DateTime.TryParse(NextDateTextBox.Text, out var nextDate))
            {
                MessageBox.Show("Неверный формат дат.");
                return;
            }

            var cert = new Certification
            {
                Date = certDate,
                Resolution = ResolutionTextBox.Text.Trim(),
                Category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                DocNumber = DocNumberTextBox.Text.Trim(),
                DocDate = docDate,
                Base = BaseTextBox.Text.Trim(),
                NextDate = nextDate,
                EmployeeId = selectedEmployee.Id
            };

            using var context = new HRDbContext();
            context.Certifications.Add(cert);
            context.SaveChanges();

            MessageBox.Show("Аттестация сохранена.");
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
