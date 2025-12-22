using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// Council information
/// </summary>
public partial class Council
{
    public int Id { get; set; }

    public int? AddressId { get; set; }

    public int? LegacyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Address? Address { get; set; }

    public virtual ICollection<CouncilContact> CouncilContacts { get; set; } = new List<CouncilContact>();

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual AppUser? ModifiedByUser { get; set; }
}
