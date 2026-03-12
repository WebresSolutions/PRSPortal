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


}
