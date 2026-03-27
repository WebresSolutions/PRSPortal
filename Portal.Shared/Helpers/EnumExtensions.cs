using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Portal.Shared.Helpers;

/// <summary>
/// Extension methods for StateEnum
/// Provides utility methods for working with state enumerations
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the full name of the state from its Description attribute
    /// </summary>
    /// <param name="state">The state enumeration value</param>
    /// <returns>The full state name, or the enum value name if no description is found</returns>
    public static string? GetFullName(this StateEnum state)
    {
        FieldInfo? field = state.GetType().GetField(state.ToString());
        DescriptionAttribute? attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? state.ToString();
    }

    /// <summary>
    /// Gets the abbreviation for the state (e.g., "NSW", "VIC")
    /// </summary>
    /// <param name="state">The state enumeration value</param>
    /// <returns>The state abbreviation as a string</returns>
    public static string GetAbbreviation(this StateEnum state) => state.ToString();

    /// <summary>
    /// Converts a state abbreviation string to a StateEnum value
    /// </summary>
    /// <param name="abbreviation">The state abbreviation (e.g., "NSW", "VIC")</param>
    /// <returns>The corresponding StateEnum value, or null if parsing fails</returns>
    public static StateEnum? FromAbbreviation(string? abbreviation) => Enum.TryParse<StateEnum>(abbreviation, true, out StateEnum state) ? state : null;

    /// <summary>
    /// Converts a state ID to a StateEnum value
    /// </summary>
    /// <param name="id">The numeric state identifier</param>
    /// <returns>The corresponding StateEnum value, or null if the ID is not valid</returns>
    public static StateEnum? FromId(int id) => Enum.IsDefined(typeof(StateEnum), id) ? (StateEnum)id : null;
    /// <summary>
    /// Gets the display name for an enum value using the DisplayAttribute
    /// Retrieves the user-friendly name defined in the Display attribute for the enum value
    /// </summary>
    /// <param name="enumValue">The enum value to get the display name for</param>
    /// <returns>The display name from the DisplayAttribute, or the enum value name if no attribute is found</returns>
    /// <exception cref="Exception">Thrown when the enum member cannot be found or accessed</exception>
    public static string GetDisplayName(this Enum enumValue)
    {

        string? res = enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
                        ?.GetName() ?? throw new Exception("Failed to get enum string");

        return res;
    }
}
