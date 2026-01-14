using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class User
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int GroupId { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? SigningName { get; set; }

    public int? WorkRate { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool? Active { get; set; }
}
