namespace Src.Services
{
    /// <summary>
    /// Interface for file upload services
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Uploads a file to the storage service
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <returns>Upload result containing URL and metadata</returns>
        Task<FileUploadResult> UploadFileAsync(IFormFile file);
    }

    /// <summary>
    /// Result of a file upload operation
    /// </summary>
    public class FileUploadResult
    {
        /// <summary>
        /// Gets or sets whether the upload was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the URL of the uploaded file
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the name of the uploaded file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets an error message if the upload failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the container name where the file is stored
        /// </summary>
        public string ContainerName { get; set; }
    }
}