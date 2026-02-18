using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

public static class UserEndpoints
{
    public static void AddUserEndpoints(this WebApplication app, bool reqAuth = true)
    {
        RouteGroupBuilder userEndpointGroup = app.MapGroup("/api/users");

        userEndpointGroup.MapGet(
            "", async (
                [FromServices] IUserService userService,
                [FromQuery] bool? activeOnly) =>
            {
                activeOnly ??= true;

                Result<UserDto[]> result = await userService.GetUsers(true);
                return EndpointsHelper.ProcessResult(result, "An Error occured while saving system settings");
            })
            .WithSummary("Get all users")
            .WithDescription("Returns a collection of Users. Optional flag for active users only")
            .Produces<UserDto[]>();
    }
}