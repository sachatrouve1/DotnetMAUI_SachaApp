using System.Collections.ObjectModel;
using SachaApp.Model;

namespace SachaApp.Services;

public class BeerCatalogService
{
    private int _nextManualId = 100000;

    public ObservableCollection<Beer> ManualBeers { get; } = [];

    public void AddManualBeer(string title, string description, string image)
    {
        ManualBeers.Insert(0, new Beer
        {
            Id = _nextManualId++,
            Name = title,
            Description = description,
            Image = string.IsNullOrWhiteSpace(image) ? "lager.png" : image,
            Price = "Ajoutee manuellement"
        });
    }
}

