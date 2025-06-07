using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HRApp.Data;
using HRApp.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Threading.Tasks;
using HRApp.Helpers;

namespace HRApp.Views
{
    public partial class RemindersPage : Page
    {
        private List<ReminderItem> allReminders = new();

        public RemindersPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadReminders();
        }

        private async void LoadReminders_Click(object sender, RoutedEventArgs e)
        {
            await LoadReminders();
        }

        private void HideProcessedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private async void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadReminders();
        }

        private async Task LoadReminders()
        {
            allReminders.Clear();
            DateTime today = DateTime.Today;
            DateTime endDate = GetPeriodEndDate(today);
            DateTime notifyDate = today.AddDays(1);

            using var context = new HRDbContext();
            var employees = context.Employees.ToList();
            var vacations = context.Vacations.ToList();
            var certifications = context.Certifications.ToList();
            var agreements = context.Agreements.ToList();
            var trips = context.BusinessTrips.ToList();

            if (!employees.Any()) return;

            foreach (var vac in vacations.Where(v => v.EndDate >= today && v.EndDate <= endDate))
            {
                var emp = employees.FirstOrDefault(e => e.Id == vac.EmployeeId);
                if (emp != null)
                {
                    var item = new ReminderItem
                    {
                        Type = "Отпуска",
                        EmployeeName = $"{emp.Surename} {emp.FirstName} {emp.SecondName}",
                        Date = vac.EndDate.ToShortDateString(),
                        Info = $"Окончание отпуска ({vac.VacationType})"
                    };
                    allReminders.Add(item);

                    if (vac.EndDate.Date == notifyDate)
                    {
                        await TelegramHelper.SendMessageAsync(
                            $"<b>Напоминание: отпуск</b>\nСотрудник: {item.EmployeeName}\nДата: {vac.EndDate:dd.MM.yyyy}\n{item.Info}");
                    }
                }
            }

            foreach (var cert in certifications.Where(c => c.NextDate >= today && c.NextDate <= endDate))
            {
                var emp = employees.FirstOrDefault(e => e.Id == cert.EmployeeId);
                if (emp != null)
                {
                    var item = new ReminderItem
                    {
                        Type = "Аттестации",
                        EmployeeName = $"{emp.Surename} {emp.FirstName} {emp.SecondName}",
                        Date = cert.NextDate.ToShortDateString(),
                        Info = "Запланирована повторная аттестация"
                    };
                    allReminders.Add(item);

                    if (cert.NextDate.Date == notifyDate)
                    {
                        await TelegramHelper.SendMessageAsync(
                            $"<b>Напоминание: аттестация</b>\nСотрудник: {item.EmployeeName}\nДата: {cert.NextDate:dd.MM.yyyy}\n{item.Info}");
                    }
                }
            }

            foreach (var agr in agreements.Where(a => a.AgreementEndDate >= today && a.AgreementEndDate <= endDate))
            {
                var emp = employees.FirstOrDefault(e => e.Id == agr.EmployeeId);
                if (emp != null)
                {
                    var item = new ReminderItem
                    {
                        Type = "Договоры",
                        EmployeeName = $"{emp.Surename} {emp.FirstName} {emp.SecondName}",
                        Date = agr.AgreementEndDate.ToShortDateString(),
                        Info = "Окончание трудового договора"
                    };
                    allReminders.Add(item);

                    if (agr.AgreementEndDate.Date == notifyDate)
                    {
                        await TelegramHelper.SendMessageAsync(
                            $"<b>Напоминание: договор</b>\nСотрудник: {item.EmployeeName}\nДата: {agr.AgreementEndDate:dd.MM.yyyy}\n{item.Info}");
                    }
                }
            }

            foreach (var trip in trips.Where(t => t.TripEndDate >= today && t.TripEndDate <= endDate))
            {
                var emp = employees.FirstOrDefault(e => e.Id == trip.EmployeeId);
                if (emp != null)
                {
                    var item = new ReminderItem
                    {
                        Type = "Командировки",
                        EmployeeName = $"{emp.Surename} {emp.FirstName} {emp.SecondName}",
                        Date = trip.TripEndDate.ToShortDateString(),
                        Info = $"Окончание командировки: {trip.Destination}"
                    };
                    allReminders.Add(item);

                    if (trip.TripEndDate.Date == notifyDate)
                    {
                        await TelegramHelper.SendMessageAsync(
                            $"<b>Напоминание: командировка</b>\nСотрудник: {item.EmployeeName}\nДата: {trip.TripEndDate:dd.MM.yyyy}\n{item.Info}");
                    }
                }
            }

            ApplyFilter();
        }

        private DateTime GetPeriodEndDate(DateTime fromDate)
        {
            string period = (PeriodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            return period switch
            {
                "3 дня" => fromDate.AddDays(3),
                "7 дней" => fromDate.AddDays(7),
                "30 дней" => fromDate.AddDays(30),
                "Все даты" => fromDate.AddYears(10),
                _ => fromDate.AddDays(7)
            };
        }

        private void ReminderTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (RemindersDataGrid == null) return;

            string selectedType = (ReminderTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            bool hideProcessed = HideProcessedCheckBox.IsChecked == true;

            var filtered = allReminders
                .Where(r => (selectedType == "Все" || r.Type == selectedType) &&
                            (!hideProcessed || !r.Processed))
                .ToList();

            RemindersDataGrid.ItemsSource = filtered;
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            var filtered = RemindersDataGrid.ItemsSource as List<ReminderItem>;
            if (filtered == null || !filtered.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                FileName = $"Напоминания_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Напоминания");

                ws.Cell(1, 1).Value = "Событие";
                ws.Cell(1, 2).Value = "Сотрудник";
                ws.Cell(1, 3).Value = "Дата";
                ws.Cell(1, 4).Value = "Детали";
                ws.Cell(1, 5).Value = "Обработано";

                for (int i = 0; i < filtered.Count; i++)
                {
                    var r = filtered[i];
                    ws.Cell(i + 2, 1).Value = r.Type;
                    ws.Cell(i + 2, 2).Value = r.EmployeeName;
                    ws.Cell(i + 2, 3).Value = r.Date;
                    ws.Cell(i + 2, 4).Value = r.Info;
                    ws.Cell(i + 2, 5).Value = r.Processed ? "Да" : "Нет";
                }

                ws.Columns().AdjustToContents();
                wb.SaveAs(sfd.FileName);
                MessageBox.Show("Экспорт завершён.");
            }
        }

        private async void SendToTelegram_Click(object sender, RoutedEventArgs e)
        {
            var items = RemindersDataGrid.ItemsSource as List<ReminderItem>;
            if (items == null || items.Count == 0)
            {
                MessageBox.Show("Нет напоминаний для отправки.");
                return;
            }

            int sentCount = 0;

            foreach (var item in items.Where(i => !i.Processed))
            {
                string message = $"<b>Напоминание: {item.Type}</b>\n" +
                                 $"Сотрудник: {item.EmployeeName}\n" +
                                 $"Дата: {item.Date}\n" +
                                 $"{item.Info}";

                bool success = await TelegramHelper.SendMessageAsync(message);
                if (success)
                    sentCount++;
            }

            MessageBox.Show($"Отправлено уведомлений в Telegram: {sentCount}");
        }

        public class ReminderItem
        {
            public string Type { get; set; }
            public string EmployeeName { get; set; }
            public string Date { get; set; }
            public string Info { get; set; }
            public bool Processed { get; set; } = false;
        }
    }
}
