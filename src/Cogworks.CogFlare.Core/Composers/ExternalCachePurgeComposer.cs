namespace Cogworks.CogFlare.Core.Composers;

public class ExternalCachePurgeComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var cogFlareSettings = builder.Config
            .GetSection(nameof(CogFlareSettings))
            .Get<CogFlareSettings>();

        if (!cogFlareSettings?.IsEnabled ?? true)
        {
            return;
        }

        builder.Services
            .AddTransient(_ => cogFlareSettings)
            .AddHttpClient()
            .AddTransient<ICachePurgeService, CachePurgeService>()
            .AddTransient<ICloudFlareCachePurgeService, CloudFlareCachePurgeService>()
            .AddTransient<IUmbracoContentNodeService, UmbracoContentNodeService>()
            .AddTransient<ICogFlareLogService, CogFlareLogService>();

        builder
            .AddNotificationAsyncHandler<ContentPublishedNotification, ExternalCachePurgeComponent>()
            .AddNotificationAsyncHandler<ContentDeletedNotification, ExternalCachePurgeComponent>()
            .AddNotificationAsyncHandler<ContentUnpublishingNotification, ExternalCachePurgeComponent>()
            .AddNotificationAsyncHandler<MediaSavedNotification, ExternalCachePurgeComponent>();
    }
}