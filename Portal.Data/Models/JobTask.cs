using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class JobTask
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public int? LegacyId { get; set; }

    public bool InvoiceRequired { get; set; }

    public DateTime ActiveDate { get; set; }

    public DateTime CompletedDate { get; set; }

    public DateTime InvoicedDate { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }
}
