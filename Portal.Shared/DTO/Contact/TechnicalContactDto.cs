namespace Portal.Shared.DTO.Contact;

public record TechnicalContactDto(int Id, int ContactId, int JobId, string JobNumber, int ContactTypeId, string ContactType, string ContactName, string Email, string? Phone, bool Deleted);