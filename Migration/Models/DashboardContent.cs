using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Holds widgets defined in the front end.
/// </summary>
public partial class DashboardContent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<DashboardItem> DashboardItems { get; set; } = new List<DashboardItem>();
}
