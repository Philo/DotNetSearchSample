using MediatR;

namespace SearchSample;

public abstract class SearchBaseHandler<TRequest, TParameters, TModel> : IRequestHandler<TRequest, PaginatedList<TModel>>
    where TParameters : IHasSorting, IHasPagination
    where TRequest : ISearchRequestBase<TParameters, TModel>
{
    protected abstract Task<IQueryable<TModel>> GetDataQueryable(CancellationToken cancellationToken);

    protected virtual IQueryable<TModel> ApplySorting(IQueryable<TModel> query, IHasSorting parameters)
    {
        return query;
    }

    protected virtual IQueryable<TModel> ApplySearchText(IQueryable<TModel> query, TParameters parameters)
    {
        return query;
    }

    protected virtual IQueryable<TModel> ApplyFilters(IQueryable<TModel> query, TParameters parameters)
    {
        return query;
    }

    public async Task<PaginatedList<TModel>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var query = await GetDataQueryable(cancellationToken);

        if (request.Parameters == null)
        {
            return PaginatedList<TModel>.Create(Enumerable.Empty<TModel>().AsQueryable(), 1, PaginationInfo.DefaultPageSize);
        }

        query = ApplySearchText(query, request.Parameters);
        query = ApplyFilters(query, request.Parameters);
        query = ApplySorting(query, request.Parameters);

        return PaginatedList<TModel>.Create(query, request.Parameters.Paging?.Page ?? 1, request.Parameters.Paging?.Size ?? PaginationInfo.DefaultPageSize);

    }
}
