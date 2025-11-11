using System;
using System.Collections.Generic;

namespace Migration.SourceDb;

public partial class JobChecklist
{
    public uint Id { get; set; }

    public DateTime? Created { get; set; }

    public int? CreatedUser { get; set; }

    public DateTime? Modified { get; set; }

    public int? ModifiedUser { get; set; }

    public DateTime? DeletedDate { get; set; }

    public bool Deleted { get; set; }

    public int JobId { get; set; }

    public int? ReComplete { get; set; }

    public DateTime? ReCompleteDate { get; set; }

    public int? ReChecked { get; set; }

    public DateTime? ReCheckedDate { get; set; }

    public int? ReSent { get; set; }

    public DateTime? ReSentDate { get; set; }

    public int? FpComplete { get; set; }

    public DateTime? FpCompleteDate { get; set; }

    public int? FpChecked { get; set; }

    public DateTime? FpCheckedDate { get; set; }

    public int? FpSent { get; set; }

    public DateTime? FpSentDate { get; set; }

    public int? MgaComplete { get; set; }

    public DateTime? MgaCompleteDate { get; set; }

    public int? MgaChecked { get; set; }

    public DateTime? MgaCheckedDate { get; set; }

    public int? MgaSent { get; set; }

    public DateTime? MgaSentDate { get; set; }

    public int? UlComplete { get; set; }

    public DateTime? UlCompleteDate { get; set; }

    public int? UlChecked { get; set; }

    public DateTime? UlCheckedDate { get; set; }

    public int? UlSent { get; set; }

    public DateTime? UlSentDate { get; set; }

    public int? BdComplete { get; set; }

    public DateTime? BdCompleteDate { get; set; }

    public int? BdChecked { get; set; }

    public DateTime? BdCheckedDate { get; set; }

    public int? BdSent { get; set; }

    public DateTime? BdSentDate { get; set; }

    public int? AfComplete { get; set; }

    public DateTime? AfCompleteDate { get; set; }

    public int? AfChecked { get; set; }

    public DateTime? AfCheckedDate { get; set; }

    public int? AfSent { get; set; }

    public DateTime? AfSentDate { get; set; }

    public int? EsComplete { get; set; }

    public DateTime? EsCompleteDate { get; set; }

    public int? EsChecked { get; set; }

    public DateTime? EsCheckedDate { get; set; }

    public int? EsSent { get; set; }

    public DateTime? EsSentDate { get; set; }

    public int? SdComplete { get; set; }

    public DateTime? SdCompleteDate { get; set; }

    public int? SdChecked { get; set; }

    public DateTime? SdCheckedDate { get; set; }

    public int? SdSent { get; set; }

    public DateTime? SdSentDate { get; set; }

    public int? PfComplete { get; set; }

    public DateTime? PfCompleteDate { get; set; }

    public int? PfChecked { get; set; }

    public DateTime? PfCheckedDate { get; set; }

    public int? PfSent { get; set; }

    public DateTime? PfSentDate { get; set; }
}
