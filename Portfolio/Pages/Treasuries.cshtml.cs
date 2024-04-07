using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Portfolio.Pages;

public class TreasuriesModel : PageModel
{
    private readonly TreasuryDirectService _treasuryDirectService;

    [BindProperty, Required]
    public string Cusip { get; set; }

    [BindProperty, Required]
    public DateTime IssueDate { get; set; }

    public TreasuryDirectSecurityIssuanceResult? Treasury { get; private set; }

    public TreasuriesModel(IHttpClientFactory httpClientFactory)
    {
        _treasuryDirectService = new TreasuryDirectService(httpClientFactory);

        Cusip = "912797JV0";
        IssueDate = new DateTime(2024, 04, 09);
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _treasuryDirectService.GetSecurity(Cusip, IssueDate);

        Treasury = result;

        return Partial("_TreasuriesTable", Treasury);
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
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<TreasuryDirectSecurityIssuanceResult?> GetSecurity(string cusip, DateTime issueDate)
    {
        var url = $"https://www.treasurydirect.gov/TA_WS/securities/{cusip}/{issueDate:MM/dd/yyyy}?format=json";

        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            using (var contentStream = await response.Content.ReadAsStreamAsync())
            {
                return await JsonSerializer.DeserializeAsync<TreasuryDirectSecurityIssuanceResult>(
                    contentStream,
                    _options);
            }
        }

        // TODO: return status code
        throw new Exception();
    }
}

public class TreasuryDirectSecurityIssuanceResult
{
    public string? Cusip { get; set; }

    public DateTime? IssueDate { get; set; }

    public string? SecurityType { get; set; }
}

public enum TreasuryDirectSecurityType
{
    Undefined = 0,
    Bill = 1,
    Note = 2,
    Bond = 3
}

#endregion Treasury Direct Service
