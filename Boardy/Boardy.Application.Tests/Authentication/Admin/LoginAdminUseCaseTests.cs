using Boardy.Application.Authentication.Admin;
using Boardy.Domain.Authentication;
using Microsoft.Extensions.Logging;
using Moq;

namespace Boardy.Application.Tests.Authentication.Admin;

public class LoginAdminUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CorrectPassword_ReturnsSuccess()
    {
        var mockRepo = new Mock<IAdminCredentialsRepository>();
        var mockHasher = new Mock<IPasswordHasher>();
        var mockLogger = new Mock<ILogger<LoginAdminUseCase>>();

        string inputPassword = "CorrectPassword123";
        string dbHash = "hashed_version_of_password";

        mockRepo.Setup(r => r.GetSettingsAsync())
            .ReturnsAsync(new AdminSettings { PasswordHash = dbHash });

        mockHasher.Setup(h => h.Verify(inputPassword, dbHash))
            .Returns(true);

        var useCase = new LoginAdminUseCase(mockRepo.Object, mockHasher.Object, mockLogger.Object);
        var request = new LoginAdminRequest { Password = inputPassword };

        var result = await useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_IncorrectPassword_ReturnsFailureAndErrorMessage()
    {
        var mockRepo = new Mock<IAdminCredentialsRepository>();
        var mockHasher = new Mock<IPasswordHasher>();
        var mockLogger = new Mock<ILogger<LoginAdminUseCase>>();

        string inputPassword = "WrongPassword";
        string dbHash = "hashed_version_of_password";

        mockRepo.Setup(r => r.GetSettingsAsync())
            .ReturnsAsync(new AdminSettings { PasswordHash = dbHash });

        mockHasher.Setup(h => h.Verify(inputPassword, dbHash))
            .Returns(false);

        var useCase = new LoginAdminUseCase(mockRepo.Object, mockHasher.Object, mockLogger.Object);
        var request = new LoginAdminRequest { Password = inputPassword };

        var result = await useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Неправильний пароль адміністратора.", result.ErrorMessage);
    }
}
