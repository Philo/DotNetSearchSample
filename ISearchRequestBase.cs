using MediatR;

namespace SearchSample;

public interface ISearchRequestBase<TParameters, TModel> : IRequest<PaginatedList<TModel>>
    where TParameters : IHasSorting, IHasPagination
    where TModel : class
{
    TParameters Parameters { get; }
}
