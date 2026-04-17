using Portal.Shared.DataEnums;

namespace Portal.Shared.DTO.Quote;

public class ClientQuoteSubmissionDto
{
    public QuoteStatusEnum Status { get; set; }
    public bool AddressIsCorrect { get; set; }
    public bool ContactDetailsAreCorrect { get; set; }
    public string? ReasonForRejection { get; set; }
    public string? ReasonForWork { get; set; }
    public string? SiteAccessDetails { get; set; }
    public string? SignedName { get; set; }
    public string? SignedDate { get; set; }
}
