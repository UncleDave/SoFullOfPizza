using Azure.Storage.Blobs;
using FluentFTP;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SoFullOfPizza.Satisfactory.SaveDownloader;

public static class SaveDownloader
{
    [FunctionName("SaveDownloader")]
    public static async Task RunAsync(
        [TimerTrigger("0 */15 * * * *")] TimerInfo myTimer,
        ILogger log
    )
    {
        var ftpHost = GetSetting("Ftp:Host");
        var ftpUser = GetSetting("Ftp:User");
        var ftpPassword = GetSetting("Ftp:Password");
        var ftpFilePath = GetSetting("Ftp:FilePath");

        var blobStorageConnectionString = GetSetting("BlobStorage:ConnectionString");
        var blobStorageContainerName = GetSetting("BlobStorage:ContainerName");
        var blobStorageBlobName = GetSetting("BlobStorage:BlobName");

        var ftpClient = new AsyncFtpClient(ftpHost, ftpUser, ftpPassword);

        var blobServiceClient = new BlobServiceClient(blobStorageConnectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(
            blobStorageContainerName
        );
        var blobClient = blobContainerClient.GetBlobClient(blobStorageBlobName);

        await ftpClient.AutoConnect();

        var stream = await blobClient.OpenWriteAsync(true);
        var success = await ftpClient.DownloadStream(stream, ftpFilePath);

        log.Log(
            success ? LogLevel.Information : LogLevel.Error,
            "Save download {Status}",
            success ? "succeeded" : "failed"
        );
    }

    private static string GetSetting(string key) =>
        Environment.GetEnvironmentVariable(key)
        ?? throw new ApplicationException($"{key} is not set");
}
