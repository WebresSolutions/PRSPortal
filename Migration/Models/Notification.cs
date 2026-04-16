using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// In-app or queued notifications per user with JSON payload and type classification.
/// </summary>
public partial class Notification
{
    public int Id { get; set; }

    public int NotificationTypeId { get; set; }

    public int UserId { get; set; }

    public bool IsActive { get; set; }

    public string Payload { get; set; } = null!;

    public virtual NotificationType NotificationType { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
