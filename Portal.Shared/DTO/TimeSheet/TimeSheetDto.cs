namespace Portal.Shared.DTO.TimeSheet;

public record TimeSheetDto(int Id, DateTime Start, DateTime? End, int UserId, int? JobId, string? Description, string UserDisplayName);
