namespace Cogworks.CogFlare.Core.Models;

public record PurgeSettings
{
    [JsonPropertyName("purge_everything")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool PurgeEverything { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("files")]
    public IEnumerable<string> Files { get; set; }
}