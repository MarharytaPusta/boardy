using Boardy.Application.Authentication.Admin;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Boardy.Application.Tests.Authentication.Admin;

public class LogoutAdminUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_Always_ReturnsSuccess()
    {
        var mockLogger = new Mock<ILogger<LogoutAdminUseCase>>();
        var mockSession = new Mock<IUserSession>();
        var useCase = new LogoutAdminUseCase(mockLogger.Object, mockSession.Object);

        var result = await useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
    }
}