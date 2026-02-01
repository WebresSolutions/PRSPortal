
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Portal.Server.Services.Interfaces;

namespace Portal.Server.Services.Instances;

public class GraphService : IGraphService
{
    private readonly GraphServiceClient graphClient;

    public GraphService(string clientId, string tenantId)
    {
        string[] scopes = ["User.Read"];

        // using Azure.Identity;
        DeviceCodeCredentialOptions options = new()
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            ClientId = clientId,
            TenantId = tenantId,
            // Callback function that receives the user prompt
            // Prompt contains the generated device code that user must
            // enter during the auth process in the browser
            DeviceCodeCallback = (code, cancellation) =>
            {
                Console.WriteLine(code.Message);
                return Task.FromResult(0);
            },
        };

        DeviceCodeCredential deviceCodeCredential = new(options);
        graphClient = new(deviceCodeCredential, scopes);
    }

    public async Task<Dictionary<string, string>> GetUsers()
    {
        try
        {
            UserCollectionResponse? result = await graphClient.Users.GetAsync();
            if (result?.Value is not null)
                return result.Value.ToDictionary(
                    user => user.Mail!,
                    user => user.Id!);

            throw new Exception("Failed to retrieve users from Microsoft Graph.");
        }
        catch (Exception)
        {

            throw;
        }
    }

}
