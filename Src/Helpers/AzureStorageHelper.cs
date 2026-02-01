using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Src.Helpers
{
    /// <summary>
    /// Helper class for Azure Storage operations
    /// </summary>
    public static class AzureStorageHelper
    {
        /// <summary>
        /// Gets a BlobContainerClient for the dev-test container
        /// </summary>
        /// <param name="connectionString">Azure Storage connection string</param>
        /// <param name="containerName">Container name (default: dev-test-files)</param>
        /// <returns>Configured BlobContainerClient</returns>
        public static BlobContainerClient GetContainerForDevTest(
            string connectionString,
            string containerName = "dev-test-files")
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Azure Storage connection string is required.", nameof(connectionString));
            }

            try
            {
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure container exists (create if it doesn't)
                containerClient.CreateIfNotExists(PublicAccessType.Blob);

                return containerClient;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to initialize Azure Blob Container. Please check connection string and container name. Error: {ex.Message}",
                    ex);
            }
        }
    }
}