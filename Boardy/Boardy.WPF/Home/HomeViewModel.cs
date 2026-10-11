using Boardy.Application.Authentication.Admin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Boardy.WPF.Home;

public partial class HomeViewModel : ObservableObject
{
    private readonly LogoutAdminUseCase _logoutUseCase;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AdminButtonText))]
    private bool _isAdminMode = false;

    public string AdminButtonText => IsAdminMode ? "Вийти з режиму адміністратора" : "Режим адміністратора";


    public HomeViewModel(LogoutAdminUseCase logoutUseCase)
    {
        _logoutUseCase = logoutUseCase;
    }

    [RelayCommand]
    private async Task AdminActionAsync()
    {
        if (IsAdminMode)
        {
            var result = await _logoutUseCase.ExecuteAsync();
            if (result.IsSuccess)
            {
                IsAdminMode = false;
            }
        }
        else
        {
            var mainWindow = System.Windows.Application.Current.MainWindow;
            mainWindow?.Hide();

            var loginControl = new Boardy.WPF.Authentication.AdminLoginView();
            var window = new System.Windows.Window
            {
                Title = "Вхід для адміністратора",
                Content = loginControl,
                SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
            };

            bool ? dialogResult = window.ShowDialog();

            if (dialogResult == true)
            {
                IsAdminMode = true;
            }

            mainWindow?.Show();
        }
    }
}