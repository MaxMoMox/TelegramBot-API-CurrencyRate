using Microsoft.Extensions.Configuration;

namespace CurrencyTelegramBot.Services.Requests;

public class ConfigurationRequest
{
    private const string SettingsPath = "appsettings.json";

    public Task<string> GetApiUrl(DateTime date, string key)
    {
        var value = Configuration().GetSection(key).Value;

        if (value == null)
        {
            throw new NullReferenceException("ConfigurationRequest.GetApiUrl: GetSection has null value response.");
        }

        return Task.FromResult(Configuration().GetSection(key).Value + date.ToString("dd.MM.yyyy"));
    }

    public Task<string> GetValue(string key)
    {
        var value = Configuration().GetSection(key).Value;

        if (value == null)
        {
            throw new NullReferenceException("ConfigurationRequest.GetValue: GetSection has null value response.");
        }

        return Task.FromResult(value);
    }

    private static IConfiguration Configuration()
    {
        try
        {
            return new ConfigurationBuilder()
                .AddJsonFile(SettingsPath, optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException("ConfigurationRequest.Configuration: Settings file could not be found.");
        }
        catch (Exception e)
        {
            throw new Exception($"ConfigurationRequest.Configuration: {e.Message}");
        }
    }
}