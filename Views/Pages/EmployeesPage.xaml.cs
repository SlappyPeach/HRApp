using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HRApp.Data;
using HRApp.Models;

namespace HRApp.Views
{
    public partial class EmployeesPage : Page
    {
        public ObservableCollection<Employee> EmployeesList { get; set; } = new();

        public EmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();
            EmployeesDataGrid.ItemsSource = EmployeesList;
        }

        private void LoadEmployees()
        {
            EmployeesList.Clear();
            using var context = new HRDbContext();
            var employees = context.Employees.ToList();
            foreach (var emp in employees)
                EmployeesList.Add(emp);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void SexFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string query = SearchTextBox.Text.Trim().ToLower();
            string selectedSex = (SexFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            using var context = new HRDbContext();
            var employees = context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(query))
                employees = employees.Where(emp => emp.Surename.ToLower().Contains(query));

            if (selectedSex != null && selectedSex != "Все")
                employees = employees.Where(emp => emp.Sex == selectedSex);

            var result = employees.ToList();

            EmployeesList.Clear();
            foreach (var emp in result)
                EmployeesList.Add(emp);
        }

        private void EmployeesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem is Employee emp)
            {
                CardTabNumberTextBlock.Text = $"Таб. №: {emp.TabNumber}";
                CardNameTextBlock.Text = $"ФИО: {emp.Surename} {emp.FirstName} {emp.SecondName}";
                CardBirthTextBlock.Text = $"Дата рождения: {emp.BirthDate.ToShortDateString()}";
                CardCitizenshipTextBlock.Text = $"Гражданство: {emp.Citizenship}";
                CardTelTextBlock.Text = $"Телефон: {emp.TelNumber}";
                CardDismissTextBlock.Text = $"Дата увольнения: {(emp.DismissalDate?.ToShortDateString() ?? "—")}";
            }
            else
            {
                CardTabNumberTextBlock.Text = "Таб. №:";
                CardNameTextBlock.Text = "ФИО:";
                CardBirthTextBlock.Text = "Дата рождения:";
                CardCitizenshipTextBlock.Text = "Гражданство:";
                CardTelTextBlock.Text = "Телефон:";
                CardDismissTextBlock.Text = "Дата увольнения:";
            }
        }

        #region Кнопки действий
        private void T2FormButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem is Employee emp)
                new T2FormWindow(emp.Id).ShowDialog();
            else
                new T2FormWindow().ShowDialog();

            LoadEmployees();
        }

        private void HireButton_Click(object sender, RoutedEventArgs e)
        {
            new HireWindow().ShowDialog();
            LoadEmployees();
        }

        private void BusinessTripButton_Click(object sender, RoutedEventArgs e)
        {
            new BusinessTripWindow().ShowDialog();
            LoadEmployees();
        }

        private void VacationButton_Click(object sender, RoutedEventArgs e)
        {
            new VacationWindow().ShowDialog();
            LoadEmployees();
        }

        private void SickLeaveButton_Click(object sender, RoutedEventArgs e)
        {
            new SickLeaveWindow().ShowDialog();
            LoadEmployees();
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            new MoveWindow().ShowDialog();
            LoadEmployees();
        }

        private void CertificationButton_Click(object sender, RoutedEventArgs e)
        {
            new CertificationWindow().ShowDialog();
            LoadEmployees();
        }

        private void EmploymentHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            new EmploymentHistoryWindow().ShowDialog();
            LoadEmployees();
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem is Employee emp)
            {
                using var context = new HRDbContext();
                var empDb = context.Employees.FirstOrDefault(x => x.Id == emp.Id);
                if (empDb != null)
                {
                    empDb.DismissalDate = System.DateTime.Today;
                    context.SaveChanges();
                    LoadEmployees();
                    MessageBox.Show("Сотрудник уволен.");
                }
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для увольнения.");
            }
        }

        private void AwardsButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesDataGrid.SelectedItem is Employee emp)
            {
                var awardsWindow = new AwardsWindow(emp.Id);
                awardsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для просмотра наград.");
            }
        }

        #endregion
    }
}
