using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Soulfire.Bot.Services;

namespace Soulfire.Bot
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration(x =>
                {
                    x.AddUserSecrets<Program>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient("newsClient", x =>
                    {
                        x.BaseAddress = new System.Uri("https://newsapi.org/v2/");
                    });

                    services.AddScoped<NewsService>();

                    services.AddLogging();
                    services.AddSingleton<IChatService, TelegramService>();
                    services.AddBotCommands();
                    services.AddHostedService<Bot>();
                });
    }
}
