using RelationTypes = Umbraco.Cms.Core.Constants.Conventions.RelationTypes;

namespace Cogworks.CogFlare.Core.Components;

public class ExternalCachePurgeComponent : 
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>,
    INotificationAsyncHandler<ContentUnpublishingNotification>,
    INotificationAsyncHandler<MediaSavedNotification>
{
    private readonly ICloudFlareCachePurgeService _cloudFlareCachePurgeService;
    private readonly IUmbracoContentNodeService _umbracoContentNodeService;
    private readonly IRelationService _relationService;
    private readonly CogFlareSettings _cogFlareSettings;
    private readonly ILogger<ExternalCachePurgeComponent> _logger;

    public ExternalCachePurgeComponent(
        IUmbracoContentNodeService umbracoContentNodeService,
        IRelationService relationService,
        CogFlareSettings cogFlareSettings,
        ICloudFlareCachePurgeService cloudFlareCachePurgeService,
        ILogger<ExternalCachePurgeComponent> logger)
    {
        _umbracoContentNodeService = umbracoContentNodeService;
        _relationService = relationService;
        _cloudFlareCachePurgeService = cloudFlareCachePurgeService;
        _logger = logger;
        _cogFlareSettings = cogFlareSettings;
    }

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        await PurgeExternalCacheAsync(notification.PublishedEntities.Select(x => x.Id),
            cancellationToken, 
            NotificationConstants.Published);
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        await PurgeExternalCacheAsync(notification.DeletedEntities.Select(x => x.Id),
            cancellationToken,
            NotificationConstants.Deleted);
    }

    public async Task HandleAsync(ContentUnpublishingNotification notification, CancellationToken cancellationToken)
    {
        await PurgeExternalCacheAsync(notification.UnpublishedEntities.Select(x => x.Id),
            cancellationToken,
            NotificationConstants.Unpublished);
    }

    public async Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken)
    {
        await PurgeExternalCacheAsync(notification.SavedEntities.Select(x => x.Id),
            cancellationToken,
            NotificationConstants.Saved,true);
    }

    private async Task PurgeExternalCacheAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        string notificationLabel,
        bool isMedia = false)
    {
        var urlsToPurge = new List<string>();

        foreach (var id in ids)
        {
            if (IsKeyNode(id))
            {
                _logger.LogInformation($"Full purge triggered: [{id}] Key node {notificationLabel}");
                await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true);

                return;
            }

            var relatedIds = GetRelatedNodeIds(id, isMedia);

            if (relatedIds.Any(IsKeyNode))
            {
                _logger.LogInformation($"Full purge triggered: [{id}] Node related to key node {notificationLabel}");
                await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true);

                return;
            }

            var baseDomain= _cogFlareSettings.Domain;

            foreach (var relatedId in relatedIds)
            {
                var url = $"{baseDomain}{_umbracoContentNodeService.GetContentUrlById(relatedId, isMedia, baseDomain.HasValue())}";

                if (url.HasValue())
                {
                    urlsToPurge.Add(url);
                }
            }

            _logger.LogInformation($"Individual node(s) purge triggered: [{string.Join(",", urlsToPurge)}] {notificationLabel}");
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
            .Union(new List<int> { nodeId })
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