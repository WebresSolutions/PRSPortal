using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

public partial class QuoteToken
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Quote Quote { get; set; } = null!;

    public virtual ICollection<QuoteAcceptance> QuoteAcceptances { get; set; } = new List<QuoteAcceptance>();
}
