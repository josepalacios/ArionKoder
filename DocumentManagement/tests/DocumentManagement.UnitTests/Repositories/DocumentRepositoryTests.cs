using DocumentManagement.Domain.Enums;
using DocumentManagement.Infrastructure.Data.Context;
using DocumentManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.UnitTests.Repositories;

public sealed class DocumentRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DocumentRepository _repository;

    public DocumentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new DocumentRepository(_context);
    }

    [Fact]
    public async Task GetPagedAsync_WithDocuments_ReturnsPagedResults()
    {
        // Arrange
        var documents = new List<Document>
        {
            new() { Title = "Doc 1", FileName = "doc1.pdf", StoragePath = "path1", ContentType = "application/pdf", FileSizeBytes = 1024, UploadedBy = "user@test.com", AccessType = AccessType.Public },
            new() { Title = "Doc 2", FileName = "doc2.pdf", StoragePath = "path2", ContentType = "application/pdf", FileSizeBytes = 1024, UploadedBy = "user@test.com", AccessType = AccessType.Public },
            new() { Title = "Doc 3", FileName = "doc3.pdf", StoragePath = "path3", ContentType = "application/pdf", FileSizeBytes = 1024, UploadedBy = "user@test.com", AccessType = AccessType.Public }
        };

        await _context.Documents.AddRangeAsync(documents);
        await _context.SaveChangesAsync();

        // Act
        var (results, totalCount) = await _repository.GetPagedAsync(
            1, 2, null, "user@test.com", UserRole.Admin);

        // Assert
        Assert.Equal(3, totalCount);
        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task HasAccessAsync_AdminRole_ReturnsTrue()
    {
        // Arrange
        var document = new Document
        {
            Title = "Private Doc",
            FileName = "doc.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
            FileSizeBytes = 1024,
            UploadedBy = "owner@test.com",
            AccessType = AccessType.Private
        };

        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        var hasAccess = await _repository.HasAccessAsync(
            document.Id, "admin@test.com", UserRole.Admin);

        // Assert
        Assert.True(hasAccess);
    }

    [Fact]
    public async Task HasAccessAsync_ViewerWithPrivateDoc_ReturnsFalse()
    {
        // Arrange
        var document = new Document
        {
            Title = "Private Doc",
            FileName = "doc.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
            FileSizeBytes = 1024,
            UploadedBy = "owner@test.com",
            AccessType = AccessType.Private
        };

        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        // Act
        var hasAccess = await _repository.HasAccessAsync(
            document.Id, "viewer@test.com", UserRole.Viewer);

        // Assert
        Assert.False(hasAccess);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}