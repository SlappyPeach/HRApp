using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using HRApp.Views;

namespace HRApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Обработчик для кнопки "Вход"
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр окна авторизации
            AuthWindow authWindow = new AuthWindow();
            authWindow.Show();

            // Закрываем текущее главное окно (если оно больше не нужно)
            this.Close();
        }

        // Обработчик для кнопки "Выход"
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
