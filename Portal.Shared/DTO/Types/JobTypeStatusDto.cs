namespace Portal.Shared.DTO.Types;

public record JobTypeStatusDto(int Id, int JobTypeId, string Name, int Sequence, string Colour, bool IsActive);
