using Microsoft.Extensions.Options;
using Portal.Server.Options;
using Portal.Server.Services.Interfaces;
using Xero.NetStandard.OAuth2.Api;

namespace Portal.Server.Services.Instances;

public class XeroIntegrationService(IAccountingApi accountingApi, IOptions<XeroOptions> xeroOptions) : IXeroIntegrationService
{
    private readonly IAccountingApi _AccountingApi = accountingApi;
    private readonly XeroOptions _XeroOptions = xeroOptions.Value;


}
