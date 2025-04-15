using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PCPal.Core.Models;
using System.Diagnostics;
using System.Xml;

namespace PCPal.Core.Services;

public interface IConfigurationService
{
    Task<DisplayConfig> LoadConfigAsync();
    Task SaveConfigAsync(DisplayConfig config);
    Task<string> GetLastIconDirectoryAsync();
    Task SaveLastIconDirectoryAsync(string path);
}

public class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        _configPath = GetConfigPath();
    }

    public async Task<DisplayConfig> LoadConfigAsync()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                string json = await File.ReadAllTextAsync(_configPath);
                var config = JsonConvert.DeserializeObject<DisplayConfig>(json);
                return config ?? new DisplayConfig();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration");
        }

        return new DisplayConfig();
    }

    public async Task SaveConfigAsync(DisplayConfig config)
    {
        try
        {
            string directory = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
            await File.WriteAllTextAsync(_configPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving configuration");
        }
    }

    public async Task<string> GetLastIconDirectoryAsync()
    {
        try
        {
            var config = await LoadConfigAsync();
            return config.LastIconDirectory ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last icon directory");
            return string.Empty;
        }
    }

    public async Task SaveLastIconDirectoryAsync(string path)
    {
        try
        {
            var config = await LoadConfigAsync();
            config.LastIconDirectory = path;
            await SaveConfigAsync(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving last icon directory");
        }
    }

    private string GetConfigPath()
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PCPal");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "config.json");
    }
}