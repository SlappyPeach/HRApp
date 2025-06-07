using HRApp.Data;
using System;
using System.Linq;

namespace HRApp.Helpers
{
    public static class OrderNumberGenerator
    {
        public static string Generate(HRDbContext context, string prefix)
        {
            string year = DateTime.Now.Year.ToString();

            // Ищем количество приказов с этим префиксом и годом
            int count = context.Orders
                .Count(o => o.RegNumber.StartsWith($"{prefix}-{year}"));

            // Формируем следующий номер
            return $"{prefix}-{year}-{(count + 1):D4}";
        }
    }
}
