using System.Globalization;

namespace SearchSample;

[AttributeUsage(AttributeTargets.Property)]
public class SortableAttribute : Attribute
{
    public SortableAttribute() { }

    public static IEnumerable<string> GetSortableColumnNames<TData>() where TData : class
    {
        return typeof(TData).GetProperties().Where(p => p.IsDefined(typeof(SortableAttribute), false)).Select(p => p.Name);
    }

    public static bool IsSortable<TDataModel>(IHasSorting parameters) where TDataModel : class
    {
        if (!parameters.HasSort())
        {
            return false;
        }

        var validSortColumns = GetSortableColumnNames<TDataModel>();
        return validSortColumns.Contains(parameters.SortBy!.Name);
    }
}