using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class TechnicalContact
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int JobId { get; set; }

    public string Role { get; set; } = null!;

    public int ContactId { get; set; }
}
