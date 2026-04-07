namespace Portal.Shared.DTO.Quote;

/// <summary>
/// The DTO for the quoting template item.
/// </summary>
/// <param name="Id"> The unique identifier of the quoting template item. </param>
/// <param name="ServiceTypeId"> The identifier of the service associated with this quoting template item. </param>
/// <param name="ServiceName"> The name of the service associated with this quoting template item. </param>
/// <param name="ServiceCode"> The code of the service associated with this quoting template item. </param>
/// <param name="Description"> The description of the quoting template item. </param>
/// <param name="DefaultPrice"> The default price of the quoting template item. </param>
public record QuoteTemplateItemDto(
    int Id,
    int ServiceTypeId,
    string ServiceName,
    string? Description,
    decimal DefaultPrice);