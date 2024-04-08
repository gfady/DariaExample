using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CustomerNameAgent.Data.Common;

public class AgentConfigProvider : IAgentConfigProvider
{
    private readonly ILogger _logger;
    private readonly AgentSettings _agentSettings;
    private readonly JsonSerializerSettings _jsonConfigSettings;
    
    // Save previous version to avoid over-reading file
    private AgentConfig? _config;

    public AgentConfigProvider(ILogger logger, AgentSettings agentSettings)
    {
        _logger = logger;
        _agentSettings = agentSettings;

        _jsonConfigSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH.mm" } }
        };
    }

    public async Task<AgentConfig?> GetConfigAsync()
    {
        if (_config != null)
        {
            return _config;
        }

        AgentConfig? config = null;
        try
        {
            if (File.Exists(_agentSettings.SettingsFilePath))
            {
                var configText = await File.ReadAllTextAsync(_agentSettings.SettingsFilePath);
                config = JsonConvert.DeserializeObject<AgentConfig>(configText, _jsonConfigSettings);
            }
            else
            {
                config = new AgentConfig()
                {
                    LastCollectUpdate = DateTime.MinValue
                };
                
                await SaveConfigAsync(config);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error retrieving config file: {exception.Message}");
        }

        return config;
    }

    public async Task SaveConfigAsync(AgentConfig newAgentConfig)
    {
        try
        {
            if (!Directory.Exists(_agentSettings.ApplicationDataPath))
            {
                Directory.CreateDirectory(_agentSettings.ApplicationDataPath);
            }
            
            var configJson = JsonConvert.SerializeObject(newAgentConfig, _jsonConfigSettings);
            await File.WriteAllTextAsync(_agentSettings.SettingsFilePath, configJson);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error saving config file: {exception.Message}");
        }
        finally
        {
            // Reset previous config (it's dirty) 
            _config = null;
        }
    }
}