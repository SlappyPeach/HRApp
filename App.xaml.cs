using System.Configuration;
using System.Data;
using System.Windows;
using HRApp.Data;
using Microsoft.EntityFrameworkCore;

namespace HRApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var context = new HRDbContext())
            {
                // Создаем или обновляем базу данных до последней версии
                context.Database.Migrate();
            }            
        }
    }
}

