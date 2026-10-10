using Boardy.Domain.Authentication;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Authentication.Admin;

public class LoginAdminUseCase
{
    private readonly IAdminCredentialsRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<LoginAdminUseCase> _logger;

    public LoginAdminUseCase(IAdminCredentialsRepository repository, IPasswordHasher hasher, ILogger<LoginAdminUseCase> logger)
    {
        _repository = repository;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<LoginAdminResult> ExecuteAsync(LoginAdminRequest request)
    {
        var settings = await _repository.GetSettingsAsync();

        bool isPasswordValid = _hasher.Verify(request.Password, settings.PasswordHash);

        if (!isPasswordValid) 
        {
            return new LoginAdminResult
            {
                IsSuccess = false,
                ErrorMessage = "Неправильний пароль адміністратора."
            };
        }

        return new LoginAdminResult { IsSuccess = true };
    }
}
