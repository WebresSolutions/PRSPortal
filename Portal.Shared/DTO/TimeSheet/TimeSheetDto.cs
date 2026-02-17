namespace Portal.Shared.DTO.TimeSheet;

public record TimeSheetDto(DateTime Start, DateTime? End, int UserId, int? JobId, string? Description);
