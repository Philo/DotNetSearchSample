using Bogus;
using System.ComponentModel.DataAnnotations;

namespace SearchSample;

public static class LocationSearch
{
    public enum PopulationBucketOption
    {
        [Display(Name = "Up to 100K")]
        LessThan100K,
        [Display(Name = "Up to 500K")]
        LessThan500K,
        [Display(Name = "500K+")]
        MoreThan500K,
        [Display(Name = "1m+")]
        MoreThan1M
    }

    public class Parameters : IHasSorting, IHasPagination
    {
        public string? Query { get; set; }

        public PopulationBucketOption? Population { get; set; }

        public ISortColumnParameter? SortBy { get; set; } = new SortColumnParameter();
        public IPaginationInfo? Paging { get; set; } = new PaginationInfo();
    }

    public class Request : ISearchRequestBase<Parameters, LocationDataModel>
    {
        public Request(Parameters searchParameters)
        {
            Parameters = searchParameters;
        }

        public Parameters Parameters { get; }
    }

    public class Handler : SearchBaseHandler<Request, Parameters, LocationDataModel>
    {
        protected override async Task<IQueryable<LocationDataModel>> GetDataQueryable(CancellationToken cancellationToken)
        {
            return await Task.FromResult(LocationDataModel.GetData().AsQueryable());
        }

        protected override IQueryable<LocationDataModel> ApplySearchText(IQueryable<LocationDataModel> query, Parameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.Query))
            {
                query = query.Where(s => s.Name.Contains(parameters.Query, StringComparison.InvariantCultureIgnoreCase));
            }

            return query;
        }

        protected override IQueryable<LocationDataModel> ApplyFilters(IQueryable<LocationDataModel> query, Parameters parameters)
        {
            if(parameters.Population.HasValue)
            {
                switch(parameters.Population.Value)
                {
                    case PopulationBucketOption.LessThan100K:
                        query = query.Where(e => e.Population < 100000);
                        break;
                    case PopulationBucketOption.LessThan500K:
                        query = query.Where(e => e.Population < 500000);
                        break;
                    case PopulationBucketOption.MoreThan500K:
                        query = query.Where(e => e.Population >= 500000);
                        break;
                    case PopulationBucketOption.MoreThan1M:
                        query = query.Where(e => e.Population >= 1000000);
                        break;
                }
            }

            return query;
        }

        //protected override IQueryable<LocationDataModel> ApplySorting(IQueryable<LocationDataModel> query, IHasSorting parameters)
        //{
        //    if (parameters.HasSort() && parameters.IsSortable<LocationDataModel>())
        //    {
        //        query = query.OrderBy(parameters);
        //    }

        //    return query;
        //}
    }
}

public class LocationDataModel
{
    [DataType(DataType.Text), Sortable]
    public string Name { get; set; } = string.Empty;

    [DataType(DataType.Text), Sortable]
    public string Country { get; set; } = string.Empty;

    [Sortable, DisplayFormat(DataFormatString = "{0:N0}")]
    public int? Population { get; set; }

    private static readonly Faker<LocationDataModel> Faker = new Faker<LocationDataModel>()
        .RuleFor(m => m.Name, f => f.Address.StreetName())
        .RuleFor(m => m.Country, f => f.Address.Country())
        .RuleFor(m => m.Population, f => f.Random.Number(100000, 10000000));

    private static readonly IEnumerable<LocationDataModel> FakeData = Faker.Generate(250);

    public static IEnumerable<LocationDataModel> GetData() => FakeData;
}