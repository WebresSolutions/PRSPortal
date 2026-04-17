using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Stores Xero OAuth refresh token material and refresh/expiry metadata for accounting integration.
/// </summary>
public partial class XeroAccess
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime DateRefreshed { get; set; }

    public DateTime Expires { get; set; }
}
