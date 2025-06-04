using HikvisionHelpers.Helpers;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Net;

namespace AnprListener
{
    internal class Program
    {
        static AnprSettings anprSettings = new AnprSettings();

        static async Task Main(string[] args)
        {

            SystemConfig();

            #region HTTP Listener

            RequestProcessing _requestProcessing = new RequestProcessing(anprSettings);

            var listener = new HttpListener();
            listener.Prefixes.Add($"http://{anprSettings.ListeningSocket.IpAddress}:{anprSettings.ListeningSocket.Port}/license/");
            listener.Start();

            Log.Information("Hikivision integration trough ISAPI listener");
            Log.Information($"Listening to: http://{anprSettings.ListeningSocket.IpAddress}:{anprSettings.ListeningSocket.Port}/license");


            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                _ = Task.Run(() => _requestProcessing.ProcessRequestAsync(context));
            }
            #endregion
        }


        private static void SystemConfig()
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(config)
               .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                                        //.WriteTo.Console()
               .CreateLogger();

            // Ensure the Microsoft.Extensions.Configuration.Binder package is installed
            anprSettings = config.GetSection("AnprSettings").Get<AnprSettings>();
        }

    }

}