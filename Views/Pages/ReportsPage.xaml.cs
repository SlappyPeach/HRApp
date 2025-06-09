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
    public partial class ReportsPage : Page
    {
        private List<dynamic> reportData = new();

        public ReportsPage()
        {
            InitializeComponent();
            ReportTypeComboBox.SelectionChanged += ReportTypeComboBox_SelectionChanged;
        }

        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selected == "Гражданство")
            {
                using var context = new HRDbContext();
                var allCitizenships = context.Employees
                    .Select(e => e.Citizenship ?? "Не указано")
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                allCitizenships.Insert(0, "Все");

                CitizenshipFilterComboBox.ItemsSource = allCitizenships;
                CitizenshipFilterComboBox.SelectedIndex = 0;
                CitizenshipFilterComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                CitizenshipFilterComboBox.Visibility = Visibility.Collapsed;
            }
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            string selectedReport = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            reportData.Clear();
            ReportDataGrid.ItemsSource = null;

            using var context = new HRDbContext();
            var employees = context.Employees.ToList();

            if (selectedReport == "Приказы за период")
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату начала и окончания.");
                    return;
                }

                var start = StartDatePicker.SelectedDate.Value;
                var end = EndDatePicker.SelectedDate.Value;
                var orders = context.Orders.Where(o => o.DocDate >= start && o.DocDate <= end).ToList();

                var report = orders.Select(o =>
                {
                    var emp = employees.FirstOrDefault(e => e.Id == o.EmployeeId);
                    return new
                    {
                        Номер = o.RegNumber,
                        ДатаДокумента = o.DocDate.ToShortDateString(),
                        ДатаНачала = o.StartDate.ToShortDateString(),
                        Сотрудник = emp != null ? $"{emp.Surename} {emp.FirstName} {emp.SecondName}" : "—",
                        Основание = o.Base,
                        Содержание = o.Content
                    };
                }).ToList<dynamic>();

                reportData = report;
                ReportDataGrid.ItemsSource = report;
                SummaryTextBlock.Text = $"Всего приказов: {report.Count}";
            }
            else if (selectedReport == "Список сотрудников")
            {
                var report = employees.Select(e => new
                {
                    ФИО = $"{e.Surename} {e.FirstName} {e.SecondName}",
                    ДатаРождения = e.BirthDate.ToShortDateString(),
                    Гражданство = e.Citizenship,
                    Телефон = e.TelNumber,
                    Адрес = e.Address1
                }).ToList<dynamic>();

                reportData = report;
                ReportDataGrid.ItemsSource = report;
                SummaryTextBlock.Text = $"Всего сотрудников: {report.Count}";
            }
            else if (selectedReport == "Больничные за период")
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату начала и окончания.");
                    return;
                }

                var start = StartDatePicker.SelectedDate.Value;
                var end = EndDatePicker.SelectedDate.Value;
                var sickLeaves = context.SickLeaves.Where(s => s.FromDate >= start && s.ToDate <= end).ToList();

                var report = sickLeaves.Select(s =>
                {
                    var emp = employees.FirstOrDefault(e => e.Id == s.EmployeeId);
                    return new
                    {
                        Номер = s.RegNumber,
                        Сотрудник = emp != null ? $"{emp.Surename} {emp.FirstName} {emp.SecondName}" : "—",
                        С = s.FromDate.ToShortDateString(),
                        По = s.ToDate.ToShortDateString(),
                        МедицинскоеУчреждение = s.MedStateName,
                        Лицензия = s.LicenseNumber
                    };
                }).ToList<dynamic>();

                reportData = report;
                ReportDataGrid.ItemsSource = report;
                SummaryTextBlock.Text = $"Всего больничных: {report.Count}";
            }
            else if (selectedReport == "Отпуска за период")
            {
                if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату начала и окончания.");
                    return;
                }

                var start = StartDatePicker.SelectedDate.Value;
                var end = EndDatePicker.SelectedDate.Value;
                var vacations = context.Vacations.Where(v => v.StartDate >= start && v.EndDate <= end).ToList();

                var report = vacations.Select(v =>
                {
                    var emp = employees.FirstOrDefault(e => e.Id == v.EmployeeId);
                    return new
                    {
                        Сотрудник = emp != null ? $"{emp.Surename} {emp.FirstName} {emp.SecondName}" : "—",
                        ВидОтпуска = v.VacationType,
                        С = v.StartDate.ToShortDateString(),
                        По = v.EndDate.ToShortDateString(),
                        Дней = v.CalendarDaysNumber,
                        Основание = v.Base
                    };
                }).ToList<dynamic>();

                reportData = report;
                ReportDataGrid.ItemsSource = report;
                SummaryTextBlock.Text = $"Всего отпусков: {report.Count}";
            }
            else if (selectedReport == "Гражданство")
            {
                var grouped = employees
                    .GroupBy(e => e.Citizenship ?? "Не указано")
                    .Select(g => new
                    {
                        Гражданство = g.Key,
                        Количество = g.Count(),
                        Сотрудники = string.Join("; ", g.Select(e => $"{e.Surename} {e.FirstName}"))
                    });

                string filter = CitizenshipFilterComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(filter) && filter != "Все")
                {
                    grouped = grouped.Where(g => g.Гражданство == filter);
                }

                reportData = grouped.ToList<dynamic>();
                ReportDataGrid.ItemsSource = reportData;
                SummaryTextBlock.Text = $"Всего записей: {reportData.Count}";
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (reportData == null || reportData.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Отчёт_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Отчёт");

                    var props = reportData.First().GetType().GetProperties();
                    for (int i = 0; i < props.Length; i++)
                        ws.Cell(1, i + 1).Value = props[i].Name;

                    for (int i = 0; i < reportData.Count; i++)
                    {
                        for (int j = 0; j < props.Length; j++)
                        {
                            var value = props[j].GetValue(reportData[i]);
                            if (value == null)
                                ws.Cell(i + 2, j + 1).SetValue(string.Empty);
                            else
                                ws.Cell(i + 2, j + 1).Value = value;
                        }
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
    }
}
