using UmbracoConstants = Umbraco.Cms.Core.Constants.PropertyEditors.Aliases;

namespace Cogworks.CogFlare.Core.ViewComponents;

public class CacheHeadersViewComponent : ViewComponent
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly CogFlareSettings _cogFlareSettings;

    public CacheHeadersViewComponent(IUmbracoContextAccessor umbracoContextAccessor, CogFlareSettings cogFlareSettings)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _cogFlareSettings = cogFlareSettings ?? new CogFlareSettings();
    }

    public IViewComponentResult Invoke()
    {
        if (!_cogFlareSettings.IsEnabled)
        {
            return Content(string.Empty);
        }

        if (IsCacheable())
        {
            if (_cogFlareSettings.CacheTimeEdge.HasValue())
            {
                HttpContext.Response.Headers["Edge-Control"] = $"cache-maxage={_cogFlareSettings.CacheTimeEdge}s";
            }

            if (_cogFlareSettings.CacheTime.HasValue())
            {
                HttpContext.Response.Headers["Cache-Control"] = _cogFlareSettings.CacheTime.Equals("0")
                    ? "no-cache, no-store, must-revalidate"
                    : $"public, max-age={_cogFlareSettings.CacheTime}";
            }
        }

        return Content(string.Empty);
    }

    private bool IsCacheable()
    {
        var blockAliases = _cogFlareSettings.BlockAliases.Split(SeparatorConstants.Comma);

        if (!blockAliases.HasAny())
        {
            return true;
        }

        var currentPage = _umbracoContextAccessor?.GetRequiredUmbracoContext()?.PublishedRequest?.PublishedContent;

        if (currentPage == null)
        {
            return true;
        }

        var blockList = currentPage.Properties
            .Where(property => property.PropertyType.DataType.EditorAlias == UmbracoConstants.BlockList &&
                               property.HasValue())
            .SelectMany(content =>
                content.Value(null!) as IEnumerable<BlockListItem> ?? Enumerable.Empty<BlockListItem>());

        return blockList.All(block => !blockAliases.Contains(block.Content.ContentType.Alias));
    }
}