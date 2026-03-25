using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.Job;
using Portal.Shared.DTO.User;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Controllers;

public static class UserEndpoints
{
    public static void AddUserEndpoints(this WebApplication app, string tags, bool reqAuth = true)
    {
        RouteGroupBuilder userEndpointGroup = app.MapGroup("/api/users").WithTags(tags);

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

        // Gets notes for jobs assigned to a specific user, with optional inclusion of deleted notes
        userEndpointGroup.MapGet("notes/{userId}", async (
            [FromServices] IUserService userService,
            HttpContext httpContext,
            [FromRoute] int userId,
            [FromQuery] bool deleted = false,
            [FromQuery] bool? actionRequired = null
            ) =>
        {
            if (userId < 0)
                return Results.BadRequest("Bad Request User Id cannot be less than 0.");

            Result<List<JobNoteDto>> result = await userService.GetUserAssignedJobsNotes(httpContext, userId, deleted, actionRequired);
            return EndpointsHelper.ProcessResult(result, "An Error occured while loading facilities");
        })
        .WithSummary("Get notes for user's assigned jobs")
        .WithDescription("Returns notes for all jobs assigned to the specified user. Use includeDeleted query parameter to include soft-deleted notes.")
        .Produces<List<JobNoteDto>>();

        if (reqAuth)
            userEndpointGroup.RequireAuthorization();
        else
            userEndpointGroup.AllowAnonymous();
    }
}