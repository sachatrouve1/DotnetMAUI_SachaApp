using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SachaApp.Model;
using SachaApp.Services;

namespace SachaApp.ViewModel;

public class Page2ViewModel : INotifyPropertyChanged
{
    private readonly BeerService _beerService;
    private readonly BeerCatalogService _beerCatalogService;
    private Beer? _selectedBeer;
    private string _loadStatusText = "Chargement des bieres...";
    private bool _isLoading;

    public ObservableCollection<Beer> Beers { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    public Page2ViewModel()
        : this(new BeerService(), new BeerCatalogService())
    {
    }

    public Page2ViewModel(BeerService beerService, BeerCatalogService beerCatalogService)
    {
        _beerService = beerService;
        _beerCatalogService = beerCatalogService;
    }

    public string BeerCountText => $"{Beers.Count} elements";

    public string LoadStatusText
    {
        get => _loadStatusText;
        private set
        {
            if (_loadStatusText == value)
            {
                return;
            }

            _loadStatusText = value;
            OnPropertyChanged();
        }
    }

    public Beer? SelectedBeer
    {
        get => _selectedBeer;
        set
        {
            if (ReferenceEquals(_selectedBeer, value))
            {
                return;
            }

            _selectedBeer = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedBeerName));
            OnPropertyChanged(nameof(SelectedBeerImage));
            OnPropertyChanged(nameof(SelectedBeerDescription));
        }
    }

    public string SelectedBeerName => SelectedBeer?.Name ?? "Selectionnez une biere";

    public string SelectedBeerImage => SelectedBeer?.Image ?? "lager.png";

    public string SelectedBeerDescription
    {
        get
        {
            if (SelectedBeer is null)
            {
                return "Touchez un item pour voir le detail.";
            }

            var price = SelectedBeer.Price ?? "n/a";
            if (!string.IsNullOrWhiteSpace(SelectedBeer.Description))
            {
                return SelectedBeer.Description;
            }

            var ratingText = SelectedBeer.Rating is { } rating
                ? $"{rating.Average:0.0}/5 ({rating.Reviews ?? "n/a"})"
                : "n/a";

            return $"Prix: {price} | Note: {ratingText}";
        }
    }

    public async Task LoadAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        LoadStatusText = "Chargement des bieres...";

        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                Beers.Clear();
                SelectedBeer = null;
                LoadStatusText = "Pas d'acces Internet sur l'appareil.";
                OnPropertyChanged(nameof(BeerCountText));
                return;
            }

            var beers = await _beerService.GetBeersAsync();

            Beers.Clear();
            foreach (var beer in beers)
            {
                Beers.Add(beer);
            }

            foreach (var manualBeer in _beerCatalogService.ManualBeers)
            {
                Beers.Add(manualBeer);
            }

            OnPropertyChanged(nameof(BeerCountText));

            SelectedBeer = Beers.FirstOrDefault();
            LoadStatusText = Beers.Count > 0
                ? string.Empty
                : (_beerService.LastError ?? "Aucune biere chargee. Verifie la connexion Internet.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

