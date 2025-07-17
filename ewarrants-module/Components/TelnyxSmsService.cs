using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class TelnyxSmsService : IDisposable
{
    private readonly string _apiKey;
    private readonly string _fromNumber;
    private readonly HttpClient _httpClient;

    public TelnyxSmsService(string apiKey, string fromNumber)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _fromNumber = fromNumber ?? throw new ArgumentNullException(nameof(fromNumber));
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api.telnyx.com/v2/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<(bool Success, string MessageId, string ErrorMessage)> SendSmsAsync(string toNumber, string message)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(toNumber))
                return (false, null, "Recipient phone number is required.");
            if (string.IsNullOrWhiteSpace(message))
                return (false, null, "Message content is required.");

            // Prepare the request payload
            var payload = new
            {
                from = _fromNumber,
                to = toNumber,
                text = message
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            // Send POST request to Telnyx API
            var response = await _httpClient.PostAsync("messages", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string messageId = responseObject?.data?.id?.ToString();
                return (true, messageId, null);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, null, $"Failed to send SMS to {toNumber}: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            return (false, null, $"Error sending SMS to {toNumber}: {ex.Message}");
        }
    }

    public async Task<(bool AllSuccessful, Dictionary<string, (bool Success, string MessageId, string ErrorMessage)> Results)> SendSmsToMultipleAsync(IEnumerable<string> toNumbers, string message)
    {
        var results = new Dictionary<string, (bool Success, string MessageId, string ErrorMessage)>();
        bool allSuccessful = true;

        if (toNumbers == null || !toNumbers.Any())
        {
            return (false, new Dictionary<string, (bool, string, string)> { { "", (false, null, "No phone numbers provided.") } });
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            foreach (var number in toNumbers)
            {
                results[number] = (false, null, "Message content is required.");
            }
            return (false, results);
        }

        foreach (var toNumber in toNumbers)
        {
            if (string.IsNullOrWhiteSpace(toNumber))
            {
                results[toNumber] = (false, null, "Recipient phone number is required.");
                allSuccessful = false;
                continue;
            }

            var result = await SendSmsAsync(toNumber, message);
            results[toNumber] = result;
            if (!result.Success)
            {
                allSuccessful = false;
            }
        }

        return (allSuccessful, results);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
