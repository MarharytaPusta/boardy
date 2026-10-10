namespace Boardy.Domain.Authentication;

public interface IAdminCredentialsRepository
{
    Task<AdminSettings> GetSettingsAsync();
    Task SaveSettingAsync(AdminSettings settings);
}
