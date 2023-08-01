using System.ComponentModel.DataAnnotations;

namespace SearchSample
{
    public class SearchParameters
    {
        public string? Query { get; set; }

        public bool? IsArchived { get; set; }

        [UIHint(nameof(UserStateOption))]
        public string? State { get; set; }

        public SortColumnParameter? SortBy { get; set; }

        public PaginationInfo? Paging { get; set; }
    }
}