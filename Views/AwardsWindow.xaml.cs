using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using HRApp.Data;
using HRApp.Models;
using Microsoft.Win32;

namespace HRApp.Views
{
    public partial class AwardsWindow : Window
    {
        private List<Employee> employees;
        private int? preselectedEmployeeId;

        public AwardsWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        public AwardsWindow(int employeeId) : this()
        {
            preselectedEmployeeId = employeeId;
        }

        private void LoadEmployees()
        {
            using var context = new HRDbContext();
            employees = context.Employees.ToList();
            EmployeeComboBox.ItemsSource = employees;
            EmployeeComboBox.DisplayMemberPath = "Surename";

            if (preselectedEmployeeId.HasValue)
            {
                var selected = employees.FirstOrDefault(e => e.Id == preselectedEmployeeId.Value);
                if (selected != null)
                    EmployeeComboBox.SelectedItem = selected;
            }
        }

        private void EmployeeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is Employee employee)
                LoadAwardsForEmployee(employee.Id);
            else
                AwardsDataGrid.ItemsSource = null;
        }

        private void LoadAwardsForEmployee(int employeeId)
        {
            using var context = new HRDbContext();
            var awards = context.EmployeeAwards
                .Where(a => a.EmployeeId == employeeId)
                .ToList();

            AwardsDataGrid.ItemsSource = awards;
        }

        private void AddAward_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            if (!DateTextBox.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите дату награждения.");
                return;
            }

            var newAward = new EmployeeAward
            {
                AwardDate = DateTextBox.SelectedDate.Value,
                AwardNumber = NumberTextBox.Text.Trim(),
                Department = (DepartmentComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                AwardType = (AwardTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "",
                EmployeeId = selectedEmployee.Id
            };

            using var context = new HRDbContext();
            context.EmployeeAwards.Add(newAward);
            context.SaveChanges();

            MessageBox.Show("Награда добавлена.");
            ClearForm();
            LoadAwardsForEmployee(selectedEmployee.Id);
        }

        private void DeleteAward_Click(object sender, RoutedEventArgs e)
        {
            if (AwardsDataGrid.SelectedItem is not EmployeeAward selectedAward)
            {
                MessageBox.Show("Выберите награду для удаления.");
                return;
            }

            using var context = new HRDbContext();
            var award = context.EmployeeAwards.FirstOrDefault(a => a.Id == selectedAward.Id);
            if (award != null)
            {
                context.EmployeeAwards.Remove(award);
                context.SaveChanges();
                LoadAwardsForEmployee(award.EmployeeId);
                MessageBox.Show("Награда удалена.");
            }
        }

        private void ClearForm()
        {
            DateTextBox.SelectedDate = null;
            NumberTextBox.Text = "";
            DepartmentComboBox.SelectedIndex = -1;
            AwardTypeComboBox.SelectedIndex = -1;
        }

        private void SaveAwards_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Сохранение происходит автоматически при добавлении.");
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника для экспорта.");
                return;
            }

            using var context = new HRDbContext();
            var awardsList = context.EmployeeAwards
                .Where(a => a.EmployeeId == selectedEmployee.Id)
                .ToList();

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Награды_{selectedEmployee.Surename}_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var workbook = new XLWorkbook();
                    var ws = workbook.Worksheets.Add("Награды");

                    ws.Cell(1, 1).Value = "Дата";
                    ws.Cell(1, 2).Value = "Номер";
                    ws.Cell(1, 3).Value = "Подразделение";
                    ws.Cell(1, 4).Value = "Тип";

                    for (int i = 0; i < awardsList.Count; i++)
                    {
                        var a = awardsList[i];
                        ws.Cell(i + 2, 1).Value = a.AwardDate.ToShortDateString();
                        ws.Cell(i + 2, 2).Value = a.AwardNumber;
                        ws.Cell(i + 2, 3).Value = a.Department;
                        ws.Cell(i + 2, 4).Value = a.AwardType;
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(sfd.FileName);
                    MessageBox.Show("Экспорт завершён.");
                    System.Diagnostics.Process.Start("explorer.exe", sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}");
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
