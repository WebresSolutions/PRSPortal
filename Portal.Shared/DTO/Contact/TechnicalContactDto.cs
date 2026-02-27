namespace Portal.Shared.DTO.Contact;

public record TechnicalContactDto(int id, int ContactId, int JobId, string ContactType, string ContactName, string Email, string? Phone);