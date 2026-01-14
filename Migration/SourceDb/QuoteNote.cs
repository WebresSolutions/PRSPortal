using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class QuoteNote
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int QuoteId { get; set; }

    public string Note { get; set; } = null!;

    public bool ActionRequired { get; set; }

    public bool ActionTaken { get; set; }

    public int? AssignedTo { get; set; }
}
