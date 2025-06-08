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
using ClosedXML.Excel;



namespace HRApp.Views
{
    public partial class SickLeaveWindow : Window
    {
        private List<Employee> employees;

        public SickLeaveWindow()
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

            if (!DateTime.TryParse(RegDateTextBox.Text, out var regDate) ||
                !DateTime.TryParse(FromDateTextBox.Text, out var fromDate) ||
                !DateTime.TryParse(ToDateTextBox.Text, out var toDate))
            {
                MessageBox.Show("Неверный формат дат.");
                return;
            }

            if (toDate < fromDate)
            {
                MessageBox.Show("Дата окончания раньше начала.");
                return;
            }

            using var context = new HRDbContext();

            // Генерируем номер больничного
            string regNumber = $"БЛН-{DateTime.Now.Year}-{(context.SickLeaves.Count() + 1):D4}";

            // Сохраняем больничный
            var sickLeave = new SickLeave
            {
                RegNumber = regNumber,
                RegDate = regDate,
                FromDate = fromDate,
                ToDate = toDate,
                Info = InfoTextBox.Text.Trim(),
                WorkFrom = "",
                EmployeeId = selectedEmployee.Id
            };
            context.SickLeaves.Add(sickLeave);

            // Регистрируем больничный в журнале
            context.RegisterDocuments.Add(new RegisterDocument
            {
                RegNumber = regNumber,
                RegDate = regDate,
                DocType = "Больничный"
            });

            context.SaveChanges();
            MessageBox.Show($"Больничный зарегистрирован. № {regNumber}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
                FileName = $"Больничный_{selectedEmployee.Surename}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Больничный");

                worksheet.Cell("A1").Value = "Фамилия";
                worksheet.Cell("B1").Value = "Имя";
                worksheet.Cell("C1").Value = "Отчество";
                worksheet.Cell("D1").Value = "Номер больничного";
                worksheet.Cell("E1").Value = "Дата регистрации";
                worksheet.Cell("F1").Value = "С";
                worksheet.Cell("G1").Value = "По";                
                worksheet.Cell("J1").Value = "Доп. информация";

                worksheet.Cell("A2").Value = selectedEmployee.Surename;
                worksheet.Cell("B2").Value = selectedEmployee.FirstName;
                worksheet.Cell("C2").Value = selectedEmployee.SecondName;
                worksheet.Cell("D2").Value = RegNumberTextBox.Text.Trim();
                worksheet.Cell("E2").Value = RegDateTextBox.Text.Trim();
                worksheet.Cell("F2").Value = FromDateTextBox.Text.Trim();
                worksheet.Cell("G2").Value = ToDateTextBox.Text.Trim();
                worksheet.Cell("J2").Value = InfoTextBox.Text.Trim();

                worksheet.Columns().AdjustToContents();

                try
                {
                    workbook.SaveAs(sfd.FileName);
                    MessageBox.Show("Больничный экспортирован в Excel.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
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
