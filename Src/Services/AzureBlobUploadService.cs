using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Src.Helpers;

namespace Src.Services
{
    /// <summary>
    /// Service for uploading files to Azure Blob Storage
    /// </summary>
    public class AzureBlobUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobUploadService> _logger;
        private readonly BlobContainerClient _containerClient;

        public AzureBlobUploadService(
            IConfiguration configuration,
            ILogger<AzureBlobUploadService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize Azure Blob Container Client
            _containerClient = AzureStorageHelper.GetContainerForDevTest(
                _configuration["AzureStorage:ConnectionString"],
                _configuration["AzureStorage:ContainerName"] ?? "tprofiletest"
            );
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage with conflict checking
        /// </summary>
        public async Task<FileUploadResult> UploadFileAsync(IFormFile file)
        {
            var result = new FileUploadResult();

            try
            {
                // Generate unique filename to avoid conflicts
                var uniqueFileName = GenerateUniqueFileName(file.FileName);
                var blobClient = _containerClient.GetBlobClient(uniqueFileName);

                // Check if blob already exists (conflict checking)
                bool blobExists = await blobClient.ExistsAsync();
                if (blobExists)
                {
                    // Generate a new unique name if conflict exists
                    uniqueFileName = GenerateUniqueFileName(file.FileName, true);
                    blobClient = _containerClient.GetBlobClient(uniqueFileName);
                    _logger.LogWarning($"Filename conflict detected. Using new name: {uniqueFileName}");
                }

                // Upload the file
                using (var stream = file.OpenReadStream())
                {
                    var uploadOptions = new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = file.ContentType
                        }
                    };

                    var response = await blobClient.UploadAsync(stream, uploadOptions);

                    if (response?.Value != null)
                    {
                        result.Success = true;
                        result.Url = blobClient.Uri.ToString();
                        result.FileName = uniqueFileName;
                        result.ContainerName = _containerClient.Name;

                        _logger.LogInformation($"File uploaded successfully: {uniqueFileName}");
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = "Failed to upload file to Azure Blob Storage.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading to Azure Blob Storage: {ex.Message}");
                result.Success = false;
                result.ErrorMessage = $"Azure Blob Storage error: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Generates a unique filename with timestamp and GUID
        /// </summary>
        private string GenerateUniqueFileName(string originalFileName, bool forceUnique = false)
        {
            var fileName = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName);

            // Clean filename: remove invalid characters and limit length
            var cleanFileName = Path.GetInvalidFileNameChars()
                .Aggregate(fileName, (current, c) => current.Replace(c, '-'));

            // Limit filename length
            cleanFileName = cleanFileName.Length > 100
                ? cleanFileName.Substring(0, 100)
                : cleanFileName;

            // Add timestamp and GUID for uniqueness if forced or for general uniqueness
            if (forceUnique)
            {
                return $"{cleanFileName}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            }

            return $"{cleanFileName}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        }
    }
}