namespace Portal.Shared.DTO.Types;

/// <summary>Catalog service line (quotes / invoicing).</summary>
public record ServiceTypeDto(
    int Id,
    string? Code,
    string ServiceName,
    decimal? DefaultRate,
    string? UnitOfMeasure,
    bool? IsActive,
    string? Description);
