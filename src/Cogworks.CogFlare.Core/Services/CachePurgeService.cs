namespace Cogworks.CogFlare.Core.Services;

public interface ICachePurgeService
{
    Task PurgeExternalCacheAsync(IEnumerable<int> ids, CancellationToken cancellationToken, string notificationLabel,
        bool isMedia = false);
}

public class CachePurgeService : ICachePurgeService
{
    private readonly ICloudFlareCachePurgeService _cloudFlareCachePurgeService;
    private readonly IUmbracoContentNodeService _umbracoContentNodeService;
    private readonly IRelationService _relationService;
    private readonly ILogService _logService;
    private readonly CogFlareSettings _cogFlareSettings;

    public CachePurgeService(
        ICloudFlareCachePurgeService cloudFlareCachePurgeService,
        IUmbracoContentNodeService umbracoContentNodeService,
        IRelationService relationService,
        ILogService logService,
        CogFlareSettings cogFlareSettings)
    {
        _cloudFlareCachePurgeService = cloudFlareCachePurgeService;
        _umbracoContentNodeService = umbracoContentNodeService;
        _relationService = relationService;
        _logService = logService;
        _cogFlareSettings = cogFlareSettings;
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
                _logService.Log($"Full purge triggered: [{id}] Key node {notificationLabel}");

                await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true);

                return;
            }

            var relatedIds = GetRelatedNodeIds(id, isMedia);

            if (relatedIds.Any(IsKeyNode))
            {
                _logService.Log($"Full purge triggered: [{id}] Node related to key node {notificationLabel}");

                await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true);

                return;
            }

            var urlsToPurge = relatedIds.Select(relatedId =>
                {
                    var url = _umbracoContentNodeService.GetContentUrlById(relatedId, isMedia,
                        _cogFlareSettings.Domain.HasValue());
                    return url.HasValue() ? $"{_cogFlareSettings.Domain}{url}" : null;
                })
                .Where(x => x is not null)
                .ToList();

            _logService.Log(
                $"Individual node(s) purge triggered: [{string.Join(",", urlsToPurge)}] {notificationLabel}");

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