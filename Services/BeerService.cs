using System.Net.Http.Json;
using SachaApp.Models;

namespace SachaApp.Services;

public class BeerService
{
    private HttpClient httpClient;
    private const string Api = "https://api.sampleapis.com/beers/ale";

    public BeerService()
    {
        httpClient = new HttpClient();
    }

    public async Task<List<Beer>> GetBeerNames()
    {
        try
        {
            var beers = await httpClient.GetFromJsonAsync<List<Beer>>(Api);
            return beers ?? new List<Beer>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Erreur API");
            return new List<Beer>();
        }
    }
}