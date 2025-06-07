using System.Configuration;
using System.Data;
using System.Windows;
using HRApp.Data;

namespace HRApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var context = new HRDbContext())
            {
                // Создаём базу данных и таблицы, если их ещё нет
                context.Database.EnsureCreated();
            }            
        }
    }
}

