using Portal.Shared.DTO.Types;

namespace Portal.Shared.DTO.Job;

public record JobHistoryDto(int JobId, JobTypeStatusDto Status, DateTime DateChanged, string ModifiedByUserName);