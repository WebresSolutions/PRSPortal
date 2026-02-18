namespace Portal.Shared.DTO.TimeSheet;

public record TimeSheetEntryDto(DateTime start, DateTime? end, string description, int? jobId, int? userId);