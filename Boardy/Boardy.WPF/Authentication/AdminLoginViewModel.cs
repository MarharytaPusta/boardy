using Boardy.Application.Authentication.Admin;
using Boardy.WPF.Home;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows.Controls;

namespace Boardy.WPF.Authentication;

public partial class AdminLoginViewModel : ObservableObject 
{
    private readonly LoginAdminUseCase _loginUseCase;
    private readonly ILogger<AdminLoginViewModel> _logger;

    [ObservableProperty]
    private string? _errorMessage;

    public AdminLoginViewModel(LoginAdminUseCase loginUseCase, ILogger<AdminLoginViewModel> logger)
    {
        _loginUseCase = loginUseCase;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoginAsync(object? parameter)
    {
        if (parameter is not PasswordBox passwordBox) return;

        _logger.LogInformation("Button 'Вхід' clicked.");

        var request = new LoginAdminRequest { Password = passwordBox.Password };
        var result = await _loginUseCase.ExecuteAsync(request);

        if (result.IsSuccess)
        {
            ErrorMessage = string.Empty;
            passwordBox.Clear();

            _logger.LogInformation("Move to administrator panel");

            var homeViewModel = App.ServiceProvider.GetRequiredService<HomeViewModel>();
            homeViewModel.IsAdminMode = true;
            WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeViewModel));
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
            passwordBox.Clear();
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInformation("Button 'Назад' clicked. Move to main menu");

        var homeViewModel = App.ServiceProvider.GetRequiredService<HomeViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(homeViewModel));
    }

    [RelayCommand]
    private void GoToChangePassword()
    {
        _logger.LogInformation("Button 'Змінити пароль' clicked. Move to change password screen");

        var changePasswordVm = App.ServiceProvider.GetRequiredService<AdminChangePasswordViewModel>();
        WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(changePasswordVm));
    }
}
