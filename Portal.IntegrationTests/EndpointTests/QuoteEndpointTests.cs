using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portal.Data;
using Portal.Data.Models;
using Portal.Shared;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Types;
using Portal.Shared.ResponseModels;
using System.Net;
using System.Net.Http.Json;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class QuoteEndpointTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient();
    private readonly PortalWebApplicationFactory _factory = fixture.Factory;

    [Fact]
    public async Task Get_service_types_includes_seeded_quote_services()
    {
        ServiceTypeDto[]? serviceTypes = await _client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service");

        Assert.NotNull(serviceTypes);
        Assert.Contains(serviceTypes, x => x.ServiceName == "Title Re-establishment Survey");
        Assert.Contains(serviceTypes, x => x.ServiceName == "Feature & AHD Level Survey");
        Assert.Contains(serviceTypes, x => x.ServiceName == "Neighbourhood Site Description");
    }

    [Fact]
    public async Task Create_quote_sums_sample_service_prices_and_persists_line_items()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext dbContext = scope.ServiceProvider.GetRequiredService<PrsDbContext>();

        Data.Models.Quote? savedQuote = await dbContext.Quotes
            .Include(q => q.QuoteItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        Assert.NotNull(savedQuote);
        Assert.Equal(2, savedQuote.JobTypeId);
        Assert.Equal(5900.00m, savedQuote.TotalPrice);
        Assert.Equal(3, savedQuote.QuoteItems.Count);

        Dictionary<string, decimal> lineTotalsByService = savedQuote.QuoteItems
            .ToDictionary(x => x.ServiceNameSnapshot, x => x.Total);

        Assert.Equal(2550.00m, lineTotalsByService["Title Re-establishment Survey"]);
        Assert.Equal(2150.00m, lineTotalsByService["Feature & AHD Level Survey"]);
        Assert.Equal(1200.00m, lineTotalsByService["Neighbourhood Site Description"]);
    }

    [Fact]
    public async Task Get_quote_details_returns_quote_with_line_items()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        QuoteDetailsDto? quoteDetails = await _client.GetFromJsonAsync<QuoteDetailsDto>($"/api/quotes/{quoteId}");
        Assert.NotNull(quoteDetails);
        Assert.Equal(quoteId, quoteDetails.Id);
        Assert.Equal(3, quoteDetails.QuoteItems.Length);

        Dictionary<string, decimal> lineTotalsByService = quoteDetails.QuoteItems
            .ToDictionary(x => x.ServiceName, x => x.Price);

        Assert.Equal(2550.00m, lineTotalsByService["Title Re-establishment Survey"]);
        Assert.Equal(2150.00m, lineTotalsByService["Feature & AHD Level Survey"]);
        Assert.Equal(1200.00m, lineTotalsByService["Neighbourhood Site Description"]);
    }

    [Fact]
    public async Task Update_quote_removes_line_items_not_in_payload_and_recalculates_total()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        QuoteDetailsDto? before = await _client.GetFromJsonAsync<QuoteDetailsDto>($"/api/quotes/{quoteId}");
        Assert.NotNull(before);
        Assert.Equal(3, before.QuoteItems.Length);

        List<QuoteItemDto> twoLines = CloneQuoteItems(before.QuoteItems.Take(2));
        QuoteUpdateDto update = ToUpdateDto(before, twoLines);

        HttpResponseMessage putResponse = await _client.PutAsJsonAsync("/api/quotes", update);
        putResponse.EnsureSuccessStatusCode();
        int? returnedId = await putResponse.Content.ReadFromJsonAsync<int?>();
        Assert.Equal(quoteId, returnedId);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Data.Models.Quote? saved = await db.Quotes.Include(q => q.QuoteItems).FirstAsync(q => q.Id == quoteId);
        Assert.Equal(2, saved.QuoteItems.Count);
        Assert.Equal(4700.00m, saved.TotalPrice);
    }

    [Fact]
    public async Task Update_quote_adds_new_line_item_and_recalculates_total()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        QuoteDetailsDto? before = await _client.GetFromJsonAsync<QuoteDetailsDto>($"/api/quotes/{quoteId}");
        Assert.NotNull(before);

        ServiceTypeDto[] serviceTypes = (await _client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service"))!;
        int extraServiceId = serviceTypes.First(x => x.ServiceName == "Neighbourhood Site Description").Id;

        List<QuoteItemDto> items = CloneQuoteItems(before.QuoteItems);
        items.Add(new QuoteItemDto
        {
            Id = 0,
            ServiceTypeId = extraServiceId,
            ServiceName = "",
            Price = 100.00m
        });

        QuoteUpdateDto update = ToUpdateDto(before, items);
        HttpResponseMessage putResponse = await _client.PutAsJsonAsync("/api/quotes", update);
        putResponse.EnsureSuccessStatusCode();

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Data.Models.Quote? saved = await db.Quotes.Include(q => q.QuoteItems).FirstAsync(q => q.Id == quoteId);
        Assert.Equal(4, saved.QuoteItems.Count);
        Assert.Equal(6000.00m, saved.TotalPrice);
    }

    [Fact]
    public async Task Update_quote_updates_existing_line_amounts_and_recalculates_total()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        QuoteDetailsDto? before = await _client.GetFromJsonAsync<QuoteDetailsDto>($"/api/quotes/{quoteId}");
        Assert.NotNull(before);

        List<QuoteItemDto> items = CloneQuoteItems(before.QuoteItems);
        QuoteItemDto first = items.First(x => x.ServiceName == "Title Re-establishment Survey");
        first.Price = 2600.00m;

        QuoteUpdateDto update = ToUpdateDto(before, items);
        HttpResponseMessage putResponse = await _client.PutAsJsonAsync("/api/quotes", update);
        putResponse.EnsureSuccessStatusCode();

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Data.Models.Quote? saved = await db.Quotes.Include(q => q.QuoteItems).FirstAsync(q => q.Id == quoteId);
        Assert.Equal(3, saved.QuoteItems.Count);
        Assert.Equal(5950.00m, saved.TotalPrice);
        Assert.Equal(2600.00m, saved.QuoteItems.First(i => i.ServiceNameSnapshot == "Title Re-establishment Survey").Total);
    }

    [Fact]
    public async Task Get_quotes_all()
    {
        int quoteId1 = await StaticHelpers.CreateQuote(_client);
        int quoteId2 = await StaticHelpers.CreateQuote(_client);

        QuoteFilterDto quoteFilterDto = new()
        {
            Page = 1,
            PageSize = 10,
            JobNumberSearch = null,
            ContactSearch = null,
            AddressSearch = null,
            OrderBy = null,
            Order = SortDirectionEnum.Asc,
            Deleted = false
        };

        PagedResponse<QuoteListDto>? quoteListDto = await _client.GetFromJsonAsync<PagedResponse<QuoteListDto>>("/api/quotes");
        Assert.NotNull(quoteListDto);

        int count = quoteListDto.Result.Count(q => q.Id == quoteId1 || q.Id == quoteId2);
        Assert.True(count >= 2, "The count of the returned filter should be greater than 2.");
        Assert.Equal(10, quoteFilterDto.PageSize);
        Assert.Equal(1, quoteFilterDto.Page);
    }

    [Fact]
    public async Task Put_quote_template_creates_template_and_persists_line_items_with_service_snapshots()
    {
        ServiceTypeDto[] serviceTypes = (await _client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service"))!;
        int titleId = serviceTypes.First(x => x.ServiceName == "Title Re-establishment Survey").Id;
        int featureId = serviceTypes.First(x => x.ServiceName == "Feature & AHD Level Survey").Id;

        QuoteTemplateDto request = new(
            0,
            "Integration template — create",
            "Created by integration test",
            true,
            DateTime.UtcNow,
            null,
            null,
            JobTypeEnum.Surveying,
            [
                new QuoteTemplateItemDto(0, titleId, "", "Title line", 2550.00m),
                new QuoteTemplateItemDto(0, featureId, "", "Feature line", 2150.00m)
            ]);

        QuoteTemplateDto created = await PutQuoteTemplateAsync(request);
        Assert.True(created.Id > 0);
        Assert.Equal(request.Name, created.Name);
        Assert.Equal(2, created.QuoteTemplateItems.Length);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Data.Models.QuoteTemplate? saved = await db.QuoteTemplates
            .Include(t => t.QuoteTemplateItems)
            .FirstOrDefaultAsync(t => t.Id == created.Id);

        Assert.NotNull(saved);
        Assert.Equal(2, saved.QuoteTemplateItems.Count);
        Assert.Contains(saved.QuoteTemplateItems, i => i.ServiceNameSnapshot == "Title Re-establishment Survey" && i.DefaultPrice == 2550.00m);
        Assert.Contains(saved.QuoteTemplateItems, i => i.ServiceNameSnapshot == "Feature & AHD Level Survey" && i.DefaultPrice == 2150.00m);
    }

    [Fact]
    public async Task Put_quote_template_returns_bad_request_when_service_type_invalid()
    {
        QuoteTemplateDto request = new(
            0,
            "Bad services",
            null,
            true,
            DateTime.UtcNow,
            null,
            null,
            JobTypeEnum.Surveying,
            [new QuoteTemplateItemDto(0, 999_999, "", null, 1.00m)]);

        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/quotes/templates", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_quote_template_updates_name_and_line_item_defaults()
    {
        ServiceTypeDto[] serviceTypes = (await _client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service"))!;
        int titleId = serviceTypes.First(x => x.ServiceName == "Title Re-establishment Survey").Id;
        int neighbourhoodId = serviceTypes.First(x => x.ServiceName == "Neighbourhood Site Description").Id;

        QuoteTemplateDto created = await PutQuoteTemplateAsync(new QuoteTemplateDto(
            0,
            "Template before update",
            "Desc",
            true,
            DateTime.UtcNow,
            null,
            null,
            JobTypeEnum.Construction,
            [new QuoteTemplateItemDto(0, titleId, "", "Original", 100.00m)]));

        QuoteTemplateItemDto existingLine = created.QuoteTemplateItems.Single();
        QuoteTemplateItemDto updatedLine = existingLine with
        {
            Description = "Updated description",
            DefaultPrice = 199.50m,
            ServiceName = "Title Re-establishment Survey"
        };

        QuoteTemplateItemDto newLine = new(0, neighbourhoodId, "Neighbourhood Site Description", "New default line", 50.00m);

        QuoteTemplateDto update = new(
            created.Id,
            "Template after update",
            "Desc updated",
            true,
            created.CreatedOn,
            created.ModifiedOn,
            created.ModifiedBy,
            JobTypeEnum.Construction,
            [updatedLine, newLine]);

        QuoteTemplateDto after = await PutQuoteTemplateAsync(update);
        Assert.Equal("Template after update", after.Name);
        Assert.Equal(2, after.QuoteTemplateItems.Length);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Data.Models.QuoteTemplate? saved = await db.QuoteTemplates
            .Include(t => t.QuoteTemplateItems)
            .FirstAsync(t => t.Id == created.Id);

        Assert.Equal(2, saved.QuoteTemplateItems.Count);
        Data.Models.QuoteTemplateItem titleRow = saved.QuoteTemplateItems.Single(i => i.ServiceId == titleId);
        Assert.Equal(199.50m, titleRow.DefaultPrice);
        Assert.Equal("Updated description", titleRow.Description);
        Assert.Contains(saved.QuoteTemplateItems, i => i.ServiceId == neighbourhoodId && i.DefaultPrice == 50.00m);
    }

    [Fact]
    public async Task Put_quote_template_update_removes_line_items_not_in_payload()
    {
        ServiceTypeDto[] serviceTypes = (await _client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service"))!;
        int titleId = serviceTypes.First(x => x.ServiceName == "Title Re-establishment Survey").Id;
        int featureId = serviceTypes.First(x => x.ServiceName == "Feature & AHD Level Survey").Id;

        QuoteTemplateDto created = await PutQuoteTemplateAsync(new QuoteTemplateDto(
            0,
            "Two lines then one",
            null,
            true,
            DateTime.UtcNow,
            null,
            null,
            JobTypeEnum.Surveying,
            [
                new QuoteTemplateItemDto(0, titleId, "Title Re-establishment Survey", "A", 1m),
                new QuoteTemplateItemDto(0, featureId, "Feature & AHD Level Survey", "B", 2m)
            ]));

        QuoteTemplateItemDto keep = created.QuoteTemplateItems.First(i => i.ServiceTypeId == titleId);
        keep = keep with { ServiceName = "Title Re-establishment Survey" };

        QuoteTemplateDto update = new(
            created.Id,
            created.Name,
            created.Description,
            created.IsActive,
            created.CreatedOn,
            created.ModifiedOn,
            created.ModifiedBy,
            created.JobType,
            [keep]);

        await PutQuoteTemplateAsync(update);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        List<Data.Models.QuoteTemplateItem> items = await db.QuoteTemplateItems
            .Where(i => i.QuoteTemplateId == created.Id)
            .ToListAsync();

        Assert.Single(items);
        Assert.Equal(titleId, items[0].ServiceId);
    }

    [Fact]
    public async Task Get_quote_templates_includes_created_template()
    {
        string uniqueName = $"Listed template {Guid.NewGuid():N}";
        ServiceTypeDto[] serviceTypes = (await _client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service"))!;
        int titleId = serviceTypes.First(x => x.ServiceName == "Title Re-establishment Survey").Id;

        await PutQuoteTemplateAsync(new QuoteTemplateDto(
            0,
            uniqueName,
            null,
            true,
            DateTime.UtcNow,
            null,
            null,
            JobTypeEnum.Surveying,
            [new QuoteTemplateItemDto(0, titleId, "", null, 1m)]));

        QuoteTemplateDto[]? list = await _client.GetFromJsonAsync<QuoteTemplateDto[]>("/api/quotes/templates");
        Assert.NotNull(list);
        Assert.Contains(list, t => t.Name == uniqueName);
    }

    [Fact]
    public async Task Delete_quote_sets_deleted_at_and_returns_true()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/quotes/{quoteId}");
        response.EnsureSuccessStatusCode();
        bool? body = await response.Content.ReadFromJsonAsync<bool?>();
        Assert.True(body);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Data.Models.Quote? saved = await db.Quotes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == quoteId);
        Assert.NotNull(saved);
        Assert.NotNull(saved.DeletedAt);

        PagedResponse<QuoteListDto>? quoteListDto = await _client.GetFromJsonAsync<PagedResponse<QuoteListDto>>("/api/quotes");
        Assert.NotNull(quoteListDto);
        Assert.DoesNotContain(quoteListDto.Result, q => q.Id == quoteId);
    }

    [Fact]
    public async Task Delete_quote_second_time_returns_bad_request()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        HttpResponseMessage first = await _client.DeleteAsync($"/api/quotes/{quoteId}");
        first.EnsureSuccessStatusCode();

        HttpResponseMessage second = await _client.DeleteAsync($"/api/quotes/{quoteId}");
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Delete_quote_unknown_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/quotes/999999");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_quote_invalid_route_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/quotes/0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_quote_pdf_returns_payload_for_existing_quote()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);

        QuotePdfDto? pdf = await _client.GetFromJsonAsync<QuotePdfDto>($"/api/quotes/{quoteId}/pdf");

        Assert.NotNull(pdf);
        Assert.False(string.IsNullOrWhiteSpace(pdf.FileName));
        Assert.EndsWith(".pdf", pdf.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("Quote_", pdf.FileName, StringComparison.Ordinal);
        File.WriteAllBytes("quote_test_output.pdf", pdf.Data);
        Assert.NotEmpty(pdf.Data);
    }

    [Fact]
    public async Task Get_quote_pdf_returns_bad_request_for_invalid_route_id()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/quotes/0/pdf");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_quote_pdf_returns_bad_request_for_unknown_quote()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/quotes/999999/pdf");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_quote_partial_without_token_query_returns_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/quotes/partial");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_quote_partial_with_blank_token_returns_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/quotes/partial?token=");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Send_quote_to_client_returns_ok_and_marks_quote_sent()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);

        using HttpRequestMessage request = new(HttpMethod.Put, $"/api/quotes/{quoteId}/send");
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        int? returnedId = await response.Content.ReadFromJsonAsync<int?>();
        Assert.Equal(quoteId, returnedId);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Data.Models.Quote? saved = await db.Quotes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == quoteId);
        Assert.NotNull(saved);
        Assert.Equal((int)QuoteStatusEnum.Sent, saved.StatusId);
        Assert.NotNull(saved.DateSentToClient);
        Assert.Equal(1, saved.QuoteSentByUserId);


        QuoteToken? token = await db.QuoteTokens.FirstOrDefaultAsync(t => t.QuoteId == returnedId);
        Assert.NotNull(token);
        Assert.True(token.ExpiresAt > DateTime.UtcNow.AddDays(13));
    }

    [Fact]
    public async Task Send_quote_to_client_returns_bad_request_for_unknown_quote()
    {
        using HttpRequestMessage request = new(HttpMethod.Put, "/api/quotes/999999/send");
        HttpResponseMessage response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_quote_template_sets_deleted_at_and_excludes_from_get_list()
    {
        string uniqueName = $"Delete template test {Guid.NewGuid():N}";
        ServiceTypeDto[] serviceTypes = (await _client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service"))!;
        int titleId = serviceTypes.First(x => x.ServiceName == "Title Re-establishment Survey").Id;

        QuoteTemplateDto created = await PutQuoteTemplateAsync(new QuoteTemplateDto(
            0,
            uniqueName,
            null,
            true,
            DateTime.UtcNow,
            null,
            null,
            JobTypeEnum.Surveying,
            [new QuoteTemplateItemDto(0, titleId, "", null, 1m)]));

        QuoteTemplateDto[]? before = await _client.GetFromJsonAsync<QuoteTemplateDto[]>("/api/quotes/templates");
        Assert.NotNull(before);
        Assert.Contains(before, t => t.Id == created.Id && t.Name == uniqueName);

        HttpResponseMessage deleteResponse = await _client.DeleteAsync($"/api/quotes/templates/{created.Id}");
        deleteResponse.EnsureSuccessStatusCode();
        bool? deleted = await deleteResponse.Content.ReadFromJsonAsync<bool?>();
        Assert.True(deleted);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Data.Models.QuoteTemplate? saved = await db.QuoteTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == created.Id);
        Assert.NotNull(saved);
        Assert.NotNull(saved.DeletedAt);


        QuoteTemplateDto[]? after = await _client.GetFromJsonAsync<QuoteTemplateDto[]>("/api/quotes/templates");
        Assert.NotNull(after);
        Assert.DoesNotContain(after, t => t.Id == created.Id);
    }

    [Fact]
    public async Task Delete_quote_template_second_time_returns_bad_request()
    {
        ServiceTypeDto[] serviceTypes = (await _client.GetFromJsonAsync<ServiceTypeDto[]>("/api/types/service"))!;
        int titleId = serviceTypes.First(x => x.ServiceName == "Title Re-establishment Survey").Id;

        QuoteTemplateDto created = await PutQuoteTemplateAsync(new QuoteTemplateDto(
            0,
            $"Twice-delete template {Guid.NewGuid():N}",
            null,
            true,
            DateTime.UtcNow,
            null,
            null,
            JobTypeEnum.Surveying,
            [new QuoteTemplateItemDto(0, titleId, "", null, 1m)]));

        HttpResponseMessage first = await _client.DeleteAsync($"/api/quotes/templates/{created.Id}");
        first.EnsureSuccessStatusCode();

        HttpResponseMessage second = await _client.DeleteAsync($"/api/quotes/templates/{created.Id}");
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Delete_quote_template_unknown_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.DeleteAsync("/api/quotes/templates/999999");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<QuoteTemplateDto> PutQuoteTemplateAsync(QuoteTemplateDto dto)
    {
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/quotes/templates", dto);
        response.EnsureSuccessStatusCode();
        QuoteTemplateDto? body = await response.Content.ReadFromJsonAsync<QuoteTemplateDto>();
        Assert.NotNull(body);
        return body;
    }

    private static List<QuoteItemDto> CloneQuoteItems(IEnumerable<QuoteItemDto> items) =>
        [.. items.Select(i => new QuoteItemDto
        {
            Id = i.Id,
            ServiceTypeId = i.ServiceTypeId,
            ServiceName = i.ServiceName,
            Price = i.Price,
            Description = i.Description
        })];

    private static QuoteUpdateDto ToUpdateDto(QuoteDetailsDto details, List<QuoteItemDto> quoteItems)
    {
        if (details.Contact is null)
            throw new InvalidOperationException("Quote details missing contact.");
        if (details.Address is null)
            throw new InvalidOperationException("Quote details missing address.");

        JobTypeEnum jobType = details.QuoteTypeId switch
        {
            (int)JobTypeEnum.Construction => JobTypeEnum.Construction,
            (int)JobTypeEnum.Surveying => JobTypeEnum.Surveying,
            _ => JobTypeEnum.Surveying
        };

        return new QuoteUpdateDto
        {
            QuoteId = details.Id,
            QuoteReferenceNumber = details.QuoteReference,
            Description = details.Description,
            QuoteStatusId = details.QuotesStatus.StatusEnum,
            JobType = jobType,
            ContactId = details.Contact.ContactId,
            TargetDeliveryDate = details.TargetDeliveryDate,
            Address = details.Address,
            QuoteItems = quoteItems
        };
    }
}
