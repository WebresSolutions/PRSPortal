using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class ContactAlert
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int ContactId { get; set; }

    public string AlertType { get; set; } = null!;

    public string Text { get; set; } = null!;
}
