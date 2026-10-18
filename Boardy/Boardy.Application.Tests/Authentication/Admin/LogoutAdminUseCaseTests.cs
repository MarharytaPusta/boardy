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
        var _mockLogger = new Mock<ILogger<LogoutAdminUseCase>>();
        var _mockSession = new Mock<IUserSession>();
        var useCase = new LogoutAdminUseCase(_mockLogger.Object, _mockSession.Object);

        var result = await useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
    }
}