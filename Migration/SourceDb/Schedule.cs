using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Schedule
{
    public uint Id { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public string? Notes { get; set; }

    public int JobId { get; set; }

    public int? ScheduleTrackId { get; set; }

    public int? QuoteId { get; set; }

    public int? TypeId { get; set; }

    public string? Colour { get; set; }
}
