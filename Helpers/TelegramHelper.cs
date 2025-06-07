using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HRApp.Helpers
{
    public static class TelegramHelper
    {
        // Укажи свои данные
        private const string BotToken = "ТВОЙ_BOT_TOKEN"; // <-- сюда вставь свой токен бота из https://t.me/BotFather
        private const string ChatId = "ТВОЙ_CHAT_ID"; // <-- сюда id из https://t.me/userinfobot

        public static async Task<bool> SendMessageAsync(string message)
        {
            using var client = new HttpClient();

            var url = $"https://api.telegram.org/bot{BotToken}/sendMessage";

            var payload = new
            {
                chat_id = ChatId,
                text = message,
                parse_mode = "HTML"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[Telegram API Error] {error}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Telegram Exception] {ex.Message}");
                return false;
            }
        }
    }
}
