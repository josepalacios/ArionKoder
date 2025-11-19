using AutoMapper;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Implementations;
using DocumentManagement.Domain.Enums;
using DocumentManagement.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace DocumentManagement.UnitTests.Services
{
    public class DocumentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IValidator<UploadDocumentRequest>> _uploadValidatorMock;
        private readonly Mock<IValidator<UpdateDocumentRequest>> _updateValidatorMock;
        private readonly DocumentService _sut; // System Under Test

        public DocumentServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _fileStorageMock = new Mock<IFileStorageService>();
            _mapperMock = new Mock<IMapper>();
            _uploadValidatorMock = new Mock<IValidator<UploadDocumentRequest>>();
            _updateValidatorMock = new Mock<IValidator<UpdateDocumentRequest>>();

            _sut = new DocumentService(
                _unitOfWorkMock.Object,
                _fileStorageMock.Object,
                _mapperMock.Object,
                _uploadValidatorMock.Object,
                _updateValidatorMock.Object);
        }

        [Fact]
        public async Task UploadDocumentAsync_WithValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new UploadDocumentRequest(
                "Test Document",
                "Test Description",
                new[] { "test", "document" },
                AccessType.Private);

            var stream = new MemoryStream();
            var userEmail = "test@example.com";

            _uploadValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _fileStorageMock
                .Setup(f => f.SaveFileAsync(
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync("2024/01/test-file.pdf");

            var documentMock = new Mock<IDocumentRepository>();
            _unitOfWorkMock.Setup(u => u.Documents).Returns(documentMock.Object);

            var tagMock = new Mock<ITagRepository>();
            _unitOfWorkMock.Setup(u => u.Tags).Returns(tagMock.Object);

            tagMock.Setup(t => t.GetOrCreateTagsAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Tag>
                {
                new() { Id = 1, Name = "test" },
                new() { Id = 2, Name = "document" }
                });

            // Act
            var result = await _sut.UploadDocumentAsync(
                request,
                stream,
                "test.pdf",
                "application/pdf",
                1024,
                userEmail);

            // Assert
            Assert.True(result.IsSuccess);
            _fileStorageMock.Verify(f => f.SaveFileAsync(
                It.IsAny<Stream>(),
                "test.pdf",
                "application/pdf",
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadDocumentAsync_WithInvalidFileType_ReturnsFailure()
        {
            // Arrange
            var request = new UploadDocumentRequest(
                "Test Document",
                null,
                null,
                AccessType.Private);

            var stream = new MemoryStream();

            _uploadValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _sut.UploadDocumentAsync(
                request,
                stream,
                "test.exe",
                "application/x-msdownload",
                1024,
                "test@example.com");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid file type", result.Error);
        }

        [Theory]
        [InlineData(10485761)] // 10MB + 1 byte
        [InlineData(20971520)] // 20MB
        public async Task UploadDocumentAsync_WithOversizedFile_ReturnsFailure(long fileSize)
        {
            // Arrange
            var request = new UploadDocumentRequest(
                "Large File",
                null,
                null,
                AccessType.Private);

            var stream = new MemoryStream();

            _uploadValidatorMock
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _sut.UploadDocumentAsync(
                request,
                stream,
                "large.pdf",
                "application/pdf",
                fileSize,
                "test@example.com");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("File size cannot exceed", result.Error);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_WithValidIdAndAccess_ReturnsDocument()
        {
            // Arrange
            var documentId = 1;
            var userEmail = "test@example.com";
            var userRole = UserRole.Contributor;

            var document = new Document
            {
                Id = documentId,
                Title = "Test Document",
                FileName = "test.pdf",
                StoragePath = "2024/01/test.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 1024,
                UploadedBy = userEmail,
                AccessType = AccessType.Private,
                DocumentTags = new List<DocumentTag>(),
                DocumentShares = new List<DocumentShare>()
            };

            var documentMock = new Mock<IDocumentRepository>();
            _unitOfWorkMock.Setup(u => u.Documents).Returns(documentMock.Object);

            documentMock
                .Setup(r => r.HasAccessAsync(documentId, userEmail, userRole, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            documentMock
                .Setup(r => r.GetByIdWithDetailsAsync(documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(document);

            var expectedResponse = new DocumentDetailResponse(
                documentId,
                "Test Document",
                null,
                "test.pdf",
                "application/pdf",
                1024,
                userEmail,
                AccessType.Private,
                Array.Empty<string>(),
                Array.Empty<DocumentShareResponse>(),
                DateTime.UtcNow,
                null);

            _mapperMock
                .Setup(m => m.Map<DocumentDetailResponse>(document))
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetDocumentByIdAsync(documentId, userEmail, userRole);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(documentId, result.Data.Id);
            Assert.Equal("Test Document", result.Data.Title);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_WithoutAccess_ReturnsUnauthorized()
        {
            // Arrange
            var documentId = 1;
            var userEmail = "test@example.com";
            var userRole = UserRole.Viewer;

            var documentMock = new Mock<IDocumentRepository>();
            _unitOfWorkMock.Setup(u => u.Documents).Returns(documentMock.Object);

            documentMock
                .Setup(r => r.HasAccessAsync(documentId, userEmail, userRole, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.GetDocumentByIdAsync(documentId, userEmail, userRole);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(403, result.StatusCode);
            Assert.Contains("permission", result.Error);
        }

        [Fact]
        public async Task DeleteDocumentAsync_WithWriteAccess_DeletesSuccessfully()
        {
            // Arrange
            var documentId = 1;
            var userEmail = "test@example.com";
            var userRole = UserRole.Contributor;

            var document = new Document
            {
                Id = documentId,
                Title = "Test Document",
                FileName = "test.pdf",
                StoragePath = "2024/01/test.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = 1024,
                UploadedBy = userEmail,
                AccessType = AccessType.Private
            };

            var documentMock = new Mock<IDocumentRepository>();
            _unitOfWorkMock.Setup(u => u.Documents).Returns(documentMock.Object);

            documentMock
                .Setup(r => r.HasWriteAccessAsync(documentId, userEmail, userRole, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            documentMock
                .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(document);

            // Act
            var result = await _sut.DeleteDocumentAsync(documentId, userEmail, userRole);

            // Assert
            Assert.True(result.IsSuccess);
            documentMock.Verify(r => r.DeleteAsync(document, It.IsAny<CancellationToken>()), Times.Once);
            _fileStorageMock.Verify(f => f.DeleteFileAsync(document.StoragePath, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
