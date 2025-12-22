using NpgsqlTypes;

namespace Migration.Models;

/// <summary>
/// Physical addresses for contacts and jobs
/// </summary>
public partial class Address
{
    public int Id { get; set; }

    public string Street { get; set; } = null!;

    public string Suburb { get; set; } = null!;

    public int? StateId { get; set; }

    public string PostCode { get; set; } = null!;

    public string Country { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Soft delete TIMESTAMPTZ - NULL means active
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public NpgsqlTsVector SearchVector { get; set; } = null!;

    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    public virtual ICollection<CouncilContact> CouncilContacts { get; set; } = new List<CouncilContact>();

    public virtual ICollection<Council> Councils { get; set; } = new List<Council>();

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<Quote> Quotes { get; set; } = new List<Quote>();

    public virtual State? State { get; set; }
}
