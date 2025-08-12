using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio.Services.TreasuryDirect;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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
	    var treasurySecurities = await _treasuryDirectService.GetAuctionedSecurities(timeSinceAuction.Days);

            return Partial("_Auctions", treasurySecurities);
        }
        catch
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Partial("_AuctionsError");
        }
    }

    public async Task<IActionResult> OnGetAnnouncements(DateTime asOf)
    {
	try
	{
	    var timeSinceAnnouncement = DateTime.Today.Subtract(asOf);
	    var treasurySecurities = await _treasuryDirectService.GetAnnouncedSecurities(timeSinceAnnouncement.Days);

	    return Partial("_Announcements", treasurySecurities);
	}
	catch
	{
	    Response.StatusCode = StatusCodes.Status500InternalServerError;
	    return Partial("_AuctionsError");
	}
    }
}
