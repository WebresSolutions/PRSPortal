namespace Portal.Shared.DTO.TimeSheet;

public record TimeSheetDto(int id, DateTime Start, DateTime? End, int UserId, int? JobId, string? Description);
