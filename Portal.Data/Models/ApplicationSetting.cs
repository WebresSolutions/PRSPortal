using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Key-value application configuration; value is JSON for flexible structured settings.
/// </summary>
public partial class ApplicationSetting
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedAt { get; set; }
}
