using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class JobsUser
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int JobId { get; set; }

    public int UserId { get; set; }
}
