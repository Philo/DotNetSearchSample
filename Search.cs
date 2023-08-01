using Bogus;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace SearchSample
{
    public static class Search
    {
        public class Request : IRequest<PaginatedList<DataModel>>
        {
            public Request(SearchParameters searchParameters)
            {
                SearchParameters = searchParameters;
            }

            public SearchParameters SearchParameters { get; }
        }

        private static async Task<IQueryable<DataModel>> GetDataQueryable()
        {
            return await Task.FromResult(DataModel.GetData().AsQueryable());
        }

        private static IQueryable<DataModel> ApplySearchText(IQueryable<DataModel> query, Request request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchParameters.Query))
            {
                query = query.Where(s =>
                        s.GivenName.Contains(request.SearchParameters.Query, StringComparison.InvariantCultureIgnoreCase)
                    ||
                        s.FamilyName.Contains(request.SearchParameters.Query, StringComparison.InvariantCultureIgnoreCase)
                    ||
                        s.EmailAddress.Contains(request.SearchParameters.Query, StringComparison.InvariantCultureIgnoreCase)
                );
            }

            return query;
        }

        private static IQueryable<DataModel> ApplyFilters(IQueryable<DataModel> query, Request request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchParameters.State) && Enum.TryParse<UserStateOption>(request.SearchParameters.State, out var state))
            {
                query = query.Where(s => s.State == state);
            }

            if (request.SearchParameters.IsArchived.HasValue)
            {
                query = query.Where(s => s.IsArchived == request.SearchParameters.IsArchived);
            }

            return query;
        }

        private static IQueryable<DataModel> ApplySorting(IQueryable<DataModel> query, Request request)
        {
            if (request.SearchParameters.SortBy != null && request.SearchParameters.SortBy.Name != null && request.SearchParameters.SortBy.Direction != null)
            {
                //var validSortColumns = new[] { 
                //    nameof(DataModel.GivenName), 
                //    nameof(DataModel.FamilyName), 
                //    nameof(DataModel.EmailAddress), 
                //    nameof(DataModel.State), 
                //    nameof(DataModel.IsArchived) 
                //};

                var validSortColumns = SortableAttribute.GetSortableColumnNames<DataModel>();

                var validSortDirections = new[]
                {
                    "asc", "desc"
                };

                if(validSortColumns.Contains(request.SearchParameters.SortBy.Name) && validSortDirections.Contains(request.SearchParameters.SortBy.Direction))
                {
                    query = query.OrderBy(request.SearchParameters.SortBy.Name, request.SearchParameters.SortBy.Direction?.ToLowerInvariant() == "desc");
                }
            }

            return query;
        }

        public class Handler : IRequestHandler<Request, PaginatedList<DataModel>>
        {
            public async Task<PaginatedList<DataModel>> Handle(Request request, CancellationToken cancellationToken)
            {
                var query = await GetDataQueryable();

                if (request.SearchParameters == null)
                {
                    return PaginatedList<DataModel>.Create(Enumerable.Empty<DataModel>().AsQueryable(), 1, PaginationInfo.DefaultPageSize);
                }

                query = ApplySearchText(query, request);
                query = ApplyFilters(query, request);
                query = ApplySorting(query, request);

               return PaginatedList<DataModel>.Create(query, request.SearchParameters.Paging?.Page ?? 1, request.SearchParameters.Paging?.Size ?? PaginationInfo.DefaultPageSize);
            }
        }

        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source,
                                            string orderByProperty, bool desc)
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty);

            if(property == null)
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
    }

    public class DataModel
    {
        [DataType(DataType.Text), Sortable]
        public string GivenName { get; set; } = string.Empty;

        [DataType(DataType.Text), Sortable]
        public string FamilyName { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress), Sortable]
        public string EmailAddress { get; set; } = string.Empty;

        public bool IsArchived { get; set; } = false;

        public UserStateOption State { get; set; }

        private static readonly Faker<DataModel> Faker = new Faker<DataModel>()
            .RuleFor(m => m.GivenName, f => f.Person.FirstName)
            .RuleFor(m => m.FamilyName, f => f.Person.LastName)
            .RuleFor(m => m.EmailAddress, f => f.Person.Email)
            .RuleFor(m => m.IsArchived, f => f.Random.Bool())
            .RuleFor(m => m.State, f => f.Random.Enum<UserStateOption>());

        private static readonly IEnumerable<DataModel> FakeData = Faker.Generate(25);

        public static IEnumerable<DataModel> GetData() => FakeData;
    }

    public enum UserStateOption
    {
        Active,
        Complete,
        Cancelled
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SortableAttribute : Attribute
    {
        public SortableAttribute() { }

        public static IEnumerable<string> GetSortableColumnNames<TData>() where TData : class
        {
            return typeof(TData).GetProperties().Where(p => p.IsDefined(typeof(SortableAttribute), false)).Select(p => p.Name);
        }
    }
}