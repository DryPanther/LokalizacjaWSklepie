using LokalizacjaWSklepie.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LokalizacjaWSklepie
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            ConfigureServices(builder.Services); // Dodaj tę linię

            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Tutaj dodaj konfigurację usługi bazy danych
            services.AddDbContext<LokalizacjaWsklepieContext>(options =>
            {
                // Konfiguracja połączenia z bazą danych
                options.UseSqlServer("YourConnectionString");
            });
        }
    }
}