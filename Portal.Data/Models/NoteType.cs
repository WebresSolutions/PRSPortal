using System;
using System.Collections.Generic;

namespace Portal.Data.Models;

/// <summary>
/// Type of note
/// </summary>
public partial class NoteType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}
