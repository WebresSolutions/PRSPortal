using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Named reusable templates; applying one copies lines into a new quote as quote_item rows.
/// </summary>
public partial class QuoteTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// Optional hint e.g. Construction vs Survey when picking a template in the UI.
    /// </summary>
    public int? JobTypeId { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? ModifiedByUserId { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual AppUser CreatedByUser { get; set; } = null!;

    public virtual JobType? JobType { get; set; }

    public virtual AppUser? ModifiedByUser { get; set; }

    public virtual ICollection<QuoteTemplateItem> QuoteTemplateItems { get; set; } = new List<QuoteTemplateItem>();
}
