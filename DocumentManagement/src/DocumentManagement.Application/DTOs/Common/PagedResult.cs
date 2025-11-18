namespace DocumentManagement.Application.Common
{
    public record PagedResult<T>(IEnumerable<T> Items, int PageNumber, int PageSize, int TotalCount, int TotalPages)
    {
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }

}
