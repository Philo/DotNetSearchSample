using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SearchSample.Pages
{
    public class LocationSearchModel : PageModel
    {
        private readonly IMediator mediator;
        private readonly ILogger<LocationSearchModel> _logger;

        [BindProperty(SupportsGet = true)]
        public LocationSearch.Parameters? Search { get; set; }

        public PaginatedList<LocationDataModel>? Data { get; set; }

        public LocationSearchModel(IMediator mediator, ILogger<LocationSearchModel> logger)
        {
            this.mediator = mediator;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            if(Search != null)
            {
                Data = await mediator.Send(new LocationSearch.Request(Search));
            }
        }
    }
}