using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Src.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Src.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FilesController> _logger;
        private const long MaxFileSize = 2 * 1024 * 1024; // 2MB

        public FilesController(
            IFileUploadService fileUploadService,
            IConfiguration configuration,
            ILogger<FilesController> logger)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Uploads an image file to Azure Blob Storage
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <returns>Upload result with URL and metadata</returns>
        [HttpPost("upload")]
        [DisableRequestSizeLimit] // For handling larger files, though we validate size
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                // Validate file exists
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file provided." });
                }

                // Validate file size
                if (file.Length > MaxFileSize)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)}MB."
                    });
                }

                // Validate file type (images only)
                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/jpg" };
                var fileContentType = file.ContentType.ToLower();

                if (!allowedContentTypes.Contains(fileContentType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Only JPG, PNG, and GIF images are allowed."
                    });
                }

                // Validate file extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid file extension. Only .jpg, .jpeg, .png, and .gif are allowed."
                    });
                }

                _logger.LogInformation($"Starting upload for file: {file.FileName}, Size: {file.Length} bytes");

                // Upload file to Azure Blob Storage
                var uploadResult = await _fileUploadService.UploadFileAsync(file);

                if (!uploadResult.Success)
                {
                    _logger.LogError($"Upload failed for {file.FileName}: {uploadResult.ErrorMessage}");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = uploadResult.ErrorMessage ?? "File upload failed."
                    });
                }

                _logger.LogInformation($"File uploaded successfully: {uploadResult.FileName}, URL: {uploadResult.Url}");

                return Ok(new
                {
                    success = true,
                    url = uploadResult.Url,
                    fileName = uploadResult.FileName,
                    fileSize = file.Length,
                    contentType = file.ContentType,
                    uploadedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred during file upload.",
                    detailedError = _configuration.GetValue<bool>("ShowDetailedErrors") ? ex.Message : null
                });
            }
        }

        /// <summary>
        /// Health check endpoint for file upload service
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                maxFileSize = MaxFileSize,
                allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" }
            });
        }
    }
}