namespace Cogworks.CogFlare.Core.Services;

public interface ICachePurgeService
{
    Task PurgeExternalCacheAsync(IEnumerable<int> ids, CancellationToken cancellationToken, string notificationLabel, bool isMedia = false);
}

public class CachePurgeService : ICachePurgeService
{
    private readonly ICloudFlareCachePurgeService _cloudFlareCachePurgeService;
    private readonly IUmbracoContentNodeService _umbracoContentNodeService;
    private readonly IRelationService _relationService;
    private readonly CogFlareSettings _cogFlareSettings;
    private readonly ILogger<ExternalCachePurgeComponent> _logger;

    public CachePurgeService(
        ICloudFlareCachePurgeService cloudFlareCachePurgeService,
        IUmbracoContentNodeService umbracoContentNodeService,
        IRelationService relationService,
        CogFlareSettings cogFlareSettings,
        ILogger<ExternalCachePurgeComponent> logger)
    {
        _cloudFlareCachePurgeService = cloudFlareCachePurgeService;
        _umbracoContentNodeService = umbracoContentNodeService;
        _relationService = relationService;
        _cogFlareSettings = cogFlareSettings;
        _logger = logger;
    }

    public async Task PurgeExternalCacheAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        string notificationLabel,
        bool isMedia = false)
    {
        foreach (var id in ids)
        {
            if (IsKeyNode(id))
            {
                if (_cogFlareSettings.EnableLogging)
                {
                    _logger.LogInformation($"Full purge triggered: [{id}] Key node {notificationLabel}");
                }

                await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true);

                return;
            }

            var relatedIds = GetRelatedNodeIds(id, isMedia);

            if (relatedIds.Any(IsKeyNode))
            {
                if (_cogFlareSettings.EnableLogging)
                {
                    _logger.LogInformation($"Full purge triggered: [{id}] Node related to key node {notificationLabel}");
                }

                await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true);

                return;
            }

            var urlsToPurge = relatedIds.Select(relateId =>
            {
              var url = _umbracoContentNodeService.GetContentUrlById(relateId, isMedia, _cogFlareSettings.Domain.HasValue());
              return !url.HasValue() ? null : $"{_cogFlareSettings.Domain}{url}";
            })
            .Where(x => x != null)
            .ToList();

            if (_cogFlareSettings.EnableLogging)
            {
                _logger.LogInformation($"Individual node(s) purge triggered: [{string.Join(",", urlsToPurge)}] {notificationLabel}");
            }

            await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, false, urlsToPurge);
        }
    }

    private IEnumerable<int> GetRelatedNodeIds(int nodeId, bool isMedia)
    {
        var relationshipType = isMedia
            ? RelationTypes.RelatedMediaAlias
            : RelationTypes.RelatedDocumentAlias;

        var relatedIds = _relationService
            .GetByChildId(nodeId)
            .Where(x => x.RelationType.Alias == relationshipType)
            .Select(x => x.ParentId)
            .Append(nodeId)
        .ToList();

        return relatedIds;
    }

    private bool IsKeyNode(int nodeId)
    {
        return _cogFlareSettings
            .GetKeyNodes()
            .Contains(nodeId);
    }
}