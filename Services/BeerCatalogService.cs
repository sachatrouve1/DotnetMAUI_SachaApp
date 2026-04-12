using System.Collections.ObjectModel;
using SachaApp.Model;

namespace SachaApp.Services;

public class BeerCatalogService
{
    private readonly LocalDatabaseService _databaseService;
    private int _nextManualId = 100000;
    private bool _isLoaded;

    public ObservableCollection<Beer> ManualBeers { get; } = [];

    public BeerCatalogService()
        : this(new LocalDatabaseService())
    {
    }

    public BeerCatalogService(LocalDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task EnsureLoadedAsync()
    {
        if (_isLoaded)
        {
            return;
        }

        var records = await _databaseService.GetManualBeerRecordsAsync();

        ManualBeers.Clear();
        foreach (var record in records)
        {
            ManualBeers.Add(new Beer
            {
                Id = record.Id,
                Name = record.Name,
                Description = record.Description,
                Image = string.IsNullOrWhiteSpace(record.Image) ? null : record.Image,
                Price = record.Price
            });
        }

        var highestId = records.Count > 0 ? records.Max(r => r.Id) : (_nextManualId - 1);
        _nextManualId = Math.Max(_nextManualId, highestId + 1);
        _isLoaded = true;
    }

    public async Task AddManualBeerAsync(string title, string description, string image)
    {
        await EnsureLoadedAsync();

        var manualBeer = new Beer
        {
            Id = _nextManualId++,
            Name = title,
            Description = description,
            Image = string.IsNullOrWhiteSpace(image) ? null : image,
            Price = "Added manually"
        };

        ManualBeers.Insert(0, manualBeer);
        await _databaseService.SaveManualBeerAsync(manualBeer);
    }
}

