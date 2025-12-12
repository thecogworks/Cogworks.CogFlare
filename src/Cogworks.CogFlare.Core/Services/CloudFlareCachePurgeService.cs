using System.Text;

namespace Cogworks.CogFlare.Core.Services;

public interface ICloudFlareCachePurgeService
{
    Task<bool> PurgeCacheAsync(CancellationToken cancellationToken, bool purgeAll = false,
        IEnumerable<string> urls = null);
}

public class CloudFlareCachePurgeService : ICloudFlareCachePurgeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CogFlareSettings _cogFlareSettings;
    private readonly ICogFlareLogService _logService;

    public CloudFlareCachePurgeService(
        CogFlareSettings cogFlareSettings,
        ICogFlareLogService logService,
        IHttpClientFactory httpClientFactory)
    {
        _cogFlareSettings = cogFlareSettings;
        _logService = logService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> PurgeCacheAsync(CancellationToken cancellationToken, bool purgeEverything = false,
        IEnumerable<string> urls = null)
    {
        if (!_cogFlareSettings.IsValid || (!urls.HasAny() && !purgeEverything))
        {
            _logService.Log("Failed to complete CloudFlare purge: CogFlare Settings are invalid");

            return false;
        }

        var purgeSettings = new PurgeSettings
        {
            PurgeEverything = purgeEverything,
            Files = purgeEverything ? null : urls
        };

        return await SendPurgeRequest(purgeSettings, cancellationToken);
    }

    private async Task<bool> SendPurgeRequest(PurgeSettings purgeSettings, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        var request = new HttpRequestMessage(HttpMethod.Post, _cogFlareSettings.Endpoint);

        if (_cogFlareSettings.AuthenticationMethod.ToLowerInvariant() == "email")
        {
            request.Headers.Add("X-Auth-Email", _cogFlareSettings.Email);
            request.Headers.Add("X-Auth-Key", _cogFlareSettings.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(purgeSettings));
        }
        else
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cogFlareSettings.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(purgeSettings), Encoding.UTF8, "application/json");
        }

        try
        {
            var response = await client.SendAsync(request, cancellationToken);

            _logService.Log($"CloudFlare response for purging: {response.ReasonPhrase}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception exception)
        {
            _logService.Log($"Failed to complete CloudFlare purge: [{exception.Message}]");

            return false;
        }
    }
}