using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class DashboardItem
{
    public int Id { get; set; }

    public int DashboardId { get; set; }

    public int ContentId { get; set; }

    public int PositionX { get; set; }

    public int PositionY { get; set; }

    public int Colspan { get; set; }

    public int Rowspan { get; set; }

    public string? CustomTitle { get; set; }

    public string? Settings { get; set; }

    public bool IsHidden { get; set; }

    public virtual DashboardContent Content { get; set; } = null!;

    public virtual Dashboard Dashboard { get; set; } = null!;
}
