using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HRApp.Data;
using HRApp.Models;
using Microsoft.Win32;
using Xceed.Words.NET;
using ClosedXML.Excel;

namespace HRApp.Views
{
    public partial class OrdersPage : Page
    {
        public ObservableCollection<OrderDisplay> OrdersList { get; set; } = new();

        private static readonly string[] Months = new[]
        {
            "января", "февраля", "марта", "апреля", "мая", "июня",
            "июля", "августа", "сентября", "октября", "ноября", "декабря"
        };

        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
            OrdersDataGrid.ItemsSource = OrdersList;
        }

        private void LoadOrders()
        {
            OrdersList.Clear();
            using var context = new HRDbContext();
            var orders = context.Orders.ToList();
            var employees = context.Employees.ToList();

            foreach (var order in orders)
            {
                var emp = employees.FirstOrDefault(e => e.Id == order.EmployeeId);
                OrdersList.Add(new OrderDisplay
                {
                    Id = order.Id,
                    StartDate = order.StartDate,
                    RegNumber = order.RegNumber,
                    DocDate = order.DocDate,
                    Base = order.Base,
                    Content = order.Content,
                    EmployeeName = emp != null ? $"{emp.Surename} {emp.FirstName} {emp.SecondName}" : "Неизвестно",
                    Sex = emp?.Sex ?? ""
                });
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchEmployeeTextBox.Text.Trim().ToLower();
            string selectedSex = (SexFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            string selectedOrder = (OrderTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            using var context = new HRDbContext();
            var employees = context.Employees.ToList();
            var orders = context.Orders.ToList();

            var filtered = from o in orders
                           join emp in employees on o.EmployeeId equals emp.Id
                           where emp.Surename.ToLower().Contains(query)
                           select new OrderDisplay
                           {
                               Id = o.Id,
                               StartDate = o.StartDate,
                               RegNumber = o.RegNumber,
                               DocDate = o.DocDate,
                               Base = o.Base,
                               Content = o.Content,
                               EmployeeName = $"{emp.Surename} {emp.FirstName} {emp.SecondName}",
                               Sex = emp.Sex
                           };

            if (selectedSex != "Все" && !string.IsNullOrWhiteSpace(selectedSex))
                filtered = filtered.Where(o => o.Sex == selectedSex);

            if (selectedOrder != "Все" && !string.IsNullOrWhiteSpace(selectedOrder))
                filtered = filtered.Where(o => o.Content == selectedOrder);

            OrdersList.Clear();
            foreach (var order in filtered)
                OrdersList.Add(order);
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchEmployeeTextBox.Text = "";
            SexFilterComboBox.SelectedIndex = 0;
            OrderTypeComboBox.SelectedIndex = 0;
            LoadOrders();
        }

        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is OrderDisplay order)
            {
                OrderNumberTextBlock.Text = $"Номер: {order.RegNumber}";
                OrderDateTextBlock.Text = $"Дата документа: {order.DocDate.ToShortDateString()}";
                OrderStartTextBlock.Text = $"Дата начала: {order.StartDate.ToShortDateString()}";
                OrderBaseTextBlock.Text = $"Основание: {order.Base}";
                OrderContentTextBlock.Text = $"Содержание: {order.Content}";
                OrderEmployeeTextBlock.Text = $"Сотрудник: {order.EmployeeName}";
            }
            else
            {
                OrderNumberTextBlock.Text = "Номер:";
                OrderDateTextBlock.Text = "Дата документа:";
                OrderStartTextBlock.Text = "Дата начала:";
                OrderBaseTextBlock.Text = "Основание:";
                OrderContentTextBlock.Text = "Содержание:";
                OrderEmployeeTextBlock.Text = "Сотрудник:";
            }
        }
        
        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is not OrderDisplay order)
            {
                MessageBox.Show("Выберите приказ для редактирования.");
                return;
            }

