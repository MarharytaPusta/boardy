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
        // 1. Arrange (Підготовка)
        var mockRepo = new Mock<IAdminCredentialsRepository>();
        var mockHasher = new Mock<IPasswordHasher>();
        var mockLogger = new Mock<ILogger<LoginAdminUseCase>>();

        string inputPassword = "CorrectPassword123";
        string dbHash = "hashed_version_of_password";

        // Налаштовуємо поведінку моків: репозиторій повертає налаштування
        mockRepo.Setup(r => r.GetSettingsAsync())
            .ReturnsAsync(new AdminSettings { PasswordHash = dbHash });

        // Хешер підтверджує, що пароль правильний
        mockHasher.Setup(h => h.Verify(inputPassword, dbHash))
            .Returns(true);

        var useCase = new LoginAdminUseCase(mockRepo.Object, mockHasher.Object, mockLogger.Object);
        var request = new LoginAdminRequest { Password = inputPassword };

        // 2. Act (Дія)
        var result = await useCase.ExecuteAsync(request);

        // 3. Assert (Перевірка)
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
    }

    // Негативний сценарій
    [Fact]
    public async Task ExecuteAsync_IncorrectPassword_ReturnsFailureAndErrorMessage()
    {
        // 1. Arrange (Підготовка)
        var mockRepo = new Mock<IAdminCredentialsRepository>();
        var mockHasher = new Mock<IPasswordHasher>();
        var mockLogger = new Mock<ILogger<LoginAdminUseCase>>();

        string inputPassword = "WrongPassword";
        string dbHash = "hashed_version_of_password";

        mockRepo.Setup(r => r.GetSettingsAsync())
            .ReturnsAsync(new AdminSettings { PasswordHash = dbHash });

        // Хешер каже, що пароль НЕ правильний
        mockHasher.Setup(h => h.Verify(inputPassword, dbHash))
            .Returns(false);

        var useCase = new LoginAdminUseCase(mockRepo.Object, mockHasher.Object, mockLogger.Object);
        var request = new LoginAdminRequest { Password = inputPassword };

        // 2. Act (Дія)
        var result = await useCase.ExecuteAsync(request);

        // 3. Assert (Перевірка)
        Assert.False(result.IsSuccess);
        Assert.Equal("Неправильний пароль", result.ErrorMessage);
    }
