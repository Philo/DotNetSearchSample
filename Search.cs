using Bogus;
using MediatR;
using System.ComponentModel.DataAnnotations;

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
            if (request.SearchParameters.SortBy != null)
            {
                // Of course this isn't a good way to do sorting, for demo purposes
                switch (request.SearchParameters.SortBy.Name?.ToLowerInvariant())
                {
                    case "givenname":
                        query = request.SearchParameters.SortBy.Direction == "asc" ? query.OrderBy(s => s.GivenName) : query.OrderByDescending(s => s.GivenName);
                        break;
                    case "familyname":
                        query = request.SearchParameters.SortBy.Direction == "asc" ? query.OrderBy(s => s.FamilyName) : query.OrderByDescending(s => s.FamilyName);
                        break;
                    case "emailaddress":
                        query = request.SearchParameters.SortBy.Direction == "asc" ? query.OrderBy(s => s.EmailAddress) : query.OrderByDescending(s => s.EmailAddress);
                        break;
                    case "state":
                        query = request.SearchParameters.SortBy.Direction == "asc" ? query.OrderBy(s => s.State) : query.OrderByDescending(s => s.State);
                        break;
                    case "isarchived":
                        query = request.SearchParameters.SortBy.Direction == "asc" ? query.OrderBy(s => s.IsArchived) : query.OrderByDescending(s => s.IsArchived);
                        break;
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
    }

    public class DataModel
    {
        [DataType(DataType.Text)]
        public string GivenName { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        public string FamilyName { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress)]
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
}