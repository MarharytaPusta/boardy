using Boardy.Application.Authentication.Admin;
using Boardy.Domain.Authentication;
using Microsoft.Extensions.Logging;
using Moq;

namespace Boardy.Application.Tests.Authentication.Admin;

public class ChangeAdminPasswordUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ValidRequest_UpdatesPassword()
    {
        var repository = new Mock<IAdminCredentialsRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var logger = new Mock<ILogger<ChangeAdminPasswordUseCase>>();
        var settings = new AdminSettings { PasswordHash = "old-hash" };
        repository.Setup(repository => repository.GetSettingsAsync()).ReturnsAsync(settings);
        hasher.Setup(hasher => hasher.Verify("current-password", "old-hash")).Returns(true);
        hasher.Setup(hasher => hasher.Hash("new-password")).Returns("new-hash");
        var useCase = new ChangeAdminPasswordUseCase(repository.Object, hasher.Object, logger.Object);

        var result = await useCase.ExecuteAsync(new ChangeAdminPasswordRequest
        {
            CurrentPassword = "current-password",
            NewPassword = "new-password",
            ConfirmNewPassword = "new-password"
        });

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Equal("new-hash", settings.PasswordHash);
        repository.Verify(repository => repository.SaveSettingAsync(settings), Times.Once);
    }

    [Theory]
    [InlineData("wrong-password", "new-password", "new-password", "Неправильний поточний пароль адміністратора.")]
    [InlineData("current-password", "", "", "Новий пароль не може бути порожнім.")]
    [InlineData("current-password", "new-password", "different-password", "Нові паролі не співпадають.")]
    public async Task ExecuteAsync_InvalidRequest_ReturnsError(
        string currentPassword,
        string newPassword,
        string confirmNewPassword,
        string expectedErrorMessage)
    {
        var repository = new Mock<IAdminCredentialsRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var logger = new Mock<ILogger<ChangeAdminPasswordUseCase>>();
        repository.Setup(repository => repository.GetSettingsAsync())
            .ReturnsAsync(new AdminSettings { PasswordHash = "old-hash" });
        hasher.Setup(hasher => hasher.Verify("current-password", "old-hash")).Returns(true);
        hasher.Setup(hasher => hasher.Verify("wrong-password", "old-hash")).Returns(false);
        var useCase = new ChangeAdminPasswordUseCase(repository.Object, hasher.Object, logger.Object);

        var result = await useCase.ExecuteAsync(new ChangeAdminPasswordRequest
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = confirmNewPassword
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorMessage, result.ErrorMessage);
        repository.Verify(repository => repository.SaveSettingAsync(It.IsAny<AdminSettings>()), Times.Never);
    }
}
