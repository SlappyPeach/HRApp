using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using HRApp.Data;
using HRApp.Models;
using Microsoft.Win32;
using Xceed.Words.NET;

namespace HRApp.Views
{
    public partial class EmploymentHistoryWindow : Window
    {
        private List<Employee> employees;
        private ObservableCollection<EmploymentHistory> historyList = new();

        public EmploymentHistoryWindow()
        {
            InitializeComponent();
            LoadEmployees();
            HistoryDataGrid.ItemsSource = historyList;
        }

        private void LoadEmployees()
        {
            using var context = new HRDbContext();
            employees = context.Employees.ToList();
            EmployeeComboBox.ItemsSource = employees;
            EmployeeComboBox.DisplayMemberPath = "Surename";
        }

        private void EmployeeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is Employee selectedEmployee)
            {
                LoadHistory(selectedEmployee.Id);
            }
        }

        private void LoadHistory(int employeeId)
        {
            using var context = new HRDbContext();
            var entries = context.EmploymentHistories.Where(h => h.EmployeeId == employeeId).ToList();
            historyList.Clear();
            foreach (var item in entries)
                historyList.Add(item);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            historyList.Add(new EmploymentHistory
            {
                StartDate = DateTime.Today.AddYears(-1),
                EndDate = DateTime.Today,
                WorkPlaceName = "Организация",
                Speciality = "Должность",
                EmployeeId = selectedEmployee.Id
            });
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryDataGrid.SelectedItem is EmploymentHistory selected)
            {
                historyList.Remove(selected);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            using var context = new HRDbContext();
            var existing = context.EmploymentHistories.Where(h => h.EmployeeId == selectedEmployee.Id).ToList();
            context.EmploymentHistories.RemoveRange(existing);
            foreach (var entry in historyList)
            {
                entry.EmployeeId = selectedEmployee.Id;
                context.EmploymentHistories.Add(entry);
            }
            context.SaveChanges();
            MessageBox.Show("Записи сохранены.");
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
                Filter = "Excel файл (*.xlsx)|*.xlsx",
                FileName = $"Трудовая_{selectedEmployee.Surename}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var workbook = new ClosedXML.Excel.XLWorkbook();
                    var ws = workbook.Worksheets.Add("Трудовая книжка");

                    // Заголовки
                    ws.Cell(1, 1).Value = "Начало";
                    ws.Cell(1, 2).Value = "Окончание";
                    ws.Cell(1, 3).Value = "Организация";
                    ws.Cell(1, 4).Value = "Специальность";

                    // Данные
                    for (int i = 0; i < historyList.Count; i++)
                    {
                        var entry = historyList[i];
                        ws.Cell(i + 2, 1).Value = entry.StartDate.ToShortDateString();
                        ws.Cell(i + 2, 2).Value = entry.EndDate?.ToShortDateString() ?? "—";
                        ws.Cell(i + 2, 3).Value = entry.WorkPlaceName;
                        ws.Cell(i + 2, 4).Value = entry.Speciality;
                    }

                    ws.Columns().AdjustToContents();
                    workbook.SaveAs(sfd.FileName);
                    MessageBox.Show("Экспорт в Excel завершён.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}");
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
