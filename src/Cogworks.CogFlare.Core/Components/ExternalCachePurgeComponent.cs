namespace Cogworks.CogFlare.Core.Components;

public class ExternalCachePurgeComponent : 
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentPublishingNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>,
    INotificationAsyncHandler<ContentUnpublishingNotification>,
    INotificationAsyncHandler<MediaSavedNotification>
{
    private readonly ICachePurgeService _cachePurgeService;

    public ExternalCachePurgeComponent(ICachePurgeService cachePurgeService)
    {
        _cachePurgeService = cachePurgeService;
    }

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        await _cachePurgeService.PurgeExternalCacheAsync(notification.PublishedEntities.Select(x => x.Id),
            cancellationToken, 
            NotificationConstants.Published);
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        await _cachePurgeService.PurgeExternalCacheAsync(notification.DeletedEntities.Select(x => x.Id),
            cancellationToken,
            NotificationConstants.Deleted);
    }

    public async Task HandleAsync(ContentUnpublishingNotification notification, CancellationToken cancellationToken)
    {
        await _cachePurgeService.PurgeExternalCacheAsync(notification.UnpublishedEntities.Select(x => x.Id),
            cancellationToken,
            NotificationConstants.Unpublished);
    }

    public async Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken)
    {
        await _cachePurgeService.PurgeExternalCacheAsync(notification.SavedEntities.Select(x => x.Id),
            cancellationToken,
            NotificationConstants.Saved,true);
    }

    public async Task HandleAsync(ContentPublishingNotification notification, CancellationToken cancellationToken)
    {

        var contentWithChangedNames = notification.PublishedEntities
            .Where(content => content.IsPropertyDirty("Name"))
            .Select(content => content.Id)
            .ToList();

        if (!contentWithChangedNames.HasAny())
        {
            return;
        }

        await _cachePurgeService.PurgeExternalCacheAsync(contentWithChangedNames,
            cancellationToken,
            NotificationConstants.Publishing,
            singleUrlPurge:true);
    }
}