public sealed class Tag : BaseEntity
{
    public required string Name { get; set; }

    // Navigation properties
    public ICollection<DocumentTag> DocumentTags { get; set; } = [];
}
