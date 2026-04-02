using Portal.Shared.DataEnums;

namespace Portal.Shared.DTO.Quote;

/// <summary>
/// The DTO for the quoting template.
/// </summary>
/// <param name="Id"> The unique identifier of the quoting template. </param>
/// <param name="Name"> The name of the quoting template. </param>
/// <param name="Description"> The description of the quoting template. </param>
/// <param name="IsActive"> Whether the quoting template is active. </param>
/// <param name="CreatedOn"> The date and time the quoting template was created. </param>
/// <param name="ModifiedOn"> The date and time the quoting template was modified. </param>
/// <param name="ModifiedBy"> The user who modified the quoting template. </param>
/// <param name="JobType"> The job type of the quoting template. </param>
/// <param name="QuoteTemplateItems"> The items of the quoting template. </param>
public record QuoteTemplateDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedOn,
    DateTime? ModifiedOn,
    string? ModifiedBy,
    JobTypeEnum JobType,
    QuoteTemplateItemDto[] QuoteTemplateItems);