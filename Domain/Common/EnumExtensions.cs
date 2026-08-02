using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Domain.Common;

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Enum, string> _displayNames = new();

    public static string GetDisplayName(this Enum value)
    {
        return _displayNames.GetOrAdd(value, v =>
        {
            var field = v.GetType().GetField(v.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? v.ToString();
        });
    }
}
