using System.Net.Http.Headers;

namespace CurrencyTelegramBot.Services.Requests;

public class ApiJsonRequest
{
    public Task<string> GetStringResponse(string address)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.GetAsync(address).Result;
            response.EnsureSuccessStatusCode();

            return Task.FromResult(response.Content.ReadAsStringAsync().Result);
        }
        catch (Exception e)
        {
            throw new Exception($"ApiJsonRequest.GetStringResponse: {e.Message}");
        }
    }
} 