            var window = new AddOrderWindow(order.Id);
            window.ShowDialog();
            LoadOrders();
        }

        private void ExportOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is not OrderDisplay order)
            {
                MessageBox.Show("Выберите приказ для экспорта.");
                return;
            }
            if (order.Content.Contains("больнич", StringComparison.OrdinalIgnoreCase))
            {
                ExportSickLeaveToExcel(order);
                return;
            }

            string? templateName = order.Content switch
            {
                var s when s.Contains("прием", StringComparison.OrdinalIgnoreCase) ||
                            s.Contains("приём", StringComparison.OrdinalIgnoreCase)
                        => "orderAgreement.docx",
                var s when s.Contains("увольнен", StringComparison.OrdinalIgnoreCase)
                        => "orderDismissal.docx",
                var s when s.Contains("перевод", StringComparison.OrdinalIgnoreCase)
                        => "orderMoveEmployee.docx",
                var s when s.Contains("командиров", StringComparison.OrdinalIgnoreCase)
                        => "orderBusinessTrip.docx",
                var s when s.Contains("отпуск", StringComparison.OrdinalIgnoreCase)
                        => "orderVacation.docx",
                _ => null
            };

            if (templateName is null)
            {
                MessageBox.Show($"Не удалось определить шаблон для приказа с содержанием:\n{order.Content}", "Ошибка");
                return;
            }

            string templatePath = System.IO.Path.Combine("Templates", templateName);
            if (!System.IO.File.Exists(templatePath))
            {
                MessageBox.Show($"Шаблон не найден:\n{templatePath}", "Ошибка");
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Приказ_{order.RegNumber}.docx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var doc = DocX.Load(templatePath);

                    using var context = new HRDbContext();
                    var orderModel = context.Orders.FirstOrDefault(o => o.Id == order.Id);
                    if (orderModel == null)
                    {
                        MessageBox.Show("Приказ не найден в базе данных.");
                        return;
                    }

                    var emp = context.Employees.FirstOrDefault(e => e.Id == orderModel.EmployeeId);

                    string surename = emp?.Surename ?? string.Empty;
                    string firstname = emp?.FirstName ?? string.Empty;
                    string secondname = emp?.SecondName ?? string.Empty;

                    string position = emp?.PositionId != null ?
                        context.Positions.FirstOrDefault(p => p.Id == emp.PositionId)?.Title ?? string.Empty : string.Empty;

                    string department = emp?.DepartmentId != null ?
                        context.Departments.FirstOrDefault(d => d.Id == emp.DepartmentId)?.Name ?? string.Empty : string.Empty;

                    switch (templateName)
                    {
                        case "orderAgreement.docx":
                            doc.ReplaceText("<DocNumber>", orderModel.RegNumber);
                            doc.ReplaceText("<DocDate>", orderModel.DocDate.ToString("dd.MM.yyyy"));
                            doc.ReplaceText("<HireDate>", orderModel.StartDate.ToString("dd.MM.yyyy"));
                            doc.ReplaceText("<TabNumber>", emp?.TabNumber ?? string.Empty);
                            doc.ReplaceText("<Surename>", surename);
                            doc.ReplaceText("<FirstName>", firstname);
                            doc.ReplaceText("<SecondName>", secondname);
                            doc.ReplaceText("<Department>", department);
                            doc.ReplaceText("<Position>", position);
                            doc.ReplaceText("<Base>", orderModel.Base);
                            break;
                        case "orderDismissal.docx":
                            doc.ReplaceText("<RegNumber>", orderModel.RegNumber);
                            doc.ReplaceText("<DocDate>", orderModel.DocDate.ToString("dd.MM.yyyy"));
                            doc.ReplaceText("<StartDate>", orderModel.StartDate.ToString("dd.MM.yyyy"));
                            doc.ReplaceText("<Surename>", surename);
                            doc.ReplaceText("<FirstName>", firstname);
                            doc.ReplaceText("<SecondName>", secondname);
                            doc.ReplaceText("<Department>", department);
                            doc.ReplaceText("<Position>", position);
                            doc.ReplaceText("<Base>", orderModel.Base);
                            break;
                        case "orderVacation.docx":
                            var vacation = context.Vacations.FirstOrDefault(v => v.EmployeeId == orderModel.EmployeeId && v.StartDate == orderModel.StartDate);
                            DateTime vs = vacation?.StartDate ?? orderModel.StartDate;
                            DateTime ve = vacation?.EndDate ?? orderModel.StartDate;
                            int days = vacation?.CalendarDaysNumber ?? (int)(ve - vs).TotalDays + 1;

                            string MonthName(DateTime d) => Months[d.Month - 1];

                            doc.ReplaceText("<surename>", surename);
                            doc.ReplaceText("<firstname>", firstname);
                            doc.ReplaceText("<secondname>", secondname);
                            doc.ReplaceText("<Servicenumber>", emp?.TabNumber ?? string.Empty);
                            doc.ReplaceText("<workplacetype>", department);
                            doc.ReplaceText("<work>", position);
                            doc.ReplaceText("<VacationType>", vacation?.VacationType ?? string.Empty);
                            doc.ReplaceText("<vacationDaysA>", days.ToString());
                            doc.ReplaceText("<vacationDaysB>", days.ToString());
                            doc.ReplaceText("<vacationDaysC>", days.ToString());

                            void FillDates(string sDay, string sMonth, string sYear, string eDay, string eMonth, string eYear)
                            {
                                doc.ReplaceText(sDay, vs.ToString("dd"));
                                doc.ReplaceText(sMonth, MonthName(vs));
                                doc.ReplaceText(sYear, vs.ToString("yy"));
                                doc.ReplaceText(eDay, ve.ToString("dd"));
                                doc.ReplaceText(eMonth, MonthName(ve));
                                doc.ReplaceText(eYear, ve.ToString("yy"));
                            }

                            FillDates("<wsd>", "<wsmonth>", "<wsy>", "<wed>", "<wemonth>", "<wey>");
                            FillDates("<sda>", "<smontha>", "<sya>", "<eda>", "<emontha>", "<eya>");
                            FillDates("<sdb>", "<smonthb>", "<syb>", "<edb>", "<emonthb>", "<eyb>");
                            FillDates("<sdc>", "<smothc>", "<syc>", "<edc>", "<emonthc>", "<eyc>");

                            doc.ReplaceText("<datetimepicker1>", DateTime.Today.ToString("dd.MM.yyyy"));
                            doc.ReplaceText("<regDate>", orderModel.RegNumber);
                            break;
                        case "orderBusinessTrip.docx":
                            var trip = context.BusinessTrips.FirstOrDefault(t => t.EmployeeId == orderModel.EmployeeId && t.TripStartDate == orderModel.StartDate);
                            DateTime ts = trip?.TripStartDate ?? orderModel.StartDate;
                            DateTime te = trip?.TripEndDate ?? orderModel.StartDate;

                            doc.ReplaceText("<Esurename>", surename);
                            doc.ReplaceText("<Efirstname>", firstname);
                            doc.ReplaceText("<Esecondname>", secondname);
                            doc.ReplaceText("<Servicenumber>", emp?.TabNumber ?? string.Empty);
                            doc.ReplaceText("<workplacetype>", department);
                            doc.ReplaceText("<work>", position);
                            doc.ReplaceText("<aimPlace>", trip?.Destination ?? string.Empty);
                            doc.ReplaceText("<aim>", trip?.Purpose ?? string.Empty);
                            doc.ReplaceText("<tripDays>", ((te - ts).Days + 1).ToString());
                            doc.ReplaceText("<sd>", ts.ToString("dd"));
                            doc.ReplaceText("<smonth>", Months[ts.Month - 1]);
                            doc.ReplaceText("<sy>", ts.ToString("yy"));
                            doc.ReplaceText("<ed>", te.ToString("dd"));
                            doc.ReplaceText("<emonth>", Months[te.Month - 1]);
                            doc.ReplaceText("<ey>", te.ToString("yy"));
                            doc.ReplaceText("<orederInfo>", trip?.Purpose ?? string.Empty);
                            doc.ReplaceText("<regNumber>", orderModel.RegNumber);
                            doc.ReplaceText("<DateTime.Now>", DateTime.Today.ToString("dd.MM.yyyy"));
                            doc.ReplaceText("<Dwork>", "Директор");
                            break;
                        case "orderMoveEmployee.docx":
                            doc.ReplaceText("<Surename>", surename);
                            doc.ReplaceText("<FirstName>", firstname);
                            doc.ReplaceText("<SecondName>", secondname);
                            doc.ReplaceText("<MoveDate>", orderModel.StartDate.ToString("dd.MM.yyyy"));
                            doc.ReplaceText("<Base>", orderModel.Base);
                            doc.ReplaceText("<DocDate>", orderModel.DocDate.ToString("dd.MM.yyyy"));
                            doc.ReplaceText("<Department>", department);
                            doc.ReplaceText("<Position>", position);
                            break;
                    }

                    doc.SaveAs(sfd.FileName);
                    MessageBox.Show("Экспорт завершён.");
                    System.Diagnostics.Process.Start("explorer.exe", sfd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}");
                }
            }
        }
        
        private void ExportSickLeaveToExcel(OrderDisplay order)
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                FileName = $"Больничный_{order.RegNumber}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Больничный");

                    ws.Cell(1, 1).Value = "Номер приказа";
                    ws.Cell(1, 2).Value = "Дата";
                    ws.Cell(1, 3).Value = "Сотрудник";
                    ws.Cell(1, 4).Value = "Дата начала";
                    ws.Cell(1, 5).Value = "Основание";
                    ws.Cell(1, 6).Value = "Содержание";

                    ws.Cell(2, 1).Value = order.RegNumber;
                    ws.Cell(2, 2).Value = order.DocDate.ToShortDateString();
                    ws.Cell(2, 3).Value = order.EmployeeName;
                    ws.Cell(2, 4).Value = order.StartDate.ToShortDateString();
                    ws.Cell(2, 5).Value = order.Base;
                    ws.Cell(2, 6).Value = order.Content;

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

    public class OrderDisplay
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public string RegNumber { get; set; }
        public DateTime DocDate { get; set; }
        public string Base { get; set; }
        public string Content { get; set; }
        public string EmployeeName { get; set; }
        public string Sex { get; set; }
    }
}
