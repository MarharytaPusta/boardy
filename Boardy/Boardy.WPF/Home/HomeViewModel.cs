using Boardy.Application.Authentication.Admin;
using Boardy.WPF.Authentication;
using Boardy.WPF.Player;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Boardy.WPF.Home;

public partial class HomeViewModel : ObservableObject
{
    private readonly LogoutAdminUseCase _logoutUseCase;
    private readonly IUserSession _userSession;

    public bool IsAdminMode => _userSession.IsAdminMode;
      
    public string AdminButtonText => IsAdminMode ? "Вийти з режиму адміністратора" : "Режим адміністратора";


    public HomeViewModel(LogoutAdminUseCase logoutUseCase, IUserSession userSession)
    {
        _logoutUseCase = logoutUseCase;
        _userSession = userSession;

        _userSession.OnAdminStateChanged += UpdateState;
    }

    private void UpdateState()
    {
        OnPropertyChanged(nameof(IsAdminMode));
        OnPropertyChanged(nameof(AdminButtonText));
    }

    [RelayCommand]
    private async Task AdminActionAsync()
    {
        if (IsAdminMode)
        {
            await _logoutUseCase.ExecuteAsync();
        }
        else
        {
            var loginViewModel = App.ServiceProvider.GetRequiredService<AdminLoginViewModel>();
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(loginViewModel));
        }
    }
}