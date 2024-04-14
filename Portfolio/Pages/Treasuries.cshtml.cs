using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Portfolio.Pages;

public class TreasuriesModel : PageModel
{
    private readonly TreasuryDirectService _treasuryDirectService;

    [BindProperty, Required]
    public string Cusip { get; set; }

    [BindProperty, Required]
    public DateTime IssueDate { get; set; }

    public TreasuriesModel(IHttpClientFactory httpClientFactory)
    {
        _treasuryDirectService = new TreasuryDirectService(httpClientFactory);

        Cusip = string.Empty;
        IssueDate = DateTime.Today;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Get previous week's announced securities
        var treasury = await _treasuryDirectService.GetAnnouncedSecurities(DateTime.Today.AddDays(-7));

        return Partial("_TreasuriesTable", treasury);
    }
}

#region Treasury Direct Service

public class TreasuryDirectService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _options;

    public TreasuryDirectService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    public async Task<TreasuryDirectSecurityIssuanceResult?> GetSecurity(string cusip, DateTime issueDate)
    {
        var url = $"https://www.treasurydirect.gov/TA_WS/securities/{cusip}/{issueDate:MM/dd/yyyy}?format=json";
        var response = await GetTreasuryDirectJsonData(url);

        // TreasuryDirect will return a 200 status code with "No data" in the body if no security matching the
        // request is found
        if (response == "No data")
        {
            return null;
        }

        return JsonSerializer.Deserialize<TreasuryDirectSecurityIssuanceResult>(response, _options);
    }

    public async Task<IEnumerable<TreasuryDirectSecurityIssuanceResult>?> GetAnnouncedSecurities(DateTime announceDate)
    {
        var daysSinceAnnouncement = DateTime.Today.Subtract(announceDate.Date).Days;
        var url = $"https://www.treasurydirect.gov/TA_WS/securities/announced?days={daysSinceAnnouncement}&format=json";
        var response = await GetTreasuryDirectJsonData(url);

        // TreasuryDirect will return a 200 status code with "No data" in the body if no security matching the
        // request is found
        if (response == "No data")
        {
            return null;
        }

        return JsonSerializer.Deserialize<IEnumerable<TreasuryDirectSecurityIssuanceResult>>(response, _options);
    }

    private async Task<string> GetTreasuryDirectJsonData(string url)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        using (var response = await client.SendAsync(request))
        {
            return await response.Content.ReadAsStringAsync();
        }
    }
}

public class TreasuryDirectSecurityIssuanceResult
{
    public string? Cusip { get; set; }

    public DateTime? IssueDate { get; set; }

    public double? OfferingAmount { get; set; }

    public TreasuryDirectSecurityType? SecurityType { get; set; }

    public string? SecurityTerm { get; set; }
}

public enum TreasuryDirectSecurityType
{
    Undefined = 0,
    Bill = 1,
    Note = 2,
    Bond = 3
}

#endregion Treasury Direct Service
