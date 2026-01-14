using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Postcode
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public string Postcode1 { get; set; } = null!;

    public string Locality { get; set; } = null!;

    public string State { get; set; } = null!;

    public string? Comments { get; set; }

    public string? DeliveryOffice { get; set; }

    public string? PresortIndicator { get; set; }

    public string? ParcelZone { get; set; }

    public string? BspNumber { get; set; }

    public string? BspName { get; set; }

    public string? Category { get; set; }
}
