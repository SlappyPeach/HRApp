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

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddOrderWindow();
            window.ShowDialog();
            LoadOrders();
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
                    var emp = context.Employees.FirstOrDefault(e =>
                        order.EmployeeName.Contains(e.Surename) &&
                        order.EmployeeName.Contains(e.FirstName));

                    string surename = emp?.Surename ?? "";
                    string firstname = emp?.FirstName ?? "";
                    string secondname = emp?.SecondName ?? "";

                    doc.ReplaceText("<RegNumber>", order.RegNumber);
                    doc.ReplaceText("<DocDate>", order.DocDate.ToShortDateString());
                    doc.ReplaceText("<StartDate>", order.StartDate.ToShortDateString());
                    doc.ReplaceText("<Base>", order.Base);
                    doc.ReplaceText("<Content>", order.Content);
                    doc.ReplaceText("<EmployeeName>", order.EmployeeName);
                    doc.ReplaceText("<ExportDate>", DateTime.Today.ToShortDateString());

                    doc.ReplaceText("<Surename>", surename);
                    doc.ReplaceText("<FirstName>", firstname);
                    doc.ReplaceText("<SecondName>", secondname);

                    string position = emp?.PositionId != null ?
                        context.Positions.FirstOrDefault(p => p.Id == emp.PositionId)?.Title ?? "" : "";

                    string department = emp?.DepartmentId != null ?
                        context.Departments.FirstOrDefault(d => d.Id == emp.DepartmentId)?.Name ?? "" : "";

                    doc.ReplaceText("<Position>", position);
                    doc.ReplaceText("<Department>", department);


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
