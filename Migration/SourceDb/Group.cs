using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Group
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int ParentId { get; set; }

    public int Lft { get; set; }

    public int Rght { get; set; }
}
