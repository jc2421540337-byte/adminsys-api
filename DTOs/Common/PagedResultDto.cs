namespace NewAdminSystem.Api.DTOs.Common
{
    public class PagedResultDto<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }

        public List<T> Items { get; set; } = new();
    }
}
