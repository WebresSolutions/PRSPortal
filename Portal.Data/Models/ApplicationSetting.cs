using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class ApplicationSetting
{
    public int Id { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
}
