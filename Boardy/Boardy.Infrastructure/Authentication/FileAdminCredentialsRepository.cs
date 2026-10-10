using Boardy.Domain.Authentication;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Boardy.Infrastructure.Authentication;

public class FileAdminCredentialsRepository : IAdminCredentialsRepository
{
    private const string FilePath = "admin_settings.json";
    private readonly ILogger<FileAdminCredentialsRepository> _logger;
    private readonly IPasswordHasher _hasher;

    public FileAdminCredentialsRepository(IPasswordHasher hasher, ILogger<FileAdminCredentialsRepository> logger)
    {
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<AdminSettings> GetSettingsAsync()
    {
        if (!File.Exists(FilePath))
        {
            _logger.LogInformation("Admin setting file not found. Apply default settings");
            var defaultSettings = new AdminSettings
            {
                PasswordHash = _hasher.Hash("admin")
            };

            await SaveSettingAsync(defaultSettings);
            return defaultSettings;
        }

        _logger.LogInformation("Read admin setting from file");
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
