using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Services.TreasuryDirect;

namespace Portfolio.Pages.Treasuries;

public class TreasuriesModel : PageModel
{
    private readonly TreasuryDirectService _treasuryDirectService;

    public TreasuriesModel(IHttpClientFactory httpClientFactory)
    {
        _treasuryDirectService = new TreasuryDirectService(httpClientFactory);
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnGetAuctions(DateTime asOf)
    {
        try
        {
            var timeSinceAuction = DateTime.Today.Subtract(asOf);
	    var treasuryDirectSecurities = await _treasuryDirectService.GetAuctionedSecurities(timeSinceAuction.Days);

            return Partial("_Auctions", treasuryDirectSecurities);
        }
        catch
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Partial("_AuctionsError");
        }
    }
}
