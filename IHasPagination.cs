namespace SearchSample
{
    public interface IHasPagination
    {
        IPaginationInfo? Paging { get; set; }
    }
}