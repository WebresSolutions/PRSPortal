using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Portal.Server.Helpers;

/// <summary>
/// Normalizes string values by trimming whitespace and truncating to database limits.
/// Use on DTOs before saving to ensure text is clean and within column limits.
/// </summary>
public static class StringNormalizer
{
    /// <summary>
    /// Trims whitespace and optionally truncates to max length.
    /// </summary>
    /// <param name="value">The string to normalize (can be null).</param>
    /// <param name="maxLength">Optional max length; if null, only trim is applied.</param>
    /// <returns>Trimmed string, truncated if over maxLength, or null if input was null.</returns>
    public static string? TrimAndTruncate(string? value, int? maxLength = null)
    {
        if (value is null)
            return null;
        string trimmed = value.Trim();
        if (maxLength.HasValue && trimmed.Length > maxLength.Value)
            return trimmed[..maxLength.Value];
        return trimmed;
    }

    public static string TrimAndTruncateNotNull(string value, int? maxLength = null)
    {
        string trimmed = value.Trim();
        if (maxLength.HasValue && trimmed.Length > maxLength.Value)
            return trimmed[..maxLength.Value];
        return trimmed;
    }

    /// <summary>
    /// Recursively normalizes all string properties on the given object:
    /// trims whitespace and truncates using [MaxLength] when present.
    /// Processes nested DTOs (e.g. Address). Skips collections and primitives.
    /// </summary>
    /// <param name="obj">The DTO or object to normalize (can be null).</param>
    public static void Normalize(object? obj)
    {
        if (obj is null)
            return;

        HashSet<object> x = new(ReferenceEqualityComparer.Instance);
        NormalizeInternal(obj, x);
    }

    private static void NormalizeInternal(object obj, HashSet<object> visited)
    {
        if (obj is null || !visited.Add(obj))
            return;

        Type type = obj.GetType();

        // Only process classes (DTOs); skip strings, value types, and common framework types
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) || type == typeof(TimeOnly) || type == typeof(DateOnly) ||
            type == typeof(Guid) || type == typeof(decimal) || type.IsValueType)
            return;

        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            if (prop.PropertyType == typeof(string))
            {
                object? current = prop.GetValue(obj);
                string? str = current as string;
                int? maxLength = prop.GetCustomAttribute<MaxLengthAttribute>()?.Length;
                string? normalized = TrimAndTruncate(str, maxLength);
                if (normalized != str)
                    prop.SetValue(obj, normalized);
                continue;
            }

            // Recurse into nested DTOs (class types, not collections)
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(Type) &&
                !prop.PropertyType.IsArray && !IsCollectionType(prop.PropertyType))
            {
                object? nested = prop.GetValue(obj);
                if (nested is not null)
                    NormalizeInternal(nested, visited);
            }
        }
    }

    private static bool IsCollectionType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return true;
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            return true;
        return false;
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
