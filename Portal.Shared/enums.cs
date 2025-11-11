using System.ComponentModel;
using System.Reflection;

namespace Portal.Shared;

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
