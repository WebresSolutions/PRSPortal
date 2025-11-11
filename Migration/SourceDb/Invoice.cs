using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class Invoice
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public bool SetoutInvoice { get; set; }

    public bool SetoutJobNumber { get; set; }

    public int? Number { get; set; }

    public string? Suffix { get; set; }

    public int JobId { get; set; }

    public int ContactId { get; set; }

    public string? PurchaseOrderNumber { get; set; }

    public string? Invoice1 { get; set; }

    public DateTime? ReadyDate { get; set; }

    public DateTime? SentDate { get; set; }

    public DateTime? PaymentReceivedDate { get; set; }

    public double? TotalPrice { get; set; }
}
