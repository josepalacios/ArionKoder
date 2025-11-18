using DocumentManagement.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DocumentManagement.Infrastructure.Services
{
    public sealed class LocalFileStorageService : IFileStorageService
    {
        private readonly string _storageBasePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _storageBasePath = configuration["FileStorage:BasePath"]
                ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            // Ensure directory exists
            if (!Directory.Exists(_storageBasePath))
            {
                Directory.CreateDirectory(_storageBasePath);
            }
        }

        public async Task<string> SaveFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            // Generate unique file path
            var uniqueFileName = $"{Guid.NewGuid()}_{SanitizeFileName(fileName)}";
            var relativePath = Path.Combine(
                DateTime.UtcNow.Year.ToString(),
                DateTime.UtcNow.Month.ToString("D2"),
                uniqueFileName);

            var fullPath = Path.Combine(_storageBasePath, relativePath);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStreamOutput = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920, // 80KB buffer
                useAsync: true);

            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

            return relativePath;
        }

        public async Task<Stream> GetFileStreamAsync(
            string storagePath,
            CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_storageBasePath, storagePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found.", storagePath);
            }

            var memoryStream = new MemoryStream();
            await using var fileStream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            await fileStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public Task DeleteFileAsync(
            string storagePath,
            CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_storageBasePath, storagePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        public Task<bool> FileExistsAsync(
            string storagePath,
            CancellationToken cancellationToken = default)
        {
            var fullPath = Path.Combine(_storageBasePath, storagePath);
            return Task.FromResult(File.Exists(fullPath));
        }

        private static string SanitizeFileName(string fileName)
        {
            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Limit length
            if (sanitized.Length > 200)
            {
                var extension = Path.GetExtension(sanitized);
                sanitized = sanitized[..(200 - extension.Length)] + extension;
            }

            return sanitized;
        }
    }
}
