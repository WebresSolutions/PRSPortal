using Microsoft.EntityFrameworkCore;
using Portal.Data;
using Portal.Data.Models;
using System.Security.Cryptography;

namespace Portal.Server.Helpers;

public static class FileHelper
{
    /// <summary>
    /// Calculates the SHA-256 hash of a byte array in 4096-byte chunks.
    /// </summary>
    /// <param name="fileBytes">The byte array representing the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the hash of the file as a string.</returns>
    public static async Task<string> GetFileHash(byte[] fileBytes)
    {
        string ret = await Task.Run(() =>
        {
            MemoryStream stream = new(fileBytes);
            const int chunkSize = 4096; // Define the chunk size for reading the byte array
            SHA256 sha256 = SHA256.Create();
            byte[] buffer = new byte[chunkSize];
            int bytesRead;

            // Read and hash the byte array in chunks
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                sha256.TransformBlock(buffer, 0, bytesRead, buffer, 0);

            sha256.TransformFinalBlock(buffer, 0, 0);
            byte[] hash = sha256.Hash ?? [];

            // Convert the hash to a hex string
            return Convert.ToHexString(hash);
        });
        return ret;
    }

    /// <summary>
    /// Used when a job is first created. This will Generate the file structure for a job. 
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="jobId">The Id of the job used for determining job number </param>
    /// <returns>A collection of job structures to generate.</returns>
    /// <exception cref="Exception"></exception>
    public static async Task<List<string>> GenerateJobFileStructure(PrsDbContext dbContext, int jobId)
    {
        // Jobs/{Year}/{JobNumber}/{JobType}/{FileType}/Files
        Job job = await dbContext.Jobs.Include(x => x.JobTypes).FirstOrDefaultAsync(x => x.Id == jobId)
            ?? throw new Exception("Could not find the provided Job");

        string fileStructure = $"Jobs/{job.CreatedOn.Year}/{job.JobNumber}";
        int[] jobTypes = [.. job.JobTypes.Select(x => x.Id)];

        FileType[] jobFileTypes = await dbContext.FileTypes
            .Include(x => x.JobType)
            .Where(x => jobTypes.Contains(x.JobTypeId))
            .ToArrayAsync();

        List<string> fileStructures = [];

        foreach (FileType fileType in jobFileTypes)
            fileStructures.Add($"{fileStructure}/{fileType.JobType.Name}/{fileType.Name}");

        return fileStructures;
    }

    /// <summary>
    /// Generates the file path for a specific job file
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="jobId">The job id of the file being uploaded</param>
    /// <param name="fileType">The file type of the file</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<string> GenerateJobFilePath(PrsDbContext dbContext, int jobId, FileType fileType)
    {
        // Jobs /{ Year}/{ JobNumber}/{ JobType}/{ FileType}/ Files
        Job job = await dbContext.Jobs.Include(x => x.JobTypes).FirstOrDefaultAsync(x => x.Id == jobId)
            ?? throw new Exception("Could not find the provided Job");

        return $"Jobs/{job.JobNumber.Take(4)}/{job.JobNumber}/{fileType.JobType.Name}/{fileType.Name}";
    }
}
