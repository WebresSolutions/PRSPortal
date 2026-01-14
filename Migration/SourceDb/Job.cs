using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Job
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int ContactId { get; set; }

    public string? EndCustomer { get; set; }

    public string Address { get; set; } = null!;

    public string? Suburb { get; set; }

    public string? State { get; set; }

    public string? Postcode { get; set; }

    public int? CouncilId { get; set; }

    public string? MapReference { get; set; }

    public string? LastPlanReference { get; set; }

    public int? CadastralJobNumber { get; set; }

    public int? SetoutJobNumber { get; set; }

    public string? Details { get; set; }

    public string? Colour { get; set; }
}
