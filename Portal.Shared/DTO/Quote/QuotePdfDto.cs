namespace Portal.Shared.DTO.Quote;

public class QuotePdfDto
{
    public byte[] Data { get; set; } = [];
    public required string FileName { get; set; }
}
