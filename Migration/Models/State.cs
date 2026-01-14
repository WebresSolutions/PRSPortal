using System;
using System.Collections.Generic;

namespace Migration.Models;

/// <summary>
/// States or territories for address management
/// </summary>
public partial class State
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Abbreviation { get; set; } = null!;

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
