namespace Cogworks.CogFlare.Core.Extensions;

public static class StringExtensions
{
    public static bool HasValue(this string? input) 
        => !string.IsNullOrWhiteSpace(input);

    public static IEnumerable<int> GetNodeIds(this string nodes)
    {
        var keyNodes = new List<int>();

        foreach (var keyNode in nodes?.Split(SeparatorConstants.Comma))
        {
            if (int.TryParse(keyNode, out var id))
            {
                keyNodes.Add(id);
            }
        }

        return keyNodes;
    }

    public static IEnumerable<Guid> GetNodeGuids(this string nodes)
    {
        var keyGuids = new List<Guid>();

        foreach (var keyGuid in nodes?.Split(SeparatorConstants.Comma) ?? [])
        {
            if (Guid.TryParse(keyGuid.Trim(), out var guid))
            {
                keyGuids.Add(guid);
            }
        }

        return keyGuids;
    }
}