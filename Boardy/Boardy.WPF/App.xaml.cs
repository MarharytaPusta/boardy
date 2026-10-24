using Boardy.Application.Authentication.Admin;
using Boardy.Application.GameCatalog.Catalog;
using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Application.Player;
using Boardy.Application.Shared;
using Boardy.Domain.Authentication;
using Boardy.Infrastructure;
using Boardy.Infrastructure.Authentication;
using Boardy.Infrastructure.Repositories;
using Boardy.WPF.Authentication;
using Boardy.WPF.GameCatalog;
using Boardy.WPF.Home;
using Boardy.WPF.Player;
using Boardy.WPF.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

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

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddDbContext<BoardyDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString);
            });

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

            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddTransient<CreatePlayerUseCase>();
            services.AddTransient<GetAllPlayers>();
            services.AddTransient<UpdatePlayerUseCase>();

            services.AddTransient<DeletePlayerUseCase>();

            services.AddTransient<CreatePlayerViewModel>();
            services.AddTransient<PlayersViewModel>();
            services.AddTransient<EditPlayerViewModel>();

            services.AddSingleton<IUserSession, UserSession>();

            services.AddSingleton<IDialogService, DialogService>();

            services.AddScoped<IScoreTemplateRepository, ScoreTemplateRepository>();
            services.AddTransient<CreateScoreTemplateUseCase>();
            services.AddTransient<CreateGameViewModel>();

            services.AddScoped<IGameRepository, GameRepository>();
            services.AddTransient<CreateGameUseCase>();

            services.AddScoped<IGameCatalogRepository, GameCatalogRepository>();
            services.AddTransient<GetGameCatalogUseCase>();
            services.AddTransient<GameCatalogViewModel>();

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
