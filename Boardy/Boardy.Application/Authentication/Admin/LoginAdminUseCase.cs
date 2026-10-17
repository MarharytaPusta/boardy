using Boardy.Domain.Authentication;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Authentication.Admin;

public class LoginAdminUseCase
{
    private readonly IAdminCredentialsRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<LoginAdminUseCase> _logger;
    private readonly IUserSession _session;

    public LoginAdminUseCase(IAdminCredentialsRepository repository, IPasswordHasher hasher, ILogger<LoginAdminUseCase> logger, IUserSession session)
    {
        _repository = repository;
        _hasher = hasher;
        _logger = logger;
        _session = session;
    }

    public async Task<Result> ExecuteAsync(LoginAdminRequest request)
    {
        var settings = await _repository.GetSettingsAsync();

        if (string.IsNullOrWhiteSpace(request.Password)) 
        {
            return Result.Failure("Пароль не може бути порожнім.");
        }

        bool isPasswordValid = _hasher.Verify(request.Password, settings.PasswordHash);

        if (!isPasswordValid) 
        {
            _logger.LogWarning("Uncorrect password entered");
            return Result.Failure("Неправильний пароль адміністратора.");
        }

        _logger.LogInformation("Admin login success");

        _session.SetAdminMode(true);
        return Result.Success();
    }
}
