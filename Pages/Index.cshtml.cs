using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SearchSample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMediator mediator;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty(SupportsGet = true)]
        public SearchParameters? Search { get; set; }

        public PaginatedList<DataModel>? Data { get; set; }

        public IndexModel(IMediator mediator, ILogger<IndexModel> logger)
        {
            this.mediator = mediator;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            if(Search != null)
            {
                Data = await mediator.Send(new Search.Request(Search));
            }
        }
    }
}