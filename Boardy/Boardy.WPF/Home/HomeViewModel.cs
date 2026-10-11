using Boardy.Application.Authentication.Admin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Boardy.WPF.Home;

public partial class HomeViewModel : ObservableObject
{
    private readonly LogoutAdminUseCase _logoutUseCase;
    private readonly UserSession _userSession;

    public bool IsAdminMode
    {
        get => _userSession.IsAdminMode;
        set
        {
            _userSession.IsAdminMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AdminButtonText));
        }
    }

    public string AdminButtonText => IsAdminMode ? "Вийти з режиму адміністратора" : "Режим адміністратора";


    public HomeViewModel(LogoutAdminUseCase logoutUseCase, UserSession userSession)
    {
        _logoutUseCase = logoutUseCase;
        _userSession = userSession;
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
                Width = 800,
                Height = 450,
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