using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class JobQuote
{
    public int Id { get; set; }

    public int JobId { get; set; }

    public int QuoteId { get; set; }
}
