namespace Portal.Shared.DTO.Contact;

public record SubContactDto(int ContactId, int ParentContactId, int ContactType, string ContactTypeName, string FullName, string Mobile, string? Phone, string Email, bool Deleted);
