using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class TechnicalContact
{
    public int Id { get; set; }

    public int TypeId { get; set; }

    public int ContactId { get; set; }

    public int JobId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual Job Job { get; set; } = null!;

    public virtual TechnicalContactType Type { get; set; } = null!;
}
