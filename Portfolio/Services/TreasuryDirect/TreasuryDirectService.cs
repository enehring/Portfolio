using System.Text.Json.Serialization;
using System.Text.Json;
using Portfolio.Services.TreasuryDirect.Models;

namespace Portfolio.Services.TreasuryDirect;

// TODO: The Treasury Direct URI should probably be factored out (ie. hostname, route, etc.) as this grows.
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
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                
                // Handle cases when TreasuryDirect sends an empty string instead of null for a number
                new EmptyStringToNullDecimalConverter()
            }
        };
    }

    public async Task<TreasuryDirectSecurityIssuance?> GetSecurityDetails(string cusip, DateTime issueDate)
    {
        var url = $"https://www.treasurydirect.gov/TA_WS/securities/{cusip}/{issueDate:MM/dd/yyyy}?format=json";
        var response = await SendTreasuryDirectGetRequest(url);
        var deserializedResponse = JsonSerializer.Deserialize<TreasuryDirectSecurityIssuance>(
            response,
            _options);

        return deserializedResponse;
    }

    public async Task<TreasuryDirectSecurityIssuance[]> GetAuctionedSecurities(int daysAgo)
    {
        //var timeSinceAuction = DateTime.Today.Subtract(asOf);
        var url = $"https://www.treasurydirect.gov/TA_WS/securities/auctioned?days={daysAgo}&format=json";
        var response = await SendTreasuryDirectGetRequest(url);
        var deserializedResponse = JsonSerializer.Deserialize<TreasuryDirectSecurityIssuance[]>(
            response,
            _options);

        return deserializedResponse ?? Array.Empty<TreasuryDirectSecurityIssuance>();
    }

    private async Task<string> SendTreasuryDirectGetRequest(string url)
    {
        var client = _httpClientFactory.CreateClient();

        try
        {
            var response = await client.GetStringAsync(url);

            // TreasuryDirect returns a 200 status code with "No data" in the body if no security matching the
            // request is found
            if (response == "No data")
            {
                return string.Empty;
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            // TODO: Do something better
            Console.WriteLine($"GET {url} failed. Status Code: {ex.StatusCode}");

            throw;
        }
    }

    private class EmptyStringToNullDecimalConverter : JsonConverter<decimal?>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(decimal?);
        }

        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return string.IsNullOrEmpty(value) ? null : decimal.Parse(value);
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            // TreasuryDirect is only ever read from
            throw new NotImplementedException();
        }
    }
}
