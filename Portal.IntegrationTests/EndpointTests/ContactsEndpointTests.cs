using System.Net;

namespace Portal.IntegrationTests.EndpointTests;

[Collection(nameof(IntegrationTestCollection))]
public sealed class ContactsEndpointTests
{
    private readonly HttpClient _client;

    public ContactsEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task List_contacts_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/contacts?page=1&pageSize=10");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task List_contacts_with_split_search_params_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/contacts?page=1&pageSize=10&nameSearch=test&emailSearch=@example&phoneSearch=04&addressSearch=street&contactIdSearch=1");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task List_contacts_with_searchFilter_returns_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/contacts?page=1&pageSize=10&searchFilter=acme");
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_contact_by_invalid_id_returns_bad_request()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/contacts/0");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_contact_by_id_returns_not_found_or_ok()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/contacts/1");
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NotFound,
            $"Unexpected status: {response.StatusCode}");
    }
}
