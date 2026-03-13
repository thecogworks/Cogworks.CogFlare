namespace Cogworks.CogFlare.Core.Services;

public interface ICachePurgeService
{
    Task PurgeExternalCacheAsync(
        IEnumerable<int> ids, 
        CancellationToken cancellationToken, 
        string notificationLabel,
        bool singleUrlPurge = false, 
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
        bool singleUrlPurge = false,
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

            var relatedIds = singleUrlPurge 
                ? [id]
                : GetRelatedNodeIds(id, isMedia);

            if (relatedIds.Any(IsKeyNode))
            {
                _logService.Log($"Full purge triggered: [{id}] Node related to key node {notificationLabel}");

                await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true);

                return;
            }

            var urlsToPurge = GetUrlsToPurge(relatedIds, isMedia);
            
            _logService.Log($"Individual node(s) purge triggered: [{string.Join(",", urlsToPurge)}] {notificationLabel}");

            await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, false, urlsToPurge);
        }
    }

    private IEnumerable<string> GetUrlsToPurge(IEnumerable<int> nodeIds, bool isMedia)
    {
        var urlsToPurge = new List<string>();

        foreach (var relatedId in nodeIds)
        {
            urlsToPurge.Add(_umbracoContentNodeService.GetContentUrlById(relatedId, isMedia, _cogFlareSettings.Domain.HasValue()));
            urlsToPurge.AddRange(GetAllUrls(relatedId));
        }

        return urlsToPurge
            .Where(url => url.HasValue())
            .Select(url => new Uri(new Uri(_cogFlareSettings.Domain), url).ToString())
            .ToList();
    }

    private IEnumerable<string> GetAllUrls(int id)
    {
        var urlAliases = _umbracoContentNodeService
                .GetContentById(id)
                ?.Value<string>("umbracoUrlAlias");

        if(!urlAliases.HasAny())
        {
            return [];
        }

        return urlAliases
            ?.Replace(" ", string.Empty)
            ?.Split(SeparatorConstants.Comma) ?? [];
    }

    private IEnumerable<int> GetRelatedNodeIds(int nodeId, bool isMedia)
    {
        var relationshipType = isMedia
            ? RelationTypes.RelatedMediaAlias
            : RelationTypes.RelatedDocumentAlias;

        var affectedIds = new HashSet<int> { nodeId };
        affectedIds.UnionWith(GetKeyParentNodeRelatedIds(nodeId));

        var relatedIds = new HashSet<int>(affectedIds);

        foreach (var affectedId in affectedIds)
        {
            var ids = _cogFlareSettings.EnableBidirectionalRelations 
                ? _relationService
                    .GetByParentOrChildId(affectedId, relationshipType)
                    .Select(x => x.ParentId == affectedId ? x.ChildId : x.ParentId)
                : _relationService
                    .GetByChildId(affectedId)
                    .Where(x => x.RelationType.Alias == relationshipType)
                    .Select(x => x.ParentId).Append(affectedId);

            relatedIds.UnionWith(ids);
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