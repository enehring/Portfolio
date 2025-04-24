using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Pages.Treasuries.TreasuryDirect;

namespace Portfolio.Pages.Treasuries
{
    public class DetailsModel : PageModel
    {
        private readonly TreasuryDirectService _treasuryDirectService;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _treasuryDirectService = new TreasuryDirectService(httpClientFactory);
        }

        public void OnGet(string cusip, DateTime issueDate)
        {
            var result = _treasuryDirectService.GetSecurityDetails(cusip, issueDate);
        }
    }
}
