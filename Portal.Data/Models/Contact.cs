using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Client or vendor contact information
/// </summary>
public partial class Contact
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public int? AddressId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete timestamp - NULL means active
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual Address? Address { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual AppUser? ModifiedByUser { get; set; }
}
