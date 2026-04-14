using Microsoft.AspNetCore.Mvc;
using Portal.Server.Helpers;
using Portal.Server.Services.Interfaces;
using Portal.Shared.DTO.File;
using Portal.Shared.ResponseModels;

namespace Portal.Server.Endpoints;

public static class FileEndpoint
{
    public static WebApplication AddFileEndpoints(this WebApplication app, string tags, bool reqAuth = true)
    {
        RouteGroupBuilder appGroup = app.MapGroup("/api/files").WithTags(tags);

        appGroup.MapGet("{fileId}", async (
           [FromServices] IFileService fileService,
           [FromRoute] int fileid
       ) =>
        {
            Result<FileDto> result = await fileService.GetFileData(fileid);
            return EndpointsHelper.ProcessResult(result, "An error occurred while loading jobs");
        })
       .WithSummary("Gets the databse from a file")
       .WithDescription("Gets a file from sharepoint with meta data.")
       .Produces<FileDto>();

        if (reqAuth)
            appGroup.RequireAuthorization();
        else
            appGroup.AllowAnonymous();

        return app;
    }
}