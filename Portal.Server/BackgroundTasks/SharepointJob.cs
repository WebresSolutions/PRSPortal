using Portal.Server.Services.Interfaces;
using Quartz;

namespace Portal.Server.BackgroundTasks;

public class SharepointJob(ILogger<SharepointJob> _logger, IFileService fileService) : IJob
{
    public const string Name = nameof(SharepointJob);

    /// <summary>
    /// Executes creating the job file structure in sharepoint.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap data = context.MergedJobDataMap;

        // Get job data - note that this isn't strongly typed
        int jobId = data.GetInt(BackgroundTasks.SchedulerConstants.JobId);
        if (jobId is 0)
        {
            _logger.LogWarning("Invalid Request. Job Id was zero");
            return;
        }
        try
        {
            await fileService.CreateSharePointFileStructure(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create job directory structure {jobId}", jobId);
            throw;
        }
    }
}
