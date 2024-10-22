namespace Cogworks.CogFlare.Core.Models;

public record CogFlareSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string KeyNodes { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public bool EnableLogging = true;
    public string BlockAliases { get; init; } = string.Empty;
    public bool IsValid => ApiKey.HasValue() && Email.HasValue() && Endpoint.HasValue();

    public IEnumerable<int> GetKeyNodes()
    {
        var keyNodes = new List<int>();

        foreach (var keyNode in KeyNodes?.Split(SeparatorConstants.Comma))
        {
            if (int.TryParse(keyNode, out var id))
            {
                keyNodes.Add(id);
            }
        }

        return keyNodes;
    }
}