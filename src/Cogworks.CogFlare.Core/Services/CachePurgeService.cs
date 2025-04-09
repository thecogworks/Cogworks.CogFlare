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
    private readonly ICogFlareLogService _logService;
    private readonly CogFlareSettings _cogFlareSettings;

    public CachePurgeService(
        ICloudFlareCachePurgeService cloudFlareCachePurgeService,
        IUmbracoContentNodeService umbracoContentNodeService,
        IRelationService relationService,
        ICogFlareLogService logService,
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

        var keyParentNodeRelatedIds = GetKeyParentNodeRelatedIds(nodeId);

        if (keyParentNodeRelatedIds.HasAny())
        {
            relatedIds.AddRange(keyParentNodeRelatedIds);
        }

        return relatedIds;
    }

    private IEnumerable<int> GetKeyParentNodeRelatedIds(int id)
    {
        var keyParentNodes = new List<int>();
        var ancestorsIds = _umbracoContentNodeService.GetAncestorsIdsById(id);

        foreach (var keyParentNode in _cogFlareSettings.KeyParentNodes?.GetNodeIds())
        {
            var relatedParentIds = ancestorsIds
                ?.Where(x => x == keyParentNode)
                ?.ToList();

            if (relatedParentIds.HasAny())
            {
                keyParentNodes.AddRange(relatedParentIds);
            }
        }

        return keyParentNodes;
    }

    private bool IsKeyNode(int nodeId)
    {
        return _cogFlareSettings
            .KeyNodes.GetNodeIds()
            .Contains(nodeId);
    }
}