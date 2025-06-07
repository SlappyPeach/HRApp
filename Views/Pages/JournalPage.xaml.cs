using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HRApp.Data;
using HRApp.Models;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace HRApp.Views
{
    public partial class JournalPage : Page
    {
        public ObservableCollection<RegisterDocument> JournalList { get; set; } = new();

        public JournalPage()
        {
            InitializeComponent();
            LoadJournal();
            JournalDataGrid.ItemsSource = JournalList;
        }

        // Загрузка всех записей
        private void LoadJournal()
        {
            JournalList.Clear();
            using var context = new HRDbContext();
            var records = context.RegisterDocuments.OrderByDescending(r => r.RegDate).ToList();
            foreach (var record in records)
                JournalList.Add(record);
        }

        // При выборе строки — показать связанный документ
        private void JournalDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (JournalDataGrid.SelectedItem is RegisterDocument selected)
            {
                DocNumberTextBlock.Text = $"Номер: {selected.RegNumber}";
                DocDateTextBlock.Text = $"Дата: {selected.RegDate.ToShortDateString()}";
                DocTypeTextBlock.Text = $"Тип: {selected.DocType}";

                // Подсветка по типу
                if (selected.DocType.Contains("Входящий"))
                    DocTypeTextBlock.Foreground = Brushes.Green;
                else if (selected.DocType.Contains("Исходящий"))
                    DocTypeTextBlock.Foreground = Brushes.Blue;
                else if (selected.DocType.Contains("Внутренний"))
                    DocTypeTextBlock.Foreground = Brushes.Orange;
                else
                    DocTypeTextBlock.Foreground = Brushes.Black;

                // Очистка
                DocSourceTextBlock.Text = "Источник/Получатель: —";
                DocShortInfoTextBlock.Text = "Краткое содержание: —";
                DocSheetsTextBlock.Text = "Листов: —";

                using var context = new HRDbContext();

                // Подгрузка связанного документа
                if (selected.DocType.Contains("Входящий"))
                {
                    var doc = context.IncomingDocuments.FirstOrDefault(d => d.RegNumber == selected.RegNumber);
                    if (doc != null)
                    {
                        DocSourceTextBlock.Text = $"Источник: {doc.Sender}";
                        DocShortInfoTextBlock.Text = $"Краткое содержание: {doc.ShortInfo}";
                        DocSheetsTextBlock.Text = "Листов: —";
                    }
                }
                else if (selected.DocType.Contains("Исходящий"))
                {
                    var doc = context.OutgoingDocuments.FirstOrDefault(d => d.RegNumber == selected.RegNumber);
                    if (doc != null)
                    {
                        DocSourceTextBlock.Text = $"Получатель: {doc.Destination}";
                        DocShortInfoTextBlock.Text = $"Краткое содержание: {doc.ShortInfo}";
                        DocSheetsTextBlock.Text = $"Листов: {doc.SheetsNumber}";
                    }
                }
                else if (selected.DocType.Contains("Внутренний"))
                {
                    var doc = context.InternalDocuments.FirstOrDefault(d => d.RegNumber == selected.RegNumber);
                    if (doc != null)
                    {
                        DocSourceTextBlock.Text = $"Назначение: {doc.Destination}";
                        DocShortInfoTextBlock.Text = $"Краткое содержание: {doc.ShortInfo}";
                        DocSheetsTextBlock.Text = $"Листов: {doc.SheetsNumber}";
                    }
                }
                else if (selected.DocType.StartsWith("Приказ"))
                {
                    var order = context.Orders.FirstOrDefault(o => o.RegNumber == selected.RegNumber);
                    if (order != null)
                    {
                        var emp = context.Employees.FirstOrDefault(e => e.Id == order.EmployeeId);
                        if (emp != null)
                        {
                            DocSourceTextBlock.Text = $"Сотрудник: {emp.Surename} {emp.FirstName} {emp.SecondName}";
                        }

                        DocShortInfoTextBlock.Text = $"Содержание: {order.Content}";
                        DocSheetsTextBlock.Text = $"Дата начала действия: {order.StartDate.ToShortDateString()}";
                    }
                }
                else if (selected.DocType == "Больничный")
                {
                    var sick = context.SickLeaves.FirstOrDefault(s => s.RegNumber == selected.RegNumber);
                    if (sick != null)
                    {
                        var emp = context.Employees.FirstOrDefault(e => e.Id == sick.EmployeeId);
                        if (emp != null)
                        {
                            DocSourceTextBlock.Text = $"Сотрудник: {emp.Surename} {emp.FirstName} {emp.SecondName}";
                        }

                        DocShortInfoTextBlock.Text = $"Мед. учреждение: {sick.MedStateName}";
                        DocSheetsTextBlock.Text = $"Период: {sick.FromDate:dd.MM.yyyy} — {sick.ToDate:dd.MM.yyyy}";
                    }
                }

            }
            else
            {
                // Очистить карточку
                DocNumberTextBlock.Text = "Номер:";
                DocDateTextBlock.Text = "Дата:";
                DocTypeTextBlock.Text = "Тип:";
                DocSourceTextBlock.Text = "Источник/Получатель:";
                DocShortInfoTextBlock.Text = "Краткое содержание:";
                DocSheetsTextBlock.Text = "Листов:";
            }
        }

        // Применить фильтр
        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            string numQuery = SearchNumberTextBox.Text.Trim().ToLower();
            string selectedType = (DocTypeFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var dateSelected = DateFilterPicker.SelectedDate;

            using var context = new HRDbContext();
            var query = context.RegisterDocuments.AsQueryable();

            if (!string.IsNullOrEmpty(numQuery))
                query = query.Where(d => d.RegNumber.ToLower().Contains(numQuery));

            if (selectedType != "Все")
                query = query.Where(d => d.DocType.Contains(selectedType));

            if (dateSelected != null)
                query = query.Where(d => d.RegDate.Date == dateSelected.Value.Date);

            var filtered = query.OrderByDescending(r => r.RegDate).ToList();
            JournalList.Clear();
            foreach (var doc in filtered)
                JournalList.Add(doc);
        }

        // Очистить фильтр
        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            SearchNumberTextBox.Text = "";
            DateFilterPicker.SelectedDate = null;
            DocTypeFilterComboBox.SelectedIndex = 0;
            LoadJournal();
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (JournalList.Count == 0)
            {
                MessageBox.Show("Нет записей для экспорта.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                FileName = $"Журнал_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Журнал документов");

                    // Заголовки
                    ws.Cell(1, 1).Value = "Номер документа";
                    ws.Cell(1, 2).Value = "Дата регистрации";
                    ws.Cell(1, 3).Value = "Тип документа";

                    // Данные
                    for (int i = 0; i < JournalList.Count; i++)
                    {
                        var doc = JournalList[i];
                        ws.Cell(i + 2, 1).Value = doc.RegNumber;
                        ws.Cell(i + 2, 2).Value = doc.RegDate.ToShortDateString();
                        ws.Cell(i + 2, 3).Value = doc.DocType;
                    }

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(sfd.FileName);
                    MessageBox.Show("Экспорт завершён.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}");
                }
            }
        }
    }
}
