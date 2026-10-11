using System.Configuration;
using System.Data;
using System.Windows;
using Boardy.Application.Authentication.Admin;
using Boardy.Domain.Authentication;
using Boardy.Infrastructure.Authentication;
using Boardy.WPF.Authentication;
using Boardy.WPF.Home;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Boardy.WPF
{
    public partial class App : System.Windows.Application
    {

        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Information("Boardy application started");
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(dispose: true);
            });

            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IAdminCredentialsRepository, FileAdminCredentialsRepository>();

            services.AddSingleton<MainViewModel>();

            services.AddTransient<LoginAdminUseCase>();
            services.AddTransient<ChangeAdminPasswordUseCase>();

            services.AddTransient<AdminLoginViewModel>();
            services.AddTransient<AdminChangePasswordViewModel>();

            services.AddTransient<LogoutAdminUseCase>();
            services.AddTransient<HomeViewModel>();

            services.AddSingleton<Boardy.Application.Authentication.Admin.UserSession>();

            ServiceProvider = services.BuildServiceProvider();

            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Boardy application closed");

            Log.CloseAndFlush();

            base.OnExit(e);
        }
    }
}
