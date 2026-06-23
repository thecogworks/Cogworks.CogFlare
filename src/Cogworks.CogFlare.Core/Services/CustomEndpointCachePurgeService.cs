namespace Cogworks.CogFlare.Core.Services;

public class CustomEndpointCachePurgeService : ICloudFlareCachePurgeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CogFlareSettings _cogFlareSettings;
    private readonly ICogFlareLogService _logService;

    public CustomEndpointCachePurgeService(
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
        if (!_cogFlareSettings.IsValid || !_cogFlareSettings.CustomServicePurgeSettings.IsValid || (!urls.HasAny() && !purgeEverything))
        {
            _logService.Log("Failed to complete CloudFlare purge: CogFlare Settings or custom purge settings are invalid");

            return false;
        }

        var purgeSettings = new PurgeSettings
        {
            PurgeEverything = purgeEverything,
            Files = urls
        };

        return await SendPurgeRequest(purgeSettings, cancellationToken);
    }

    private async Task<bool> SendPurgeRequest(PurgeSettings purgeSettings, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

        var request = new HttpRequestMessage(HttpMethod.Post, _cogFlareSettings.CustomServicePurgeSettings.Endpoint);

        request.Headers.Add(_cogFlareSettings.CustomServicePurgeSettings.HeaderName, _cogFlareSettings.CustomServicePurgeSettings.HeaderValue);
        request.Content = new StringContent(JsonSerializer.Serialize(purgeSettings));

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync();

            _logService.Log($"API response: {responseBody}");

            var urlsLogMessage = purgeSettings.Files.HasAny()
                ? $"({purgeSettings.Files.Count()}) URLs processed -"
                : string.Empty;

            _logService.Log($"Response {response.StatusCode} = {urlsLogMessage} Custom endpoint [{_cogFlareSettings.CustomServicePurgeSettings.Endpoint}] response for purging: {response.ReasonPhrase}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception exception)
        {
            _logService.Log($"Failed to complete custom endpoint purge: [{exception.Message}]");

            return false;
        }
    }
}