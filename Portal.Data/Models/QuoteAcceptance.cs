using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Record when a client accepted a fee proposal via token link; optional drawn signature image (bytea).
/// </summary>
public partial class QuoteAcceptance
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    /// <summary>
    /// Which quote_token was validated at acceptance time; NULL if token row was purged.
    /// </summary>
    public int? QuoteTokenId { get; set; }

    public DateTime AcceptedAt { get; set; }

    /// <summary>
    /// Optional typed full name in addition to or instead of signature_image.
    /// </summary>
    public string? SignatoryName { get; set; }

    /// <summary>
    /// Small raster signature (e.g. PNG); NULL if only typed name / checkbox acceptance.
    /// </summary>
    public byte[]? SignatureImage { get; set; }

    public string SignatureContentType { get; set; } = null!;

    public string? ClientIp { get; set; }

    public string? UserAgent { get; set; }

    /// <summary>
    /// Quote reference at time of acceptance for audit trail.
    /// </summary>
    public string QuoteReferenceSnapshot { get; set; } = null!;

    /// <summary>
    /// Total price at time of acceptance for audit trail.
    /// </summary>
    public decimal QuoteTotalSnapshot { get; set; }

    public virtual Quote Quote { get; set; } = null!;

    public virtual QuoteToken? QuoteToken { get; set; }
}
