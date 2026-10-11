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
            _logger.LogInformation("The process of exiting administrator mode has started.");
            _logger.LogInformation("\r\nThe process of exiting administrator mode has been successfully completed");

            return Task.FromResult(LogoutAdminResult.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\r\nAn error occurred while exiting administrator mode");
            return Task.FromResult(LogoutAdminResult.Failure("Failed to log out due to an internal error"));
        }
    }
}