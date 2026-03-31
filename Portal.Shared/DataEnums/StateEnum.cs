using System.ComponentModel;

namespace Portal.Shared;

/// <summary>
/// Enumeration representing Australian states and territories
/// </summary>
public enum StateEnum
{
    /// <summary>
    /// New South Wales
    /// </summary>
    [Description("New South Wales")]
    NSW = 1,

    /// <summary>
    /// Queensland
    /// </summary>
    [Description("Queensland")]
    QLD = 2,

    /// <summary>
    /// Victoria
    /// </summary>
    [Description("Victoria")]
    VIC = 3,

    /// <summary>
    /// South Australia
    /// </summary>
    [Description("South Australia")]
    SA = 4,

    /// <summary>
    /// Tasmania
    /// </summary>
    [Description("Tasmania")]
    TAS = 5,

    /// <summary>
    /// Western Australia
    /// </summary>
    [Description("Western Australia")]
    WA = 6,

    /// <summary>
    /// Northern Territory
    /// </summary>
    [Description("Northern Territory")]
    NT = 7
}
