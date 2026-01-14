using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Proposal
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int? Number { get; set; }

    public string? Suffix { get; set; }

    public int QuoteId { get; set; }

    public int ContactId { get; set; }

    public string? Invoice { get; set; }

    public DateTime? ReadyDate { get; set; }

    public DateTime? SentDate { get; set; }

    public double? TotalPrice { get; set; }
}
