namespace Cogworks.CogFlare.Core.Controllers.APIs;

public class CloudFlareCachePurgeApiController : UmbracoAuthorizedJsonController
{
    private readonly ICloudFlareCachePurgeService _cloudFlareCachePurgeService;
    private readonly IUmbracoContentNodeService _umbracoContentNodeService;
    private readonly CogFlareSettings _cogFlareSettings;

    public CloudFlareCachePurgeApiController(
        ICloudFlareCachePurgeService cloudFlareCachePurgeService,
        CogFlareSettings cogFlareSettings,
        IUmbracoContentNodeService umbracoContentNodeService)
    {
        _cloudFlareCachePurgeService = cloudFlareCachePurgeService;
        _cogFlareSettings = cogFlareSettings;
        _umbracoContentNodeService = umbracoContentNodeService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PurgeCache(CancellationToken cancellationToken)
    {
        return await _cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true)
            ? Ok(true)
            : Problem();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetSettings()
    {
        var keyNodes = _cogFlareSettings
            .KeyNodes.GetNodeIds()
            .Select(id => _umbracoContentNodeService.GetContentUrlById(id));

        var settings = _cogFlareSettings with
        {
            KeyNodes = keyNodes.HasAny()
                ? string.Join(SeparatorConstants.Comma, keyNodes)
                : string.Empty,
        };

        return Ok(settings);
    }
}