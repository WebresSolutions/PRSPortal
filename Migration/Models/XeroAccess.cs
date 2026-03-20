using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class XeroAccess
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime DateRefreshed { get; set; }

    public DateTime Expires { get; set; }
}
