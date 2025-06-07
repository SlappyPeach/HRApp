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
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HRApp.Data;
using HRApp.Views; // где лежит MainMenuWindow

namespace HRApp.Views
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();

            // Устанавливаем текущую дату
            DateTextBlock.Text = $"Дата: {DateTime.Now:dd.MM.yyyy}";

            // Загружаем данные о погоде для Москвы
            _ = LoadWeatherAsync();
        }

        /// <summary>
        /// Загружает данные о погоде для Байконура через API OpenWeatherMap.
        /// </summary>
        private async Task LoadWeatherAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Замените "YOUR_API_KEY" на свой ключ API OpenWeatherMap
                    string apiKey = "0508e6a8230e2b494a97a3537090f905";
                    // Используем город Байконур для запроса
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q=Baikonur&appid={apiKey}&units=metric&lang=ru";

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();

                    WeatherResponse weatherResponse = JsonSerializer.Deserialize<WeatherResponse>(json);

                    // Обновляем UI элементы на UI-потоке
                    Dispatcher.Invoke(() =>
                    {
                        if (weatherResponse != null && weatherResponse.weather.Length > 0)
                        {
                            WeatherTextBlock.Text = weatherResponse.weather[0].description;
                            TemperatureTextBlock.Text = $"{weatherResponse.main.temp}°C";
                            WindTextBlock.Text = $"{weatherResponse.wind.speed} м/с";
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка получения данных о погоде: " + ex.Message);
            }
        }

        // Классы для десериализации ответа API
        private class WeatherResponse
        {
            public WeatherDetail[] weather { get; set; }
            public MainDetail main { get; set; }
            public WindDetail wind { get; set; }
        }

        private class WeatherDetail
        {
            public string description { get; set; }
        }

        private class MainDetail
        {
            public double temp { get; set; }
        }

        private class WindDetail
        {
            public double speed { get; set; }
        }

        // Обработчик кнопки "Войти"
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Считываем логин и пароль с формы
            string login = LoginTextBox.Text;
            string password = (PasswordBox.Visibility == Visibility.Visible)
                ? PasswordBox.Password
                : PasswordTextBox.Text;

            // Проверяем наличие пользователя в БД
            using (var context = new HRDbContext())
            {
                var user = context.Registers
                                  .FirstOrDefault(u => u.LoginUser == login && u.PasswordUser == password);

                if (user != null)
                {
                    // Если пользователь найден, открываем главное меню
                    MainMenuWindow mainMenu = new MainMenuWindow();
                    mainMenu.Show();

                    // Закрываем окно авторизации
                    this.Close();
                }
                else
                {
                    // Выводим сообщение об ошибке
                    MessageBox.Show("Неверный логин или пароль!",
                                    "Ошибка авторизации",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        // Обработчик кнопки "Отмена"
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Обработчики чекбокса "Показать пароль"
        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
        }
    }
}


