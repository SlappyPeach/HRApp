using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using System.Windows.Threading;
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

            int currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(currentYear - 5, 11);
            YearComboBox.ItemsSource = years;
            YearComboBox.Text = currentYear.ToString();
        }

        private void GenerateTimeSheet_Click(object sender, RoutedEventArgs e)
        {
            if (MonthComboBox.SelectedItem is not ComboBoxItem selectedMonth ||
                !int.TryParse(selectedMonth.Tag.ToString(), out int month) ||
                !int.TryParse(YearComboBox.Text, out int year))
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
            var salaryRates = context.SalaryRates.ToList();
            var vacations = context.Vacations.ToList();
            var trips = context.BusinessTrips.ToList();
            var sickLeaves = context.SickLeaves.ToList();
            var agreements = context.Agreements.ToList();
            var absenceRecords = context.AbsenceRecords
                .Where(r => r.Year == year && r.Month == month)
                .ToList();

            // Список праздничных дней на выбранный месяц
            var holidays = GetRussianPublicHolidays(year, month);

            foreach (var emp in employees)
            {
                var department = departments.FirstOrDefault(d => d.Id == emp.DepartmentId)?.Name ?? "-";
                var position = positions.FirstOrDefault(p => p.Id == emp.PositionId)?.Title ?? "-";

                var agreement = agreements
                    .Where(a => a.EmployeeId == emp.Id)
                    .OrderByDescending(a => a.AgreementDate)
                    .FirstOrDefault();
                decimal hourRate = agreement != null
                    ? agreement.Salary / 160m
                    : (salaryRates.FirstOrDefault(s => s.PositionId == emp.PositionId)?.Amount ?? 0m) / 160m;

                var entry = new TimeSheetEntry
                {
                    EmployeeName = $"{emp.Surename} {emp.FirstName} {emp.SecondName}".Trim(),
                    Position = position,
                    Department = department,
                    Days = new string[daysInMonth],
                    DailyHours = new decimal[daysInMonth],
                    HourRate = hourRate
                };

                for (int i = 0; i < daysInMonth; i++)
                {
                    var date = new DateTime(year, month, i + 1);

                    var rec = absenceRecords.FirstOrDefault(r => r.EmployeeId == emp.Id && r.Day == i + 1);
                    if (rec != null)
                    {
                        entry.Days[i] = rec.Status;
                        entry.DailyHours[i] = rec.Hours;
                        continue;
                    }

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
                        entry.Days[i] = "В";
                    }
                    else
                    {
                        entry.Days[i] = ""; // Явка, неявка — вручную
                    }

                    entry.DailyHours[i] = 0m;
                }

                timeSheetData.Add(entry);
            }

            // Строка ИТОГО
            var total = new TimeSheetEntry
            {
                EmployeeName = "ИТОГО:",
                Position = "",
                Department = "",
                Days = new string[daysInMonth],
                DailyHours = new decimal[daysInMonth]
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
                TimeSheetDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = (i + 1).ToString() + " ч",
                    Binding = new System.Windows.Data.Binding($"DailyHours[{dayNum}]"),
                    IsReadOnly = false
                });
            }

            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Я", Binding = new System.Windows.Data.Binding("TotalY"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "О", Binding = new System.Windows.Data.Binding("TotalO"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Б", Binding = new System.Windows.Data.Binding("TotalB"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "К", Binding = new System.Windows.Data.Binding("TotalK"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Н", Binding = new System.Windows.Data.Binding("TotalN"), IsReadOnly = true });

            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Ставка", Binding = new System.Windows.Data.Binding("HourRate"), IsReadOnly = false });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Часы 1-15", Binding = new System.Windows.Data.Binding("HoursFirstHalf"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Часы 16-", Binding = new System.Windows.Data.Binding("HoursSecondHalf"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Всего часов", Binding = new System.Windows.Data.Binding("TotalHours"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Неявки, дни", Binding = new System.Windows.Data.Binding("AbsenceDays"), IsReadOnly = true });
            TimeSheetDataGrid.Columns.Add(new DataGridTextColumn { Header = "Неявки, ч", Binding = new System.Windows.Data.Binding("AbsenceHours"), IsReadOnly = true });

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
            int pairIndex = columnIndex - 3;
            int dayIndex = pairIndex / 2;

            if (pairIndex >= 0 && dayIndex < editedEntry.Days.Length)
            {
                bool hoursColumn = pairIndex % 2 == 1;
                if (hoursColumn)
                {
                    if (e.EditingElement is TextBox tb && decimal.TryParse(tb.Text, out var hrs))
                        editedEntry.DailyHours[dayIndex] = hrs;
                }
                else if (e.EditingElement is TextBox tb)
                {
                    string input = tb.Text.Trim().ToUpper();
                    string[] allowed = { "Я", "О", "Б", "К", "Н", "В", "" };

                    if (!allowed.Contains(input))
                    {
                        MessageBox.Show("Допустимые значения: Я, О, Б, К, Н, В");
                        tb.Text = "";
                        return;
                    }

                    editedEntry.Days[dayIndex] = input;
                }
            }
            else if (e.Column.Header?.ToString() == "Ставка")
            {
                if (e.EditingElement is TextBox tb && decimal.TryParse(tb.Text, out var rate))
                    editedEntry.HourRate = rate;
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
                totalRow.DailyHours[d] = timeSheetData.Take(timeSheetData.Count - 1).Sum(e => e.DailyHours[d]);
            }

            totalRow.SummaryAbsenceDays = timeSheetData
                .Take(timeSheetData.Count - 1)
                .Sum(e => e.Days.Count(d => d != "Я" && d != "В"));

            totalRow.SummaryAbsenceHours = timeSheetData
                .Take(timeSheetData.Count - 1)
                .Sum(e => Enumerable.Range(0, e.DailyHours.Length)
                                   .Where(i => e.Days[i] != "Я")
                                   .Sum(i => e.DailyHours[i]));

            foreach (var row in timeSheetData)
            {
                row.ForceRecalculate();
            }

            Dispatcher.InvokeAsync(() =>
            {
                TimeSheetDataGrid.Items.Refresh();
            }, DispatcherPriority.Background);
        }

        private void SaveTimeSheet_Click(object sender, RoutedEventArgs e)
        {
            if (MonthComboBox.SelectedItem is not ComboBoxItem selectedMonth ||
                !int.TryParse(selectedMonth.Tag.ToString(), out int month) ||
                !int.TryParse(YearComboBox.Text, out int year))
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

            string tsCode = $"{year}-{month:00}";
            var existingSheets = context.TimeSheets.Where(t => t.TimeSheetCode == tsCode).ToList();
            context.TimeSheets.RemoveRange(existingSheets);
            context.SaveChanges();

            var employees = context.Employees.ToList();

            foreach (var entry in timeSheetData.Where(e => e.EmployeeName != "ИТОГО:"))
            {
                var parts = entry.EmployeeName.Split(' ');
                if (parts.Length < 2) continue;

                var emp = employees.FirstOrDefault(e =>
                    e.Surename == parts[0] && e.FirstName == parts[1]);

                if (emp == null) continue;

                context.TimeSheets.Add(new TimeSheet
                {
                    TimeSheetCode = tsCode,
                    DaysPerWeek = 5,
                    HourPrice = entry.HourRate,
                    HoursWeek1 = (int)entry.HoursFirstHalf,
                    HoursWeek2 = (int)entry.HoursSecondHalf,
                    EmployeeId = emp.Id
                });

                for (int i = 0; i < entry.Days.Length; i++)
                {
                    string status = entry.Days[i];
                    if (!new[] { "Я", "О", "Б", "К", "Н", "В" }.Contains(status)) continue;

                    context.AbsenceRecords.Add(new AbsenceRecord
                    {
                        EmployeeId = emp.Id,
                        Year = year,
                        Month = month,
                        Day = i + 1,
                        Status = status,
                        Hours = entry.DailyHours[i]
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
                    {
                        ws.Cell(1, baseCol + d * 2).Value = (d + 1).ToString();
                        ws.Cell(1, baseCol + d * 2 + 1).Value = (d + 1) + " ч";
                    }

                    int offset = baseCol + days * 2;
                    ws.Cell(1, offset).Value = "Я";
                    ws.Cell(1, offset + 1).Value = "О";
                    ws.Cell(1, offset + 2).Value = "Б";
                    ws.Cell(1, offset + 3).Value = "К";
                    ws.Cell(1, offset + 4).Value = "Н";
                    ws.Cell(1, offset + 5).Value = "Ставка";
                    ws.Cell(1, offset + 6).Value = "Часы 1-15";
                    ws.Cell(1, offset + 7).Value = "Часы 16-";
                    ws.Cell(1, offset + 8).Value = "Всего часов";
                    ws.Cell(1, offset + 9).Value = "Неявки, дни";
                    ws.Cell(1, offset + 10).Value = "Неявки, ч";

                    for (int i = 0; i < timeSheetData.Count; i++)
                    {
                        var row = timeSheetData[i];
                        ws.Cell(i + 2, 1).Value = row.EmployeeName;
                        ws.Cell(i + 2, 2).Value = row.Position;
                        ws.Cell(i + 2, 3).Value = row.Department;

                        for (int d = 0; d < days; d++)
                        {
                            ws.Cell(i + 2, baseCol + d * 2).Value = row.Days[d];
                            ws.Cell(i + 2, baseCol + d * 2 + 1).Value = row.DailyHours[d];
                        }

                        ws.Cell(i + 2, offset).Value = row.TotalY;
                        ws.Cell(i + 2, offset + 1).Value = row.TotalO;
                        ws.Cell(i + 2, offset + 2).Value = row.TotalB;
                        ws.Cell(i + 2, offset + 3).Value = row.TotalK;
                        ws.Cell(i + 2, offset + 4).Value = row.TotalN;
                        ws.Cell(i + 2, offset + 5).Value = row.HourRate;
                        ws.Cell(i + 2, offset + 6).Value = row.HoursFirstHalf;
                        ws.Cell(i + 2, offset + 7).Value = row.HoursSecondHalf;
                        ws.Cell(i + 2, offset + 8).Value = row.TotalHours;
                        ws.Cell(i + 2, offset + 9).Value = row.AbsenceDays;
                        ws.Cell(i + 2, offset + 10).Value = row.AbsenceHours;
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
            public decimal[] DailyHours { get; set; }

            internal int SummaryAbsenceDays { get; set; }
            internal decimal SummaryAbsenceHours { get; set; }

            private decimal hourRate;
            public decimal HourRate
            {
                get => hourRate;
                set
                {
                    hourRate = value;
                    OnPropertyChanged(nameof(HourRate));
                    ForceRecalculate();
                }
            }

            public int TotalY => Days.Count(d => d == "Я");
            public int TotalO => Days.Count(d => d == "О");
            public int TotalB => Days.Count(d => d == "Б");
            public int TotalK => Days.Count(d => d == "К");
            public int TotalN => Days.Count(d => d == "Н");

            public decimal HoursFirstHalf
            {
                get
                {
                    if (EmployeeName == "ИТОГО:")
                        return DailyHours.Take(Math.Min(15, DailyHours.Length)).Sum();

                    return Enumerable.Range(0, Math.Min(15, DailyHours.Length))
                        .Where(i => Days[i] == "Я")
                        .Sum(i => DailyHours[i]);
                }
            }

            public decimal HoursSecondHalf
            {
                get
                {
                    if (EmployeeName == "ИТОГО:")
                        return DailyHours.Skip(Math.Min(15, DailyHours.Length)).Sum();

                    return Enumerable.Range(Math.Min(15, DailyHours.Length), DailyHours.Length - Math.Min(15, DailyHours.Length))
                        .Where(i => Days[i] == "Я")
                        .Sum(i => DailyHours[i]);
                }
            }

            public decimal TotalHours => HoursFirstHalf + HoursSecondHalf;

            public int AbsenceDays
            {
                get
                {
                    if (EmployeeName == "ИТОГО:")
                        return SummaryAbsenceDays;

                    return Days.Count(d => d != "Я" && d != "В");
                }
            }

            public decimal AbsenceHours
            {
                get
                {
                    if (EmployeeName == "ИТОГО:")
                        return SummaryAbsenceHours;

                    return Enumerable.Range(0, DailyHours.Length)
                        .Where(i => Days[i] != "Я")
                        .Sum(i => DailyHours[i]);
                }
            }

            public void ForceRecalculate()
            {
                OnPropertyChanged(nameof(TotalY));
                OnPropertyChanged(nameof(TotalO));
                OnPropertyChanged(nameof(TotalB));
                OnPropertyChanged(nameof(TotalK));
                OnPropertyChanged(nameof(TotalN));
                OnPropertyChanged(nameof(HoursFirstHalf));
                OnPropertyChanged(nameof(HoursSecondHalf));
                OnPropertyChanged(nameof(TotalHours));
                OnPropertyChanged(nameof(AbsenceDays));
                OnPropertyChanged(nameof(AbsenceHours));
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
