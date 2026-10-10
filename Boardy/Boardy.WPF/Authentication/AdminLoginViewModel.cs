using Boardy.Application.Authentication.Admin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Boardy.WPF.Authentication;

public partial class AdminLoginViewModel : ObservableObject 
{
    private readonly LoginAdminUseCase _loginUseCase;

    [ObservableProperty]
    private string? _errorMessage;

    public AdminLoginViewModel(LoginAdminUseCase loginUseCase)
    {
        _loginUseCase = loginUseCase;
    }

    [RelayCommand]
    private async Task LoginAsync(object? parameter)
    {
        if (parameter is not PasswordBox passwordBox) return;

        var request = new LoginAdminRequest { Password = passwordBox.Password };
        var result = await _loginUseCase.ExecuteAsync(request);

        if (result.IsSuccess)
        {
            ErrorMessage = string.Empty;
            passwordBox.Clear();

            // Add code to move to the next screen 
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
        // Add code to move to the previous screen
    }
}
