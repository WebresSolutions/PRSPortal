using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace Portal.Data.Models;

/// <summary>
/// Client or vendor contact information
/// </summary>
public partial class Contact
{
    public int Id { get; set; }

    public int? ParentContactId { get; set; }

    public int? LegacyId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public int? AddressId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete TIMESTAMPTZ - NULL means active
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public NpgsqlTsVector SearchVector { get; set; } = null!;

    public virtual Address? Address { get; set; }

    public virtual ICollection<CouncilContact> CouncilContacts { get; set; } = new List<CouncilContact>();

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual ICollection<Contact> InverseParentContact { get; set; } = new List<Contact>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual Contact? ParentContact { get; set; }
}
