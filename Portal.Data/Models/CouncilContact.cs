using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class CouncilContact
{
    public int Id { get; set; }

    public int? LegacyId { get; set; }

    public int CouncilId { get; set; }

    public int ContactId { get; set; }

    public int AddressId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual Contact Contact { get; set; } = null!;

    public virtual Council Council { get; set; } = null!;

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual AppUser? ModifiedByUser { get; set; }
}
