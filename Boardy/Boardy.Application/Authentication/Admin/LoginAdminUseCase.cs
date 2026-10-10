using Boardy.Domain.Authentication;

namespace Boardy.Application.Authentication.Admin;

public class LoginAdminUseCase
{
    private readonly IAdminCredentialsRepository _repository;
    private readonly IPasswordHasher _hasher;

    public LoginAdminUseCase(IAdminCredentialsRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
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
