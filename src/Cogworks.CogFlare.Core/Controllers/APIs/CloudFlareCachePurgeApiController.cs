namespace Cogworks.CogFlare.Core.Controllers.APIs;

[VersionedApiBackOfficeRoute("cogflare")]
[ApiExplorerSettings(GroupName = "CogFlare")]
public class CloudFlareCachePurgeApiController(
    ICloudFlareCachePurgeService cloudFlareCachePurgeService,
    CogFlareSettings cogFlareSettings,
    IUmbracoContentNodeService umbracoContentNodeService) : ManagementApiControllerBase
{
    [HttpGet("purge-cache")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PurgeCache(CancellationToken cancellationToken)
    {
        return await cloudFlareCachePurgeService.PurgeCacheAsync(cancellationToken, true)
            ? Ok(true)
            : Problem();
    }

    [HttpGet("settings")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CogFlareSettings))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetSettings()
    {
        var keyNodes = cogFlareSettings
            .KeyNodes.GetNodeIds()
            .Select(id => umbracoContentNodeService.GetContentUrlById(id));

        var settings = cogFlareSettings with
        {
            KeyNodes = keyNodes.HasAny()
                ? string.Join(SeparatorConstants.Comma, keyNodes)
                : string.Empty,
        };

        return Ok(settings);
    }
}