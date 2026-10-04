using System.Configuration;
using System.Data;
using System.Windows;
using Serilog;

namespace Boardy.WPF
{
    public partial class App : System.Windows.Application
    {
        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Information("Boardy application started");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Boardy application closed");

            Log.CloseAndFlush();

            base.OnExit(e);
        }
    }
}
