using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nextended.Core.Extensions;
using Portal.Data;
using Portal.Data.Models;
using Portal.Shared.DataEnums;
using Portal.Shared.DTO.Quote;
using Portal.Shared.DTO.Quote.PartailQuote;
using System.Net;
using System.Net.Http.Json;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class ClientEndpointTests
{
    private readonly HttpClient _client;
    private readonly PortalWebApplicationFactory _factory;

    public ClientEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
        _factory = fixture.Factory;
    }

    [Fact]
    public async Task Get_quote_partial_with_invalid_token_returns_unauthorized()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/client/partialquote?token=not-a-valid-token-value");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
    }

    [Fact]
    public async Task Get_quote_partial_with_valid_token_returns_reduced_details()
    {

        int quoteId = await StaticHelpers.CreateQuote(_client);
        const string rawToken = "integration-test-token-w7Qk-partial_01";
        await InsertTokenAsync(quoteId, rawToken);

        string url = $"/api/client/partialquote?token={Uri.EscapeDataString(rawToken)}";
        QuotePartialDetailsDto? dto = await _client.GetFromJsonAsync<QuotePartialDetailsDto>(url);

        Assert.NotNull(dto);
        Assert.False(string.IsNullOrWhiteSpace(dto.QuoteRef));
        Assert.Equal(5900.00m, dto.Total);
        Assert.NotNull(dto.Contact);
        Assert.False(string.IsNullOrWhiteSpace(dto.Contact.FullName));
        Assert.Equal(3, dto.LineItems.Length);

        Dictionary<string, decimal> lineTotalsByName = dto.LineItems.ToDictionary(x => x.Name, x => x.Price);
        Assert.Equal(2550.00m, lineTotalsByName["Title Re-establishment Survey"]);
        Assert.Equal(2150.00m, lineTotalsByName["Feature & AHD Level Survey"]);
        Assert.Equal(1200.00m, lineTotalsByName["Neighbourhood Site Description"]);
    }

    [Fact]
    public async Task Submit_quote_without_token_query_returns_bad_request()
    {
        ClientQuoteSubmissionDto body = ValidRejectBody("any reason");
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/client/submitquote", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_quote_with_blank_token_returns_bad_request()
    {
        ClientQuoteSubmissionDto body = ValidRejectBody("any reason");
        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/client/submitquote?token=", body);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_quote_with_invalid_token_returns_unauthorized()
    {
        ClientQuoteSubmissionDto body = ValidRejectBody("Not interested.");
        const string badToken = "not-a-valid-token";
        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/client/submitquote?token={Uri.EscapeDataString(badToken)}",
            body);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Submit_quote_with_invalid_status_returns_bad_request()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        const string rawToken = "integration-test-token-submit-invalid-status";
        await InsertTokenAsync(quoteId, rawToken);

        ClientQuoteSubmissionDto body = new()
        {
            Status = QuoteStatusEnum.Draft,
            ReasonForRejection = null
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/client/submitquote?token={Uri.EscapeDataString(rawToken)}",
            body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_quote_rejected_without_reason_returns_bad_request()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        const string rawToken = "integration-test-token-submit-reject-no-reason";
        await InsertTokenAsync(quoteId, rawToken);

        ClientQuoteSubmissionDto body = new()
        {
            Status = QuoteStatusEnum.Rejected,
            ReasonForRejection = null
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            $"/api/client/submitquote?token={Uri.EscapeDataString(rawToken)}",
            body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_quote_rejected_with_reason_updates_quote_and_returns_partial_details()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        const string rawToken = "integration-test-token-submit-reject-ok";
        await InsertTokenAsync(quoteId, rawToken);

        const string reason = "Budget no longer available for this scope.";
        ClientQuoteSubmissionDto body = ValidRejectBody(reason);

        string url = $"/api/client/submitquote?token={Uri.EscapeDataString(rawToken)}";
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();

        QuotePartialDetailsDto? dto = await response.Content.ReadFromJsonAsync<QuotePartialDetailsDto>();
        Assert.NotNull(dto);
        Assert.False(string.IsNullOrWhiteSpace(dto.QuoteRef));
        Assert.Equal(QuoteStatusEnum.Rejected, dto.QuoteStatus);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Quote? saved = await db.Quotes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == quoteId);
        Assert.NotNull(saved);
        Assert.Equal((int)QuoteStatusEnum.Rejected, saved.StatusId);
        Assert.Equal(reason, saved.QuoteRejectionReason);
    }

    [Fact]
    public async Task Submit_quote_accepted_persists_acceptance_and_returns_partial_details()
    {
        int quoteId = await StaticHelpers.CreateQuote(_client);
        const string rawToken = "integration-test-token-submit-accept-ok";
        await InsertTokenAsync(quoteId, rawToken);

        ClientQuoteSubmissionDto body = new()
        {
            Status = QuoteStatusEnum.Accepted,
            AddressIsCorrect = true,
            ContactDetailsAreCorrect = true,
            SignedName = "Integration Test Signatory",
            SignedDate = DateTime.UtcNow.ToString("O")
        };

        string url = $"/api/client/submitquote?token={Uri.EscapeDataString(rawToken)}";
        HttpResponseMessage response = await _client.PutAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();

        QuotePartialDetailsDto? dto = await response.Content.ReadFromJsonAsync<QuotePartialDetailsDto>();
        Assert.NotNull(dto);
        Assert.Equal(5900.00m, dto.Total);
        Assert.Equal(QuoteStatusEnum.Accepted, dto.QuoteStatus);

        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        Quote? saved = await db.Quotes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == quoteId);
        Assert.NotNull(saved);
        Assert.Equal((int)QuoteStatusEnum.Accepted, saved.StatusId);

        QuoteAcceptance? acceptance = await db.QuoteAcceptances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.QuoteId == quoteId);
        Assert.NotNull(acceptance);
        Assert.Equal("Integration Test Signatory", acceptance.SignatoryName);
        Assert.Equal(5900.00m, acceptance.QuoteTotalSnapshot);
    }

    private async Task InsertTokenAsync(int quoteId, string rawToken)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        PrsDbContext db = scope.ServiceProvider.GetRequiredService<PrsDbContext>();
        await StaticHelpers.InsertQuoteTokenForRawValueAsync(quoteId, rawToken, db);
    }

    private static ClientQuoteSubmissionDto ValidRejectBody(string reason) =>
        new()
        {
            Status = QuoteStatusEnum.Rejected,
            ReasonForRejection = reason
        };
}
