namespace Portal.Shared.DTO.Quote;

/// <param name="Name"> The name of the line item, which could be a description of the service or product being quoted. </param>
/// <param name="Price"> The price of the line item </param>
/// <param name="Description"> The description of the line item. </param>
public record QuoteLineItemPartialDto(string Name, decimal Price, string Description);
