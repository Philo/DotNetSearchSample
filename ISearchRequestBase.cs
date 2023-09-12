using MediatR;

namespace SearchSample;

public interface ISearchRequestBase<TParameters, TModel> : IRequest<PaginatedList<TModel>>
    where TParameters : IHasSorting, IHasPagination
{
    TParameters Parameters { get; }
}
