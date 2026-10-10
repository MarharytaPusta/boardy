using Boardy.Domain.Authentication;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Boardy.Infrastructure.Authentication;

public class FileAdminCredentialsRepository : IAdminCredentialsRepository
{
    private const string FilePath = "admin_settings.json";
    private readonly IPasswordHasher _hasher;

    public FileAdminCredentialsRepository(IPasswordHasher hasher)
    {
        _hasher = hasher;
    }

    public async Task<AdminSettings> GetSettingsAsync()
    {
        if (!File.Exists(FilePath))
        {
            var defaultSettings = new AdminSettings
            {
                PasswordHash = _hasher.Hash("admin")
            };

            await SaveSettingAsync(defaultSettings);
            return defaultSettings;
        }

        var json = await File.ReadAllTextAsync(FilePath);
        return JsonSerializer.Deserialize<AdminSettings>(json) ?? new AdminSettings();
    }

    public async Task SaveSettingAsync(AdminSettings settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);

        await File.WriteAllTextAsync(FilePath, json);
    }
}
