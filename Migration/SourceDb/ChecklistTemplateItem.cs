using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class ChecklistTemplateItem
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int ChecklistTemplateId { get; set; }

    public string? Name { get; set; }

    public int Order { get; set; }

    public int Number { get; set; }

    public string? Headers { get; set; }

    public string? Fields { get; set; }

    public bool Searchable { get; set; }

    public string? Text1 { get; set; }

    public string? Text2 { get; set; }

    public string? Text3 { get; set; }

    public string? Text4 { get; set; }

    public string? Memo1 { get; set; }

    public int? OtherId1 { get; set; }

    public DateTime? InvoicedDate { get; set; }

    public DateTime? Date1 { get; set; }

    public DateTime? Date2 { get; set; }

    public DateTime? Date3 { get; set; }

    public DateTime? Date4 { get; set; }
}
