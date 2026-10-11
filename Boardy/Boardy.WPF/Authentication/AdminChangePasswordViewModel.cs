using Boardy.Application.Authentication.Admin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Boardy.WPF.Authentication;

public partial class AdminChangePasswordViewModel : ObservableObject
{
    private readonly ChangeAdminPasswordUseCase _changePasswordUseCase;
    private readonly ILogger<AdminChangePasswordViewModel> _logger;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public AdminChangePasswordViewModel(
        ChangeAdminPasswordUseCase changePasswordUseCase,
        ILogger<AdminChangePasswordViewModel> logger)
    {
        _changePasswordUseCase = changePasswordUseCase;
        _logger = logger;
    }

    [RelayCommand]
    private async Task ChangePasswordAsync(object? parameter)
    {
        if (parameter is not AdminChangePasswordView view)
        {
            return;
        }

        _logger.LogInformation("Button 'Змінити пароль' clicked");
        var request = new ChangeAdminPasswordRequest
        {
            CurrentPassword = view.CurrentPasswordBox.Password,
            NewPassword = view.NewPasswordBox.Password,
            ConfirmNewPassword = view.ConfirmPasswordBox.Password
        };
        var result = await _changePasswordUseCase.ExecuteAsync(request);

        ErrorMessage = result.ErrorMessage;
        SuccessMessage = result.IsSuccess ? "Пароль успішно змінено." : string.Empty;

        view.CurrentPasswordBox.Clear();
        view.NewPasswordBox.Clear();
        view.ConfirmPasswordBox.Clear();
    }
}
