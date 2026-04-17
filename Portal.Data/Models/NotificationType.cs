using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Lookup of notification kinds for notification.notification_type_id.
/// </summary>
public partial class NotificationType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
