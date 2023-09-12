using System.ComponentModel.DataAnnotations;

namespace SearchSample;

public class SearchParameters: IHasPagination, IHasSorting
{
    public string? Query { get; set; }

    public bool? IsArchived { get; set; }

    [UIHint(nameof(UserStateOption))]
    public string? State { get; set; }

    public IEnumerable<string>? MultiState { get; set; }

    public ISortColumnParameter? SortBy { get; set; } = new SortColumnParameter();

    public IPaginationInfo? Paging { get; set; } = new PaginationInfo();
}