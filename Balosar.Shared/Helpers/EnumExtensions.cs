using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Portal.Shared.Helpers;

/// <summary>
/// Extension methods for enum types
/// Provides utility methods for working with enums, particularly for display name retrieval
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the display name for an enum value using the DisplayAttribute
    /// Retrieves the user-friendly name defined in the Display attribute for the enum value
    /// </summary>
    /// <param name="enumValue">The enum value to get the display name for</param>
    /// <returns>The display name from the DisplayAttribute, or the enum value name if no attribute is found</returns>
    /// <exception cref="Exception">Thrown when the enum member cannot be found or accessed</exception>
    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
                        ?.GetName() ?? throw new Exception("Failed to get enum string");
    }
}