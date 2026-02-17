using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserDto[]>> GetUsers(bool activeOnly = true);
}
