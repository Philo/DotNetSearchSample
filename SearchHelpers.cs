using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace SearchSample;

public static class SearchHelpers
{
    /// <summary>
    /// Build a order by linq expression from a <see cref="IHasSorting" instance/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="source"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, IHasSorting parameters)
    {
        if (parameters.HasSort())
        {
            source = source.OrderBy(parameters!.SortBy!.Name, parameters.SortBy.Direction);
        }
        return source;
    }

    /// <summary>
    /// Provides string based order by function that builds linq expression
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="source"></param>
    /// <param name="orderByProperty"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source,
                                        string? orderByProperty, SortDirectionOption? direction)
    {
        if (string.IsNullOrWhiteSpace(orderByProperty))
        {
            return source;
        }

        string command = direction == SortDirectionOption.Desc ? "OrderByDescending" : "OrderBy";
        var type = typeof(TEntity);
        var property = type.GetProperty(orderByProperty);

        if (property == null)
        {
            return source;
        }

        var parameter = Expression.Parameter(type, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);
        var resultExpression = Expression.Call(typeof(Queryable), command,
                                               new[] { type, property.PropertyType },
                                               source.AsQueryable().Expression,
                                               Expression.Quote(orderByExpression));
        return source.AsQueryable().Provider.CreateQuery<TEntity>(resultExpression);
    }

    /// <summary>
    /// Determines whether the property provided via <paramref name="expression"/> is sortable due to having the <see cref="SortableAttribute"/>
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="htmlHelper"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static bool IsSortableColumn<TModel, T>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, T>> expression)
    {
        return (expression.Body as MemberExpression)?.Member.IsDefined(typeof(SortableAttribute), false) ?? false;
    }

    /// <summary>
    /// Determines if the property provided via <paramref name="expression"/> is set as the sorted column via <paramref name="sortByParameterName"/> and <paramref name="sortDirParameterName"/>
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="htmlHelper"></param>
    /// <param name="sortByParameterName"></param>
    /// <param name="sortDirParameterName"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static bool IsSortedByColumn<TModel, T>(this IHtmlHelper<TModel> htmlHelper,
        string sortByParameterName,
        string sortDirParameterName,
        Expression<Func<TModel, T>> expression)
    {
        if(IsSortableColumn(htmlHelper, expression))
        {
            var colName = htmlHelper.NameFor(expression);
            var qs = QueryHelpers.ParseQuery(htmlHelper.ViewContext.HttpContext.Request.QueryString.ToUriComponent());
            return qs.TryGetValue(sortByParameterName, out var sortedCol) && colName.Equals(sortedCol.FirstOrDefault());
        }
        return false;
    }

    /// <summary>
    /// Appends querystring parameters associated with column based sorting to this <paramref name="queryString"/>.
    /// This intentionally removes the page index parameter provided by <paramref name="pageParameterName"/>
    /// </summary>
    /// <param name="queryString"></param>
    /// <param name="pageParameterName"></param>
    /// <param name="sortByParameterName"></param>
    /// <param name="sortDirParameterName"></param>
    /// <param name="sortCol"></param>
    /// <returns></returns>
    public static IDictionary<string, string> ApplySortingQueryParameterDictionary(
        this QueryString queryString, 
        string pageParameterName, 
        string sortByParameterName, 
        string sortDirParameterName, 
        string? sortCol = null)
    {
        var removeParams = new[] { pageParameterName };
        
        var qs = QueryHelpers.ParseQuery(queryString.ToUriComponent());

        foreach (var p in removeParams)
        {
            qs.Remove(p);
        }

        if (qs == null)
        {
            return new Dictionary<string, string>();
        }

        var sortDir = (qs.TryGetValue(sortByParameterName, out var sortByValue) && sortByValue == sortCol) && qs.TryGetValue(sortDirParameterName, out var sortDirValue) && sortDirValue == "asc" ? "desc" : "asc";

        qs[sortByParameterName] = sortCol;
        qs[sortDirParameterName] = sortDir;

        return qs.ToDictionary(s => s.Key, s => s.Value.ToString());
    }

    /// <summary>
    /// Appends querystring parameters associated with paging actions provided by <paramref name="pageParameterName"/> and <paramref name="pageSizeParameterName"/>
    /// </summary>
    /// <param name="queryString"></param>
    /// <param name="pageParameterName"></param>
    /// <param name="pageSizeParameterName"></param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static IDictionary<string, string> ApplyPagingQueryParameterDictionary(this QueryString queryString, 
        string pageParameterName, string pageSizeParameterName,
        int page, int? size = null)
    {
        var qs = QueryHelpers.ParseQuery(queryString.ToUriComponent());

        if (qs == null)
        {
            return new Dictionary<string, string>();
        }

        qs[pageParameterName] = page.ToString();

        if (size.HasValue)
        {
            qs[pageSizeParameterName] = size.ToString();
        }

        return qs.ToDictionary(s => s.Key, s => s.Value.ToString());
    }

    /// <summary>
    /// Attempts to read a friendly display name from a given enum member otherwise returns the enum name
    /// </summary>
    /// <param name="enumValue">A member of a enum</param>
    /// <returns>The value of the name property on <see cref="DisplayAttribute"/> applied to the member, otherwise the enum nmember name itself</returns>
    public static string GetDisplayNameOrDefault(Enum enumValue)
    {
        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .FirstOrDefault()
                        ?.GetCustomAttribute<DisplayAttribute>()?.Name ?? enumValue.ToString();
    }

    /// <summary>
    /// Create's an array of <see cref="SelectListItem"/> objects from a provided <typeparamref name="TEnum"/>, using the <paramref name="selectedOptions"/> to set selected state
    /// </summary>
    /// <typeparam name="TEnum">An Enum type that represents the options to be provided like the returned array</typeparam>
    /// <param name="selectedOptions">Which of the <typeparamref name="TEnum"/> options are considered selected</param>
    /// <returns>An array of <see cref="SelectListItem"/> created from <typeparamref name="TEnum"/> and <paramref name="selectedOptions"/></returns>
    public static SelectListItem[] CreateOptionsList<TEnum>(IEnumerable<string>? selectedOptions)
        where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>()
            .Select(s => new SelectListItem
            {
                Text = GetDisplayNameOrDefault(s),
                Value = s.ToString(),
                Selected = selectedOptions?.Contains(s.ToString()) ?? false
            })
            .ToArray();
    }
}