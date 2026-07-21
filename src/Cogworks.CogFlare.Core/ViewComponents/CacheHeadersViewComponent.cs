using UmbracoConstants = Umbraco.Cms.Core.Constants.PropertyEditors.Aliases;

namespace Cogworks.CogFlare.Core.ViewComponents;

public class CacheHeadersViewComponent(
  IUmbracoContextAccessor umbracoContextAccessor,
  IPublishedValueFallback publishedValueFallback,
  CogFlareSettings cogFlareSettings)
  : ViewComponent
{
  private readonly CogFlareSettings _cogFlareSettings = cogFlareSettings ?? new CogFlareSettings();

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
                var isCacheTimeZero = _cogFlareSettings.CacheTime.Equals("0");

                HttpContext.Response.Headers["Cache-Control"] = isCacheTimeZero
                    ? "no-cache, no-store, must-revalidate"
                    : $"public, max-age={_cogFlareSettings.CacheTime}";

                // added unique identifier to the response headers to indicate that the page is cacheable and has a non-zero cache time
                if (!isCacheTimeZero)
                {
                    HttpContext.Response.Headers[ApplicationConstants.CustomHeaderLabel] = "1";
                }
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

        var currentPage = umbracoContextAccessor?.GetRequiredUmbracoContext()?.PublishedRequest?.PublishedContent;

        if (currentPage == null)
        {
            return true;
        }

        var blockList = currentPage.Properties
            .Where(property => property.PropertyType.DataType.EditorAlias == UmbracoConstants.BlockList &&
                               property.HasValue())
            .SelectMany(property =>
                currentPage.Value<BlockListModel>(publishedValueFallback, property.Alias) ?? Enumerable.Empty<BlockListItem>());

        return blockList.All(block => !blockAliases.Contains(block.Content.ContentType.Alias));
    }
}