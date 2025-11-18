namespace DocumentManagement.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
        protected DomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public sealed class DocumentNotFoundException : DomainException
    {
        public DocumentNotFoundException(int documentId)
            : base($"Document with ID {documentId} was not found.") { }
    }

    public sealed class UnauthorizedAccessException : DomainException
    {
        public UnauthorizedAccessException(string message) : base(message) { }
    }

    public sealed class InvalidFileException : DomainException
    {
        public InvalidFileException(string message) : base(message) { }
    }

    public sealed class FileSizeExceededException : DomainException
    {
        public FileSizeExceededException(long maxSizeBytes)
            : base($"File size exceeds maximum allowed size of {maxSizeBytes / 1024 / 1024}MB.") { }
    }
}
