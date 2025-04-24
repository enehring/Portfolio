using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Services.TreasuryDirect;

namespace Portfolio.Pages.Treasuries
{
    public class DetailsModel : PageModel
    {
        private readonly TreasuryDirectService _treasuryDirectService;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _treasuryDirectService = new TreasuryDirectService(httpClientFactory);
        }

        public async Task OnGet(string cusip, DateTime issueDate)
        {
            var result = await _treasuryDirectService.GetSecurityDetails(cusip, issueDate);
        }
    }
}
