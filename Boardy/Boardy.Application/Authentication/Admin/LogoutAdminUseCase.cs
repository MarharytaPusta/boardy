using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Boardy.Application.Authentication.Admin;

public class LogoutAdminUseCase
{
    private readonly ILogger<LogoutAdminUseCase> _logger;
    private readonly IUserSession _session;

    public LogoutAdminUseCase(ILogger<LogoutAdminUseCase> logger, IUserSession session)
    {
        _logger = logger;
        _session = session;
    }

    public Task<Result> ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("The process of exiting administrator mode has started.");
            _logger.LogInformation("\r\nThe process of exiting administrator mode has been successfully completed");
            _session.SetAdminMode(false);
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\r\nAn error occurred while exiting administrator mode");
            return Task.FromResult(Result.Failure("Не вдалось вийти з акаунта через непередбачувану помилку"));
        }
    }
}