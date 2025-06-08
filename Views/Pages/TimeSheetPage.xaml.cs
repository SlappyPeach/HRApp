using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using HRApp.Data;
using HRApp.Models;

namespace HRApp.Views
{
    public partial class TimeSheetPage : Page
    {
        private List<TimeSheetEntry> timeSheetData = new();

        public TimeSheetPage()
        {
            InitializeComponent();
        }

        private void GenerateTimeSheet_Click(object sender, RoutedEventArgs e)
        {
            if (MonthComboBox.SelectedItem is not ComboBoxItem selectedMonth ||
                !int.TryParse(selectedMonth.Tag.ToString(), out int month) ||
                !int.TryParse(YearTextBox.Text, out int year))
            {
                MessageBox.Show("Укажите корректные месяц и год.");
                return;
            }

            int daysInMonth = DateTime.DaysInMonth(year, month);
            timeSheetData.Clear();

            using var context = new HRDbContext();
            var employees = context.Employees.ToList();
            var departments = context.Departments.ToList();
            var positions = context.Positions.ToList();
            var vacations = context.Vacations.ToList();
            var trips = context.BusinessTrips.ToList();
            var sickLeaves = context.SickLeaves.ToList();
            var absenceRecords = context.AbsenceRecords
                .Where(r => r.Year == year && r.Month == month)
                .ToList();

            // Список праздничных дней (можно дополнить)
            var holidays = new List<DateTime>
            {
                new DateTime(year, 1, 1),
                new DateTime(year, 1, 7),
                new DateTime(year, 2, 23),
                new DateTime(year, 3, 8),
                new DateTime(year, 5, 1),
                new DateTime(year, 5, 9),
                new DateTime(year, 6, 12),
                new DateTime(year, 11, 4)
            };

            foreach (var emp in employees)
            {
                var department = departments.FirstOrDefault(d => d.Id == emp.DepartmentId)?.Name ?? "-";
                var position = positions.FirstOrDefault(p => p.Id == emp.PositionId)?.Title ?? "-";

                var entry = new TimeSheetEntry
                {
                    EmployeeName = $"{emp.Surename} {emp.FirstName} {emp.SecondName}".Trim(),
                    Position = position,
                    Department = department,
                    Days = new string[daysInMonth]
                };

                for (int i = 0; i < daysInMonth; i++)
                {
                    var date = new DateTime(year, month, i + 1);

                    // Приоритет: Больничный → Отпуск → Командировка → Выходной
                    if (sickLeaves.Any(b => b.EmployeeId == emp.Id && date >= b.FromDate && date <= b.ToDate))
                    {
                        entry.Days[i] = "Б";
                    }
                    else if (vacations.Any(v => v.EmployeeId == emp.Id && date >= v.StartDate && date <= v.EndDate))
                    {
                        entry.Days[i] = "О";
                    }
                    else if (trips.Any(t => t.EmployeeId == emp.Id && date >= t.TripStartDate && date <= t.TripEndDate))
                    {
                        entry.Days[i] = "К";
                    }
                    else if (date.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(date))
                    {
                        entry.Days[i] = "Н";
                    }
                    else
                    {
                        entry.Days[i] = ""; // Явка, неявка — вручную
                    }
                }

                timeSheetData.Add(entry);
            }

            // Строка ИТОГО
            var total = new TimeSheetEntry
            {
                EmployeeName = "ИТОГО:",
                Position = "",
                Department = "",
                Days = new string[daysInMonth]
            };

            timeSheetData.Add(total);

            // Отрисовка таблицы
            TimeSheetDataGrid.ItemsSource = null;
            TimeSheetDataGrid.Columns.Clear();

            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Сотрудник",
                Binding = new System.Windows.Data.Binding("EmployeeName"),
                IsReadOnly = true
            });

            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Должность",
                Binding = new System.Windows.Data.Binding("Position"),
                IsReadOnly = true
            });

            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Подразделение",
                Binding = new System.Windows.Data.Binding("Department"),
                IsReadOnly = true
            });

            for (int i = 0; i < daysInMonth; i++)
            {
                int dayNum = i;
                TimeSheetDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = (i + 1).ToString(),
                    Binding = new System.Windows.Data.Binding($"Days[{dayNum}]"),
                    IsReadOnly = false
                });
            }

            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Я", Binding = new System.Windows.Data.Binding("TotalY"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "О", Binding = new System.Windows.Data.Binding("TotalO"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Б", Binding = new System.Windows.Data.Binding("TotalB"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "К", Binding = new System.Windows.Data.Binding("TotalK"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Н", Binding = new System.Windows.Data.Binding("TotalN"), IsReadOnly = true });

            TimeSheetDataGrid.ItemsSource = timeSheetData;
            TimeSheetDataGrid.CellEditEnding += TimeSheetDataGrid_CellEditEnding;

            RecalculateTotals();
            SummaryTextBlock.Text = $"Табель учёта отсутствий за {selectedMonth.Content} {year}. Сотрудников: {timeSheetData.Count - 1}";
        }


        private HashSet<DateTime> GetRussianPublicHolidays(int year, int month)
        {
            return new HashSet<DateTime>
            {
                new DateTime(year, 1, 1),  // Новый год
                new DateTime(year, 1, 7),  // Рождество
                new DateTime(year, 2, 23), // День защитника Отечества
                new DateTime(year, 3, 8),  // 8 Марта
                new DateTime(year, 5, 1),  // Праздник весны и труда
                new DateTime(year, 5, 9),  // День Победы
                new DateTime(year, 6, 12), // День России
                new DateTime(year, 11, 4), // День народного единства
            }.Where(d => d.Month == month).ToHashSet();
        }


        private void TimeSheetDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;

            var editedEntry = e.Row.Item as TimeSheetEntry;
            if (editedEntry == null || editedEntry.EmployeeName == "ИТОГО:")
                return;

            int columnIndex = e.Column.DisplayIndex;
            int dayIndex = columnIndex - 3;

            if (dayIndex < 0 || dayIndex >= editedEntry.Days.Length)
                return;

            if (e.EditingElement is TextBox tb)
            {
                string input = tb.Text.Trim().ToUpper();
                string[] allowed = { "Я", "О", "Б", "К", "Н", "" };

                if (!allowed.Contains(input))
                {
                    MessageBox.Show("Допустимые значения: Я, О, Б, К, Н");
                    tb.Text = "";
                    return;
                }

                editedEntry.Days[dayIndex] = input;
            }

            RecalculateTotals();
        }

        private void RecalculateTotals()
        {
            if (timeSheetData.Count == 0)
                return;

            int days = timeSheetData[0].Days.Length;
            var totalRow = timeSheetData.Last();
            if (totalRow.EmployeeName != "ИТОГО:")
                return;

            for (int d = 0; d < days; d++)
            {
                int countY = timeSheetData.Take(timeSheetData.Count - 1).Count(e => e.Days[d] == "Я");
                totalRow.Days[d] = countY.ToString();
            }

            foreach (var row in timeSheetData)
            {
                row.ForceRecalculate();
            }

            Dispatcher.InvokeAsync(() =>
            {
                TimeSheetDataGrid.Items.Refresh();
            });
        }

        private void SaveTimeSheet_Click(object sender, RoutedEventArgs e)
        {
            if (MonthComboBox.SelectedItem is not ComboBoxItem selectedMonth ||
                !int.TryParse(selectedMonth.Tag.ToString(), out int month) ||
                !int.TryParse(YearTextBox.Text, out int year))
            {
                MessageBox.Show("Укажите корректные месяц и год.");
                return;
            }

            using var context = new HRDbContext();

            var existing = context.AbsenceRecords
                .Where(r => r.Year == year && r.Month == month)
                .ToList();

            context.AbsenceRecords.RemoveRange(existing);
            context.SaveChanges();

            var employees = context.Employees.ToList();

            foreach (var entry in timeSheetData.Where(e => e.EmployeeName != "ИТОГО:"))
            {
                var parts = entry.EmployeeName.Split(' ');
                if (parts.Length < 2) continue;

                var emp = employees.FirstOrDefault(e =>
                    e.Surename == parts[0] && e.FirstName == parts[1]);

                if (emp == null) continue;

                for (int i = 0; i < entry.Days.Length; i++)
                {
                    string status = entry.Days[i];
                    if (!new[] { "Я", "О", "Б", "К", "Н" }.Contains(status)) continue;

                    context.AbsenceRecords.Add(new AbsenceRecord
                    {
                        EmployeeId = emp.Id,
                        Year = year,
                        Month = month,
                        Day = i + 1,
                        Status = status
                    });
                }
            }

            context.SaveChanges();
            MessageBox.Show("Табель успешно сохранён.");
        }

        private void ExportDOCX_Click(object sender, RoutedEventArgs e)
        {
            if (timeSheetData == null || timeSheetData.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                FileName = $"Табель_отсутствий_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Табель");

                    int days = timeSheetData[0].Days.Length;
                    int baseCol = 4;

                    ws.Cell(1, 1).Value = "Сотрудник";
                    ws.Cell(1, 2).Value = "Должность";
                    ws.Cell(1, 3).Value = "Подразделение";

                    for (int d = 0; d < days; d++)
                        ws.Cell(1, baseCol + d).Value = (d + 1).ToString();

                    int offset = baseCol + days;
                    ws.Cell(1, offset).Value = "Я";
                    ws.Cell(1, offset + 1).Value = "О";
                    ws.Cell(1, offset + 2).Value = "Б";
                    ws.Cell(1, offset + 3).Value = "К";
                    ws.Cell(1, offset + 4).Value = "Н";

                    for (int i = 0; i < timeSheetData.Count; i++)
                    {
                        var row = timeSheetData[i];
                        ws.Cell(i + 2, 1).Value = row.EmployeeName;
                        ws.Cell(i + 2, 2).Value = row.Position;
                        ws.Cell(i + 2, 3).Value = row.Department;

                        for (int d = 0; d < days; d++)
                            ws.Cell(i + 2, baseCol + d).Value = row.Days[d];

                        ws.Cell(i + 2, offset).Value = row.TotalY;
                        ws.Cell(i + 2, offset + 1).Value = row.TotalO;
                        ws.Cell(i + 2, offset + 2).Value = row.TotalB;
                        ws.Cell(i + 2, offset + 3).Value = row.TotalK;
                        ws.Cell(i + 2, offset + 4).Value = row.TotalN;
                    }

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(sfd.FileName);
                    MessageBox.Show("Экспорт завершён.");
                    System.Diagnostics.Process.Start("explorer.exe", sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}");
                }
            }
        }

        public class TimeSheetEntry : INotifyPropertyChanged
        {
            public string EmployeeName { get; set; }
            public string Position { get; set; }
            public string Department { get; set; }
            public string[] Days { get; set; }

            public int TotalY => Days.Count(d => d == "Я");
            public int TotalO => Days.Count(d => d == "О");
            public int TotalB => Days.Count(d => d == "Б");
            public int TotalK => Days.Count(d => d == "К");
            public int TotalN => Days.Count(d => d == "Н");

            public void ForceRecalculate()
            {
                OnPropertyChanged(nameof(TotalY));
                OnPropertyChanged(nameof(TotalO));
                OnPropertyChanged(nameof(TotalB));
                OnPropertyChanged(nameof(TotalK));
                OnPropertyChanged(nameof(TotalN));
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
