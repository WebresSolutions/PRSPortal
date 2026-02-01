
namespace Portal.Server.Services.Interfaces;

public interface IGraphService
{
    Task<Dictionary<string, string>> GetUsers();
}