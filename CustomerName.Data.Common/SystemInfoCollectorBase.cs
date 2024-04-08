using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CustomerNameAgent.Data.Common;

public abstract class SystemInfoCollectorBase : ISystemInfoCollectorBase
{
    protected readonly ILogger Logger;
    protected readonly AgentSettings AgentSettings;
    private readonly IAgentConfigProvider _agentConfigProvider;
    private readonly JsonSerializerSettings _jsonSettings;

    protected SystemInfoCollectorBase(ILogger logger, AgentSettings agentSettings,
        IAgentConfigProvider agentConfigProvider)
    {
        Logger = logger;
        AgentSettings = agentSettings;
        _agentConfigProvider = agentConfigProvider;

        _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
        };
    }

    public async Task CollectIfNeededAsync()
    {
        var config = await _agentConfigProvider.GetConfigAsync();
        if (config?.LastCollectUpdate == null)
        {
            return;
        }

        if ((DateTime.UtcNow - config.LastCollectUpdate.Value).TotalDays > 1)
        {
            var systemInfo = await GetSystemInfoAsync();
            await SaveReportAsync(systemInfo);
            await SendSystemInfoAsync(systemInfo);
            config.LastCollectUpdate = DateTime.UtcNow;
            await _agentConfigProvider.SaveConfigAsync(config);
        }
        else
        {
            Logger.LogInformation(
                $"No new data is needed, as the previous update occurred at [{config?.LastCollectUpdate}].");
        }
    }

    protected abstract Task<AgentInformation> GetSystemInfoAsync();

    private async Task SaveReportAsync(AgentInformation agentInformation)
    {
        try
        {
            if (!Directory.Exists(AgentSettings.ApplicationDataPath))
            {
                Directory.CreateDirectory(AgentSettings.ApplicationDataPath);
            }

            var infoText = JsonConvert.SerializeObject(agentInformation, _jsonSettings);
            await File.WriteAllTextAsync(
                Path.Combine(AgentSettings.ApplicationDataPath, GetReportFileName(agentInformation.CreationDate)),
                infoText);
        }
        catch (Exception exception)
        {
            Logger.LogError($"An error occurred while saving the system information report..: {exception.Message}");
        }
    }

    private async Task SendSystemInfoAsync(AgentInformation agentInformation)
    {
        try
        {
            var jsonData = JsonConvert.SerializeObject(agentInformation, _jsonSettings);

            using var httpClient = new HttpClient();
            using var formData = new MultipartFormDataContent();

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonData));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            formData.Add(content, "file", $"{GetReportFileName(agentInformation.CreationDate)}");

            var secret = HashHelper.GetSecurityString(agentInformation.CreationDate, AgentSettings.AgentConfigurations.Secret);
            httpClient.DefaultRequestHeaders.Add("SecurityHash", secret);

            var response = await httpClient.PostAsync(AgentSettings.AgentConfigurations.SendUrl, formData);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Logger.LogError($"The submission of information to the remote server was successful.");
            }
            else
            {
                Logger.LogError(
                    $"An error occurred while sending information to the remote server.: {response.StatusCode} + {responseContent}");
            }
        }
        catch (Exception exception)
        {
            Logger.LogError($"An error occurred while sending information to the remote server.: {exception.Message}");
        }
    }

    private string GetReportFileName(DateTime reportDate)
    {
        return $"InfoReport_{reportDate:yyyy-dd-M_HH-mm}.json";
    }
}