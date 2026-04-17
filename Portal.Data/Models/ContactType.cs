using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Lookup of contact categories (e.g. client, vendor, internal) used by contact.type_id.
/// </summary>
public partial class ContactType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
