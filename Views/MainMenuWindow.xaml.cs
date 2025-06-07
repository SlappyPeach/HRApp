using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HRApp.Views; // папка, где лежат страницы
using System.Windows.Navigation;

namespace HRApp.Views
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();

            // Подписываемся на событие навигации, чтобы скрывать логотип при загрузке страниц
            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Если в Frame загружен какой-либо контент, скрываем логотип; иначе показываем
            if (MainFrame.Content != null)
                LogoImage.Visibility = Visibility.Collapsed;
            else
                LogoImage.Visibility = Visibility.Visible;
        }

        private void JournalButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageTextBlock.Text = "Журнал";
            MainFrame.Navigate(new JournalPage());
        }

        private void EmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageTextBlock.Text = "Сотрудники";
            MainFrame.Navigate(new EmployeesPage());
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageTextBlock.Text = "Приказы";
            MainFrame.Navigate(new OrdersPage());
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageTextBlock.Text = "Отчёты";
            MainFrame.Navigate(new ReportsPage());
        }

        private void TimeSheetButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageTextBlock.Text = "Табель";
            MainFrame.Navigate(new TimeSheetPage());
        }

        private void RemindersButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPageTextBlock.Text = "Напоминания";
            MainFrame.Navigate(new RemindersPage());
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

