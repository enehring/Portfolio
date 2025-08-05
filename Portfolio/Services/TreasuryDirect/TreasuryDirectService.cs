using System.Text.Json.Serialization;
using System.Text.Json;
using Portfolio.Models.Treasuries;

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

    public async Task<TreasurySecurity?> GetSecurityDetails(string cusip, DateTime issueDate)
    {
        var url = $"https://www.treasurydirect.gov/TA_WS/securities/{cusip}/{issueDate:MM/dd/yyyy}?format=json";
        var response = await SendTreasuryDirectGetRequest(url);
        var deserializedResponse = JsonSerializer.Deserialize<TreasurySecurity>(
            response,
            _options);

        return deserializedResponse;
    }

    public async Task<TreasurySecurity[]> GetAuctionedSecurities(int daysAgo)
    {
        var url = $"https://www.treasurydirect.gov/TA_WS/securities/auctioned?days={daysAgo}&format=json";
        var response = await SendTreasuryDirectGetRequest(url);
        var deserializedResponse = JsonSerializer.Deserialize<TreasurySecurity[]>(
            response,
            _options);

        return deserializedResponse ?? Array.Empty<TreasurySecurity>();
    }

    public async Task<TreasurySecurity[]> GetAnnouncedSecurities(int daysAgo)
    {
        var url = $"https://www.treasurydirect.gov/TA_WS/securities/announced?days={daysAgo}&format=json";
        var response = await SendTreasuryDirectGetRequest(url);
        var deserializedResponse = JsonSerializer.Deserialize<TreasurySecurity[]>(
            response,
            _options);
        
        return deserializedResponse ?? Array.Empty<TreasurySecurity>();
    }

    private async Task<string> SendTreasuryDirectGetRequest(string url)
    {
        var client = _httpClientFactory.CreateClient();

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            // TreasuryDirect returns a 200 status code with "No data" in the body if no security matching the
            // request is found
            if (body == "No data")
            {
                return string.Empty;
            }

            return body;
        }
        catch (HttpRequestException ex)
        {
            var msg = $"GET request failed.\n" +
                $"\tURL: {url}\n" +
                $"\tStatus Code: {(int?)ex.StatusCode} ({ex.StatusCode})";

            // TODO: Use a logger
            Console.WriteLine(msg);

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
