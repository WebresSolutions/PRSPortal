using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Task
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int JobId { get; set; }

    public int? ChecklistTemplateId { get; set; }

    public string TaskType { get; set; } = null!;

    public double? QuotedPrice { get; set; }

    public DateTime? QuotedDate { get; set; }

    public string? Details { get; set; }

    public string? PurchaseOrderNumber { get; set; }

    public DateTime? ActiveDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? InvoicedDate { get; set; }

    public bool InvoiceNotRequired { get; set; }

    public bool SetoutTask { get; set; }
}
