using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace SharpAutomation.Helpers
{
    public sealed class ConfigurationHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public ConfigurationHelper(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = Log.ForContext<ConfigurationHelper>();
        }

        public string GetConfig(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.Error("Configuration key is null or whitespace in {MethodName}.", nameof(GetConfig));
                throw new ArgumentException($"Configuration Key cannot be null or empty: {nameof(key)}");
            }

            var value = _configuration[key];

            if (string.IsNullOrEmpty(value))
            {
                _logger.Error("Configuration key '{Key}' was not found or is empty in {MethodName}.", key, nameof(GetConfig));
                throw new KeyNotFoundException($"Configuration key '{key}' not found.");
            }

            _logger.Information("Successfully retrieved configuration value for key '{Key}'.", key);
            return value;
        }

        public void CreateAppsettings(string filePath, Dictionary<string, string> configs)
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(configs, jsonOptions);
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Unable to load or locate file from location : {filePath}");

                File.WriteAllText(filePath, jsonString);
                _logger.Information("Appsettings file created at {FilePath}.", filePath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating appsettings file at {FilePath}.", filePath);
                throw;
            }
        }

        public void CreateEnvFile(string path, Dictionary<string, string> configs)
        {
            try
            {
                using var writer = new StreamWriter(path);
                foreach (var config in configs)
                {
                    writer.WriteLine($"{config.Key}={config.Value}");
                }
                _logger.Information(".env file created at {Path}.", path);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating .env file at {Path}.", path);
                throw;
            }
        }
    }
}
