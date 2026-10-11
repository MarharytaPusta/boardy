using Boardy.Application.Authentication.Admin;
using Boardy.WPF.Authentication;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
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
            var loginViewModel = App.ServiceProvider.GetRequiredService<AdminLoginViewModel>();
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(loginViewModel));
        }
    }
}