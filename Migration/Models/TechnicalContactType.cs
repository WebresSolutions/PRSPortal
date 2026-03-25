using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class TechnicalContactType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TechnicalContact> TechnicalContacts { get; set; } = new List<TechnicalContact>();
}
