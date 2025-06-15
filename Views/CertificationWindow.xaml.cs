using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HRApp.Data;
using HRApp.Models;
using Microsoft.Win32;
using ClosedXML.Excel;

namespace HRApp.Views
{
    public partial class CertificationWindow : Window
    {
        private List<Employee> employees;
        private readonly int? preselectedEmployeeId;

        public CertificationWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        public CertificationWindow(int employeeId) : this()
        {
            preselectedEmployeeId = employeeId;
            if (preselectedEmployeeId.HasValue)
            {
                EmployeeComboBox.SelectedValue = preselectedEmployeeId.Value;
                EmployeeComboBox.IsEnabled = false;
            }
        }

        private void LoadEmployees()
        {
            using var context = new HRDbContext();
            employees = context.Employees.ToList();
            EmployeeComboBox.ItemsSource = employees;
            EmployeeComboBox.DisplayMemberPath = "Surename";
            EmployeeComboBox.SelectedValuePath = "Id";

            if (preselectedEmployeeId.HasValue)
            {
                EmployeeComboBox.SelectedValue = preselectedEmployeeId.Value;
                EmployeeComboBox.IsEnabled = false;
            }
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

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                FileName = $"Аттестация_{selectedEmployee.Surename}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Аттестация");

                ws.Cell("A1").Value = "Фамилия";
                ws.Cell("B1").Value = "Имя";
                ws.Cell("C1").Value = "Отчество";
                ws.Cell("D1").Value = "Дата аттестации";
                ws.Cell("E1").Value = "Решение";
                ws.Cell("F1").Value = "Категория";
                ws.Cell("G1").Value = "Номер документа";
                ws.Cell("H1").Value = "Дата документа";
                ws.Cell("I1").Value = "Основание";
                ws.Cell("J1").Value = "Следующая аттестация";

                ws.Cell("A2").Value = selectedEmployee.Surename;
                ws.Cell("B2").Value = selectedEmployee.FirstName;
                ws.Cell("C2").Value = selectedEmployee.SecondName;
                ws.Cell("D2").Value = DateTextBox.Text.Trim();
                ws.Cell("E2").Value = ResolutionTextBox.Text.Trim();
                ws.Cell("F2").Value = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                ws.Cell("G2").Value = DocNumberTextBox.Text.Trim();
                ws.Cell("H2").Value = DocDateTextBox.Text.Trim();
                ws.Cell("I2").Value = BaseTextBox.Text.Trim();
                ws.Cell("J2").Value = NextDateTextBox.Text.Trim();

                ws.Columns().AdjustToContents();

                try
                {
                    workbook.SaveAs(sfd.FileName);
                    MessageBox.Show("Аттестация экспортирована в Excel.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
