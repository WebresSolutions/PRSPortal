using System.Net;
using System.Net.Http.Json;
using Portal.Shared.DTO.Contact;

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

    [Fact]
    public async Task Create_contact_returns_success_and_id()
    {
        string unique = $"create-{Guid.NewGuid():N}";
        ContactCreationDto dto = new()
        {
            TypeId = 1,
            FirstName = "Test",
            LastName = unique,
            Email = $"{unique}@example.com",
            Phone = "0400000000"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/contacts", dto);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        int? contactId = await response.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(contactId);
        Assert.True(contactId > 0);

        response = await _client.GetAsync($"/api/contacts/{contactId}");
        response.EnsureSuccessStatusCode();
        ContactDetailsDto? contact = await response.Content.ReadFromJsonAsync<ContactDetailsDto>();
        Assert.NotNull(contact);
        Assert.Equal(dto.FirstName, contact.FirstName);
        Assert.Equal(dto.LastName, contact.LastName);
        Assert.Equal(dto.Email, contact.Email);
        Assert.Equal(dto.Phone, contact.Phone);
        Assert.Equal(dto.TypeId, contact.ContactType);
    }

    [Fact]
    public async Task Create_contact_invalid_type_returns_bad_request()
    {
        ContactCreationDto dto = new()
        {
            TypeId = 0,
            FirstName = "Test",
            LastName = "InvalidType",
            Email = "invalid-type@example.com"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/contacts", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ContactCreationDto dtoInvalidType = new()
        {
            TypeId = 99999,
            FirstName = "Test",
            LastName = "InvalidType",
            Email = "invalid-type2@example.com"
        };
        response = await _client.PostAsJsonAsync("/api/contacts", dtoInvalidType);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_contact_returns_success()
    {
        string unique = $"update-{Guid.NewGuid():N}";
        ContactCreationDto createDto = new()
        {
            TypeId = 1,
            FirstName = "Original",
            LastName = unique,
            Email = $"{unique}@example.com"
        };

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/contacts", createDto);
        createResponse.EnsureSuccessStatusCode();
        int? contactId = await createResponse.Content.ReadFromJsonAsync<int?>();
        Assert.NotNull(contactId);
        Assert.True(contactId > 0);

        ContactUpdateDto updateDto = new()
        {
            ContactId = contactId.Value,
            TypeId = 1,
            FirstName = "Updated",
            LastName = unique,
            Email = $"{unique}@example.com",
            Phone = "0412345678"
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/contacts", updateDto);
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ContactDetailsDto? updated = await response.Content.ReadFromJsonAsync<ContactDetailsDto>();
        Assert.NotNull(updated);
        Assert.Equal(updateDto.ContactId, updated.ContactId);
        Assert.Equal(updateDto.FirstName, updated.FirstName);
        Assert.Equal(updateDto.LastName, updated.LastName);
        Assert.Equal(updateDto.Email, updated.Email);
        Assert.Equal(updateDto.Phone, updated.Phone);
    }

    [Fact]
    public async Task Update_contact_invalid_id_returns_bad_request()
    {
        ContactUpdateDto dto = new()
        {
            ContactId = 0,
            TypeId = 1,
            FirstName = "Test",
            LastName = "BadId",
            Email = "bad-id@example.com"
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/contacts", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_contact_not_found_returns_not_found()
    {
        ContactUpdateDto dto = new()
        {
            ContactId = 999999,
            TypeId = 1,
            FirstName = "Test",
            LastName = "NotFound",
            Email = "notfound@example.com"
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync("/api/contacts", dto);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
