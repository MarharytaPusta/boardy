using Boardy.Domain.Authentication;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Authentication.Admin;

public class ChangeAdminPasswordUseCase
{
    private readonly IAdminCredentialsRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<ChangeAdminPasswordUseCase> _logger;

    public ChangeAdminPasswordUseCase(
        IAdminCredentialsRepository repository,
        IPasswordHasher hasher,
        ILogger<ChangeAdminPasswordUseCase> logger)
    {
        _repository = repository;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<ChangeAdminPasswordResult> ExecuteAsync(ChangeAdminPasswordRequest request)
    {
        var settings = await _repository.GetSettingsAsync();

        if (!_hasher.Verify(request.CurrentPassword, settings.PasswordHash))
        {
            _logger.LogWarning("An attempt was made to change the administrator password with an incorrect current password");
            return new ChangeAdminPasswordResult { ErrorMessage = "Неправильний поточний пароль адміністратора." };
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return new ChangeAdminPasswordResult { ErrorMessage = "Новий пароль не може бути порожнім." };
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return new ChangeAdminPasswordResult { ErrorMessage = "Нові паролі не співпадають." };
        }

        settings.PasswordHash = _hasher.Hash(request.NewPassword);
        await _repository.SaveSettingAsync(settings);

        _logger.LogInformation("Administrator password changed successfully");
        return new ChangeAdminPasswordResult { IsSuccess = true };
    }
}
