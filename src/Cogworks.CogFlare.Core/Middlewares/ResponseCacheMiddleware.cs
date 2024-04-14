using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;

namespace Cogworks.CogFlare.Core.Middlewares
{
    public class ResponseCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IDomainService _domainService;
        private readonly CogFlareSettings _cogFlareSettings;

        public ResponseCacheMiddleware(RequestDelegate next, IUmbracoContextFactory umbracoContextFactory,
            IDomainService domainService, CogFlareSettings cogFlareSettings)
        {
            _next = next;
            _umbracoContextFactory = umbracoContextFactory;
            _domainService = domainService;
            _cogFlareSettings = cogFlareSettings;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestHost = context.Request.Host.Value.ToLower();
            var UrlAddress = context.Request.Path.Value?.ToLower();
            var domains = _domainService.GetAll(true) as IList<IDomain> ?? _domainService.GetAll(true).ToList();
            var domainRoutePrefixId = string.Empty;
            var domainLanguageIsoCode = string.Empty;

            if (domains.Any())
            {
                var domain = domains.FirstOrDefault(currentDomain =>
                    requestHost.StartsWith(currentDomain.DomainName.ToLower()));

                if (domain != null)
                {
                    domainRoutePrefixId = domain.RootContentId.ToString();
                    domainLanguageIsoCode = domain.LanguageIsoCode;
                }
            }

            using (var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var contentCache = umbracoContextReference.UmbracoContext.Content;
                var currentNode = contentCache?.GetByRoute(domainRoutePrefixId + UrlAddress, false,
                    culture: domainLanguageIsoCode);

                if (currentNode != null)
                {
                    var disableCloudFlareCache = currentNode.HasProperty(ApplicationConstants.DisableCloudFlareCache) &&
                                                 currentNode.Value<bool>(ApplicationConstants.DisableCloudFlareCache);

                    var blockListPropertyAlias = _cogFlareSettings.BlockListPropertyAlias;
                    var formBlockAlias = _cogFlareSettings.FormBlockAlias;

                    var blockList = currentNode.HasProperty(blockListPropertyAlias)
                        ? currentNode.Value<BlockListModel>(blockListPropertyAlias)
                        : null;

                    var formBlock = blockList?.FirstOrDefault(x => x.Content.ContentType.Alias == formBlockAlias);

                    if (disableCloudFlareCache || formBlock != null)
                    {
                        context.Response.GetTypedHeaders().CacheControl =
                            new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                            {
                                NoCache = true
                            };
                    }
                }
            }

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }
}

public static class ResponseCultureMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseCacheMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseCacheMiddleware>();
    }
}