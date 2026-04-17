using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Invoice header linked to contact and/or job with total price and audit fields.
/// </summary>
public partial class Invoice
{
    public int Id { get; set; }

    public int? LegacyId { get; set; }

    public int? ContactId { get; set; }

    public int? JobId { get; set; }

    public decimal TotalPrice { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job? Job { get; set; }

    public virtual AppUser? ModifiedByUser { get; set; }
}
