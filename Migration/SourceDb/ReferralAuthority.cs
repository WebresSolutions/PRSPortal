using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class ReferralAuthority
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Suburb { get; set; }

    public string State { get; set; } = null!;

    public string? Postcode { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Fax { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }
}
