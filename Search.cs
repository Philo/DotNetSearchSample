using Bogus;
using System.ComponentModel.DataAnnotations;

namespace SearchSample;

public static class Search
{
    public class Request : ISearchRequestBase<SearchParameters, DataModel>
    {
        public Request(SearchParameters searchParameters)
        {
            Parameters = searchParameters;
        }

        public SearchParameters Parameters { get; }
    }

    public class Handler : SearchBaseHandler<Request, SearchParameters, DataModel>
    {
        protected override async Task<IQueryable<DataModel>> GetDataQueryable(CancellationToken cancellationToken)
        {
            return await Task.FromResult(DataModel.GetData().AsQueryable());
        }

        protected override IQueryable<DataModel> ApplySearchText(IQueryable<DataModel> query, SearchParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.Query))
            {
                query = query.Where(s =>
                        s.GivenName.Contains(parameters.Query, StringComparison.InvariantCultureIgnoreCase)
                    ||
                        s.FamilyName.Contains(parameters.Query, StringComparison.InvariantCultureIgnoreCase)
                    ||
                        s.EmailAddress.Contains(parameters.Query, StringComparison.InvariantCultureIgnoreCase)
                );
            }

            return query;
        }

        protected override IQueryable<DataModel> ApplyFilters(IQueryable<DataModel> query, SearchParameters parameters)
        {
            static IEnumerable<TEnum> ToEnumList<TEnum>(IEnumerable<string> options) where TEnum : struct
            {
                foreach (var option in options)
                {
                    if (Enum.TryParse<TEnum>(option, out var enumOption))
                    {
                        yield return enumOption;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(parameters.State) && Enum.TryParse<UserStateOption>(parameters.State, out var state))
            {
                query = query.Where(s => s.State == state);
            }

            if (parameters.MultiState?.Any() ?? false)
            {
                var states = ToEnumList<UserStateOption>(parameters.MultiState);
                query = query.Where(s => states.Contains(s.State));
            }

            if (parameters.IsArchived.HasValue)
            {
                query = query.Where(s => s.IsArchived == parameters.IsArchived);
            }

            return query;
        }

        protected override IQueryable<DataModel> ApplySorting(IQueryable<DataModel> query, IHasSorting parameters)
        {
            if (parameters.HasSort() && parameters.IsSortable<DataModel>())
            {
                query = query.OrderBy(parameters);
            }

            return query;
        }
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

    private static readonly IEnumerable<DataModel> FakeData = Faker.Generate(250);

    public static IEnumerable<DataModel> GetData() => FakeData;
}

public enum UserStateOption
{
    [Display(Name = "Is Active")]
    Active,
    [Display(Name = "Is Complete")]
    Complete,
    [Display(Name = "Is Cancelled")]
    Cancelled
}