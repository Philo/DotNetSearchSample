namespace SearchSample
{
    public interface IHasSorting
    {
        ISortColumnParameter? SortBy { get; set; }
        bool HasSort() => SortBy != null && !string.IsNullOrWhiteSpace(SortBy.Name) && SortBy.Direction.HasValue;

        bool IsSortable<TDataModel>() where TDataModel : class => SortableAttribute.IsSortable<TDataModel>(this);
    }
}