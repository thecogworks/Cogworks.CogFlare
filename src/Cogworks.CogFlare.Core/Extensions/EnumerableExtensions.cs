namespace Cogworks.CogFlare.Core.Extensions;

public static class EnumerableExtensions
{
    public static bool HasAny<T>(this IEnumerable<T>? items) 
        => items is not null && items.Any();
}