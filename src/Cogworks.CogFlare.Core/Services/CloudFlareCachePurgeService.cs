﻿namespace Cogworks.CogFlare.Core.Services;

public interface ICloudFlareCachePurgeService
{
    Task<bool> PurgeCacheAsync(CancellationToken cancellationToken, bool purgeAll = false, IEnumerable<string> urls = null);
}

public class CloudFlareCachePurgeService : ICloudFlareCachePurgeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CogFlareSettings _cogFlareSettings;
    private readonly ILogger<CloudFlareCachePurgeService> _logger;

    public CloudFlareCachePurgeService(
        CogFlareSettings cogFlareSettings,
        ILogger<CloudFlareCachePurgeService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _cogFlareSettings = cogFlareSettings;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> PurgeCacheAsync(CancellationToken cancellationToken, bool purgeEverything = false, IEnumerable<string> urls = null)
    {
        if (!_cogFlareSettings.IsValid || (!urls.HasAny() && !purgeEverything))
        {
            _logger.LogInformation("Failed to complete CloudFlare purge: CogFlare Settings are invalid");
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

        request.Headers.Add("X-Auth-Email", _cogFlareSettings.Email);
        request.Headers.Add("X-Auth-Key", _cogFlareSettings.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(purgeSettings));

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            _logger.LogInformation($"CloudFlare response for purging: {response.ReasonPhrase}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception exception)
        {
            _logger.LogInformation($"Failed to complete CloudFlare purge: [{exception.Message}]");
            return false;
        }
    }
}