using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Boardy.Application.Authentication.Admin;

public class LogoutAdminUseCase
{
    private readonly ILogger<LogoutAdminUseCase> _logger;

    public LogoutAdminUseCase(ILogger<LogoutAdminUseCase> logger)
    {
        _logger = logger;
    }

    public Task<LogoutAdminResult> ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("Ініційовано процес виходу з режиму адміністратора");
            _logger.LogInformation("Вихід з режиму адміністратора успішно завершено");

            return Task.FromResult(LogoutAdminResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Сталася помилка під час виходу з режиму адміністратора");
            return Task.FromResult(LogoutAdminResult.Failure("Не вдалося виконати вихід через внутрішню помилку"));
        }
    }
}