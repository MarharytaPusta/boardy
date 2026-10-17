using Boardy.Domain.Shared;
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

    public Task<Result> ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("The process of exiting administrator mode has started.");
            _logger.LogInformation("\r\nThe process of exiting administrator mode has been successfully completed");

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\r\nAn error occurred while exiting administrator mode");
            return Task.FromResult(Result.Failure("Не вдалось вийти з акаунта через непередбачувану помилку"));
        }
    }
}