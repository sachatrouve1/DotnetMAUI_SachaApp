using System.Text.Json;
using System.Text.Json.Serialization;
using SachaApp.Model;

namespace SachaApp.Services;

public class BeerService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
    private static readonly string[] ApiEndpoints =
    [
        "https://api.sampleapis.com/beers/ale",
        "https://api.sampleapis.com/beers/stouts"
    ];
    private static readonly string[] ArrayPropertyCandidates = ["data", "results", "beers", "items"];
    public string? LastError { get; private set; }

    public BeerService()
    {
        _httpClient = new HttpClient(new SocketsHttpHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });
        _httpClient.Timeout = TimeSpan.FromSeconds(12);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SachaApp/1.0 (+MAUI)");
        _httpClient.DefaultRequestVersion = new Version(1, 1);
    }

    public async Task<List<Beer>> GetBeersAsync()
    {
        const int maxAttempts = 3;
        LastError = null;

        foreach (var endpoint in ApiEndpoints)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var response = await _httpClient.GetAsync(endpoint);
                    if (!response.IsSuccessStatusCode)
                    {
                        LastError = $"API connection failed (HTTP {(int)response.StatusCode}).";
                        if ((int)response.StatusCode >= 500 && attempt < maxAttempts)
                        {
                            await Task.Delay(350 * attempt);
                            continue;
                        }

                        break;
                    }

                    var payload = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(payload);
                    if (!TryGetBeerArray(doc.RootElement, out var beerArray))
                    {
                        if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                            doc.RootElement.TryGetProperty("message", out var messageElement) &&
                            messageElement.ValueKind == JsonValueKind.String)
                        {
                            LastError = messageElement.GetString() ?? "Invalid API response format.";
                        }
                        else
                        {
                            LastError = "Invalid API response format.";
                        }

                        break;
                    }

                    var beers = new List<Beer>();
                    foreach (var item in beerArray.EnumerateArray())
                    {
                        try
                        {
                            var beer = JsonSerializer.Deserialize<Beer>(item.GetRawText(), JsonOptions);
                            if (beer is not null)
                            {
                                beers.Add(beer);
                            }
                        }
                        catch
                        {
                            // Skip malformed entries but keep the valid list from the API.
                        }
                    }

                    if (beers.Count > 0)
                    {
                        return beers;
                    }

                    LastError = "No data received from the API.";
                    break;
                }
                catch (Exception ex)
                {
                    LastError = $"API connection failed: {ex.Message}";
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(350 * attempt);
                        continue;
                    }

                    break;
                }
            }
        }

        return [];
    }

    private static bool TryGetBeerArray(JsonElement rootElement, out JsonElement beerArray)
    {
        if (rootElement.ValueKind == JsonValueKind.Array)
        {
            beerArray = rootElement;
            return true;
        }

        if (rootElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in ArrayPropertyCandidates)
            {
                if (rootElement.TryGetProperty(propertyName, out var candidate) &&
                    candidate.ValueKind == JsonValueKind.Array)
                {
                    beerArray = candidate;
                    return true;
                }
            }

            foreach (var property in rootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    beerArray = property.Value;
                    return true;
                }
            }
        }

        beerArray = default;
        return false;
    }


    public Task<List<Beer>> GetNames() => GetBeersAsync();

    public async Task<List<int>> GetIdsAsync()
    {
        var beers = await GetBeersAsync();
        return beers.Select(b => b.Id).ToList();
    }

    public async Task<List<string>> GetNamesAsync()
    {
        var beers = await GetBeersAsync();
        return beers.Select(b => b.Name ?? string.Empty).ToList();
    }

    public async Task<List<string>> GetPricesAsync()
    {
        var beers = await GetBeersAsync();
        return beers.Select(b => b.Price ?? string.Empty).ToList();
    }

    public async Task<List<string>> GetImagesAsync()
    {
        var beers = await GetBeersAsync();
        return beers.Select(b => b.Image ?? string.Empty).ToList();
    }

    public async Task<List<Rating>> GetRatingsAsync()
    {
        var beers = await GetBeersAsync();
        return beers.Select(b => b.Rating).OfType<Rating>().ToList();
    }
}