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

namespace HRApp.Views
{
    public partial class MoveWindow : Window
    {
        private List<Employee> employees;

        public MoveWindow()
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

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            if (!DateTime.TryParse(MoveDateTextBox.Text, out var moveDate))
            {
                MessageBox.Show("Некорректная дата перевода.");
                return;
            }

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "orderMoveEmployee.docx");
            if (!File.Exists(templatePath))
            {
                MessageBox.Show($"Шаблон приказа не найден по пути:\n{templatePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                FileName = $"Перевод_{selectedEmployee.Surename}.docx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var doc = DocX.Load(templatePath);

                    // Плейсхолдеры
                    doc.ReplaceText("<Surename>", selectedEmployee.Surename);
                    doc.ReplaceText("<FirstName>", selectedEmployee.FirstName);
                    doc.ReplaceText("<SecondName>", selectedEmployee.SecondName);
                    doc.ReplaceText("<NewDepartment>", NewDepartmentTextBox.Text.Trim());
                    doc.ReplaceText("<NewPosition>", NewPositionTextBox.Text.Trim());
                    doc.ReplaceText("<MoveDate>", moveDate.ToShortDateString());
                    doc.ReplaceText("<Base>", BaseTextBox.Text.Trim());
                    doc.ReplaceText("<MoveType>", MoveTypeTextBox.Text.Trim());
                    doc.ReplaceText("<DocDate>", DateTime.Today.ToShortDateString());
                    

                    doc.SaveAs(sfd.FileName);
                    MessageBox.Show("Приказ сохранён.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}");
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeComboBox.SelectedItem is not Employee selectedEmployee)
            {
                MessageBox.Show("Выберите сотрудника.");
                return;
            }

            if (!DateTime.TryParse(MoveDateTextBox.Text, out var moveDate))
            {
                MessageBox.Show("Некорректная дата перевода.");
                return;
            }

            string newDepartment = NewDepartmentTextBox.Text.Trim();
            string newPosition = NewPositionTextBox.Text.Trim();
            string baseText = BaseTextBox.Text.Trim();
            string moveType = MoveTypeTextBox.Text.Trim();

            using var context = new HRDbContext();

            // Генерация приказа
            var order = new Order
            {
                EmployeeId = selectedEmployee.Id,
                StartDate = moveDate,
                Content = "Приказ о переводе",
                DocDate = DateTime.Today,
                Base = baseText,
                RegNumber = OrderNumberGenerator.Generate(context, "ПЕР"),
                MoveType = moveType,
                NewDepartment = newDepartment,
                NewPosition = newPosition
            };
            context.Orders.Add(order);

            // Запись в журнал
            context.RegisterDocuments.Add(new RegisterDocument
            {
                RegNumber = order.RegNumber,
                RegDate = DateTime.Today,
                DocType = "Приказ о переводе"
            });

            context.SaveChanges();
            MessageBox.Show($"Перевод зарегистрирован. Приказ № {order.RegNumber}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
