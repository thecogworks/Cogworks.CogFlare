using UmbracoConstants = Umbraco.Cms.Core.Constants.PropertyEditors.Aliases;

namespace Cogworks.CogFlare.Core.ViewComponents;

public class CacheHeadersViewComponent : ViewComponent
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly CogFlareSettings _cogFlareSettings;

    public CacheHeadersViewComponent(IUmbracoContextAccessor umbracoContextAccessor, CogFlareSettings cogFlareSettings)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _cogFlareSettings = cogFlareSettings;
    }

    public IViewComponentResult Invoke()
    {
        if (IsCacheable())
        {
            HttpContext.Response.Headers["Cache-Control"] = $"public, max-age={TimeConstants.OneMonth}";
            HttpContext.Response.Headers["Edge-Control"] = $"cache-maxage={TimeConstants.OneMonth}s";
        }
        else
        {
            HttpContext.Response.Headers["Cache-Control"] = "private, no-cache, must-revalidate";
        }

        return Content(String.Empty);
    }

    private bool IsCacheable()
    {
        if(!_cogFlareSettings.FormBlockAlias.HasValue())
        {
            return true;
        }

        var currentPage = _umbracoContextAccessor?.GetRequiredUmbracoContext()?.PublishedRequest?.PublishedContent;

        if (currentPage == null)
        {
            return true;
        }

        var blockList = currentPage.Properties
            .Where(property => property.PropertyType.DataType.EditorAlias == UmbracoConstants.BlockList && property.HasValue())
            .SelectMany(content => content.Value(null!) as IEnumerable<BlockListItem> ?? Enumerable.Empty<BlockListItem>());

        return blockList.All(block => !block.Content.ContentType.Alias.InvariantEquals(_cogFlareSettings.FormBlockAlias));
    }
}