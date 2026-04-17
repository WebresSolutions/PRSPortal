using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Secret or hashed token rows for client portal access to a quote (e.g. email link); expires and optional single-use via used_at.
/// </summary>
public partial class QuoteToken
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    /// <summary>
    /// Opaque token or stored hash, depending on application strategy; must match email link validation.
    /// </summary>
    public string Token { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// After this instant the token must not grant access.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// If set, token was consumed (e.g. one-time flow); leave NULL for multi-use view tokens.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    public virtual Quote Quote { get; set; } = null!;

    public virtual ICollection<QuoteAcceptance> QuoteAcceptances { get; set; } = new List<QuoteAcceptance>();
}
