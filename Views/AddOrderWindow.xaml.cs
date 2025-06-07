using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HRApp.Data;
using HRApp.Models;

namespace HRApp.Views
{
    public partial class AddOrderWindow : Window
    {
        private List<Employee> employees;
        private int? editingOrderId = null; // null — добавление, иначе — редактирование

        // Конструктор для добавления
        public AddOrderWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        // Конструктор для редактирования
        public AddOrderWindow(int orderId)
        {
            InitializeComponent();
            editingOrderId = orderId;
            LoadEmployees();
            LoadOrderData(orderId);
        }

        private void LoadEmployees()
        {
            using var context = new HRDbContext();
            employees = context.Employees.ToList();
            EmployeeComboBox.ItemsSource = employees;
            EmployeeComboBox.DisplayMemberPath = "Surename";
        }

        private void LoadOrderData(int orderId)
        {
            using var context = new HRDbContext();
            var order = context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                MessageBox.Show("Приказ не найден.");
                this.Close();
                return;
            }

            StartDateTextBox.Text = order.StartDate.ToShortDateString();
            RegNumberTextBox.Text = order.RegNumber;
            DocDateTextBox.Text = order.DocDate.ToShortDateString();
            BaseTextBox.Text = order.Base;
            ContentTextBox.Text = order.Content;

            // Выбираем сотрудника
            var emp = employees.FirstOrDefault(e => e.Id == order.EmployeeId);
            if (emp != null)
                EmployeeComboBox.SelectedItem = emp;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            if (!DateTime.TryParse(StartDateTextBox.Text, out var startDate) ||
                !DateTime.TryParse(DocDateTextBox.Text, out var docDate))
            {
                MessageBox.Show("Неверный формат дат.");
                return;
            }

            if (string.IsNullOrWhiteSpace(RegNumberTextBox.Text))
            {
                MessageBox.Show("Введите номер приказа.");
                return;
            }

            using var context = new HRDbContext();

            if (editingOrderId == null)
            {
                // Добавление
                var order = new Order
                {
                    StartDate = startDate,
                    RegNumber = RegNumberTextBox.Text.Trim(),
                    DocDate = docDate,
                    Base = BaseTextBox.Text.Trim(),
                    Content = ContentTextBox.Text.Trim(),
                    EmployeeId = selectedEmployee.Id
                };
                context.Orders.Add(order);
            }
            else
            {
                // Обновление
                var order = context.Orders.FirstOrDefault(o => o.Id == editingOrderId.Value);
                if (order == null)
                {
                    MessageBox.Show("Приказ не найден.");
                    return;
                }

                order.StartDate = startDate;
                order.RegNumber = RegNumberTextBox.Text.Trim();
                order.DocDate = docDate;
                order.Base = BaseTextBox.Text.Trim();
                order.Content = ContentTextBox.Text.Trim();
                order.EmployeeId = selectedEmployee.Id;
            }

            context.SaveChanges();
            MessageBox.Show("Приказ сохранён.");
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
