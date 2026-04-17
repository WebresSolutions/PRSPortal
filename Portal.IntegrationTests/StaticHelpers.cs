using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using Portal.Shared;
using Portal.Shared.DTO.Address;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Types;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Portal.IntegrationTests;

public class StaticHelpers
{
    private static string HashQuoteTokenForIntegrationTest(string rawToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    public static async Task InsertQuoteTokenForRawValueAsync(int quoteId, string rawToken, PrsDbContext db)
    {
        await db.QuoteTokens.Where(t => t.QuoteId == quoteId).ExecuteDeleteAsync();
        await db.QuoteTokens.AddAsync(new QuoteToken
        {
            QuoteId = quoteId,
            Token = HashQuoteTokenForIntegrationTest(rawToken),
            CreatedOn = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            UsedAt = null,
        });
        await db.SaveChangesAsync();
    }

    public static async Task<int> CreateQuote(HttpClient client)
    {
        ServiceTypeDto[] serviceTypes = (await client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service"))!;

        int titleReestablishmentServiceId = serviceTypes.First(x => x.ServiceName == "Title Re-establishment Survey").Id;
        int featureAndAhdLevelServiceId = serviceTypes.First(x => x.ServiceName == "Feature & AHD Level Survey").Id;
        int neighbourhoodSiteDescriptionServiceId = serviceTypes.First(x => x.ServiceName == "Neighbourhood Site Description").Id;

        QuoteCreationDto request = new()
        {
            QuoteTypeId = 2,
            QuoteStatusId = 1,
            ContactId = 1,
            Description = "Pricing verification quote",
            TargetDeliveryDate = DateTime.UtcNow.Date.AddDays(14),
            Address = new AddressDto
            {
                Street = "117-131 CAPEL STREET",
                Suburb = "NORTH MELBOURNE",
                PostCode = "3051",
                State = StateEnum.VIC
            },
            QuoteItems =
            [
                new QuoteItemDto { ServiceTypeId = titleReestablishmentServiceId, Price = 2550.00m },
                new QuoteItemDto { ServiceTypeId = featureAndAhdLevelServiceId, Price = 2150.00m },
                new QuoteItemDto { ServiceTypeId = neighbourhoodSiteDescriptionServiceId, Price = 1200.00m }
            ]
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/quotes", request);
        response.EnsureSuccessStatusCode();

        int? quoteId = await response.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(quoteId);
        return quoteId.Value;
    }

}
