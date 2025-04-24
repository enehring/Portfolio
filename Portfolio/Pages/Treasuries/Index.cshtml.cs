using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Pages.Treasuries.TreasuryDirect;

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
        var treasuryDirectSecurities = await _treasuryDirectService.GetAuctionedSecurities(asOf);

        return Partial("_TreasuriesTable", treasuryDirectSecurities);
    }
}
