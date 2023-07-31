using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace SearchSample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        [UIHint(nameof(SearchParameters))]
        [BindProperty(SupportsGet = true)]
        public SearchParameters? Search { get; set; }

        public PaginatedList<DataModel>? Data { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            var query = DataModel.GetData().AsQueryable();

            if (Search != null)
            {

                if (!string.IsNullOrWhiteSpace(Search.Query))
                {
                    query = query.Where(s => 
                            s.GivenName.Contains(Search.Query, StringComparison.InvariantCultureIgnoreCase) 
                        || 
                            s.FamilyName.Contains(Search.Query, StringComparison.InvariantCultureIgnoreCase) 
                        || 
                            s.EmailAddress.Contains(Search.Query, StringComparison.InvariantCultureIgnoreCase)
                    );
                }

                if(Search.State.HasValue)
                {
                    query = query.Where(s => s.State == Search.State);
                }

                if (Search.IsArchived.HasValue)
                {
                    query = query.Where(s => s.IsArchived == Search.IsArchived);
                }

                if (Search.SortBy != null)
                {
                    // Of course this isn't a good way to do sorting, for demo purposes
                    switch (Search.SortBy.Name?.ToLowerInvariant())
                    {
                        case "givenname":
                            query = Search.SortBy.Direction == "asc" ? query.OrderBy(s => s.GivenName) : query.OrderByDescending(s => s.GivenName);
                            break;
                        case "familyname":
                            query = Search.SortBy.Direction == "asc" ? query.OrderBy(s => s.FamilyName) : query.OrderByDescending(s => s.FamilyName);
                            break;
                        case "emailaddress":
                            query = Search.SortBy.Direction == "asc" ? query.OrderBy(s => s.EmailAddress) : query.OrderByDescending(s => s.EmailAddress);
                            break;
                        case "state":
                            query = Search.SortBy.Direction == "asc" ? query.OrderBy(s => s.State) : query.OrderByDescending(s => s.State);
                            break;
                        case "isarchived":
                            query = Search.SortBy.Direction == "asc" ? query.OrderBy(s => s.IsArchived) : query.OrderByDescending(s => s.IsArchived);
                            break;
                    }
                }

                var pagedList = PaginatedList<DataModel>.Create(query, Search.Paging?.Page ?? 1, Search.Paging?.Size ?? PaginationInfo.DefaultPageSize);

                Data = pagedList;
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

        public EnumOptionParameter State { get; set; }

        private static readonly Faker<DataModel> Faker = new Faker<DataModel>()
            .RuleFor(m => m.GivenName, f => f.Person.FirstName)
            .RuleFor(m => m.FamilyName, f => f.Person.LastName)
            .RuleFor(m => m.EmailAddress, f => f.Person.Email)
            .RuleFor(m => m.IsArchived, f => f.Random.Bool())
            .RuleFor(m => m.State, f => f.Random.Enum<EnumOptionParameter>());

        private static readonly IEnumerable<DataModel> FakeData = Faker.Generate(25);

        public static IEnumerable<DataModel> GetData() => FakeData;
    }

    public enum EnumOptionParameter
    {
        Active,
        Complete,
        Cancelled
    }

    public class PaginationInfo
    {
        public const int DefaultPageSize = 10;
        private int pageSize = DefaultPageSize;

        public int Page { get; set; } = 1;
        public int Size
        {
            get => pageSize;
            set
            {
                if (value < 0 || value > 50)
                {
                    pageSize = DefaultPageSize;
                }
                else
                {
                    pageSize = value;
                }
            }
        }
    }

    public class SortColumnParameter
    {
        [HiddenInput(DisplayValue = false)]
        public string? Name { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string? Direction { get; set; }
    }

    public class SearchParameters
    {
        public string? Query { get; set; }

        public bool? IsArchived { get; set; }

        public EnumOptionParameter? State { get; set; }

        public SortColumnParameter? SortBy { get; set; }

        public PaginationInfo? Paging { get; set; }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        private PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public static PaginatedList<T> Create(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}