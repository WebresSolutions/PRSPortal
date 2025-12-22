using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Migration;

public enum SortDirectionEnum
{
    [Display(Name = "asc")]
    Asc = 1,

    [Display(Name = "desc")]
    Desc = 2,
}

public enum StateEnum
{
    [Description("New South Wales")]
    NSW = 1,

    [Description("Queensland")]
    QLD = 2,

    [Description("Victoria")]
    VIC = 3,

    [Description("South Australia")]
    SA = 4,

    [Description("Tasmania")]
    TAS = 5,

    [Description("Western Australia")]
    WA = 6,

    [Description("Northern Territory")]
    NT = 7
}

public enum JobTypeEnum
{
    [Display(Name = "Construction")]
    Construction = 1,
    [Display(Name = "Surveying")]
    Surveying = 2,
}

// Helper extension methods
public static class StateExtensions
{
    public static string? GetFullName(this StateEnum state)
    {
        FieldInfo? field = state.GetType().GetField(state.ToString());
        DescriptionAttribute? attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? state.ToString();
    }

    public static string GetAbbreviation(this StateEnum state) => state.ToString();

    public static StateEnum? FromAbbreviation(string? abbreviation) => Enum.TryParse<StateEnum>(abbreviation, true, out StateEnum state) ? state : null;

    public static StateEnum? FromId(int id) => Enum.IsDefined(typeof(StateEnum), id) ? (StateEnum)id : null;
}

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