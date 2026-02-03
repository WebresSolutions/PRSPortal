using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class Dashboard
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsDefault { get; set; }

    public int DashboardY { get; set; }

    public int DashboardX { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual ICollection<DashboardItem> DashboardItems { get; set; } = new List<DashboardItem>();

    public virtual AppUser User { get; set; } = null!;
}
