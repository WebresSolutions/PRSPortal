using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Timecard
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int TaskId { get; set; }

    public int? Hours { get; set; }

    public int? Minutes { get; set; }

    public string? Details { get; set; }
}
