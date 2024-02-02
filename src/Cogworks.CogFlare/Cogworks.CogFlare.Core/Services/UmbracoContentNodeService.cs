namespace Cogworks.CogFlare.Core.Services;

public interface IUmbracoContentNodeService
{
    string? GetContentUrlById(int id, bool isMedia = false, bool isRelativeUrl = false);
}

public class UmbracoContentNodeService : IUmbracoContentNodeService
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public UmbracoContentNodeService(IUmbracoContextFactory umbracoContextFactory, IPublishedUrlProvider publishedUrlProvider)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public string? GetContentUrlById(int id, bool isMedia = false, bool isRelativeUrl = false)
    {
        var urlMode = isRelativeUrl
            ? UrlMode.Relative
            : UrlMode.Absolute;

        return isMedia 
            ? GetMediaById(id)?.Url(_publishedUrlProvider, mode: urlMode)
            : GetContentById(id)?.Url(_publishedUrlProvider, mode: urlMode);
    }

    private IPublishedContent? GetContentById(int id)
    {
        using var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

        var contentCache = umbracoContextReference.UmbracoContext.Content;
        var content = contentCache!.GetById(id);

        return content ?? default;
    }

    private IPublishedContent? GetMediaById(int id)
    {
        using var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

        var mediaCache = umbracoContextReference.UmbracoContext.Media;
        var media = mediaCache!.GetById(id);

        return media ?? default;
    }
}