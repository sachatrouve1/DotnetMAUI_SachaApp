using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SachaApp.Model;
using SachaApp.Services;

namespace SachaApp.ViewModel;

public class LabViewModel : INotifyPropertyChanged
{
    private readonly BeerService _beerService;
    private readonly BeerCatalogService _beerCatalogService;
    private readonly FavoritesService _favoritesService;
    private string _selectedFilter = "All";
    private bool _isLoading;
    private string _loadStatusText = "Loading beers...";
    private Beer? _selectedBeerA;
    private Beer? _selectedBeerB;
    private Beer? _selectedListBeer;

    public ObservableCollection<Beer> AllBeers { get; } = [];
    public ObservableCollection<Beer> FilteredBeers { get; } = [];

    public IReadOnlyList<string> FilterOptions { get; } = ["All", "Favorites", "Wishlist"];

    public ICommand ToggleFavoriteCommand { get; }
    public ICommand ToggleWishlistCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public LabViewModel(BeerService beerService, BeerCatalogService beerCatalogService, FavoritesService favoritesService)
    {
        _beerService = beerService;
        _beerCatalogService = beerCatalogService;
        _favoritesService = favoritesService;

        ToggleFavoriteCommand = new Command(ToggleFavoriteForSelected);
        ToggleWishlistCommand = new Command(ToggleWishlistForSelected);
    }

    public string SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (_selectedFilter == value)
            {
                return;
            }

            _selectedFilter = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

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

    public Beer? SelectedBeerA
    {
        get => _selectedBeerA;
        set
        {
            if (ReferenceEquals(_selectedBeerA, value))
            {
                return;
            }

            _selectedBeerA = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CompareNameA));
            OnPropertyChanged(nameof(CompareDetailsA));
            OnPropertyChanged(nameof(CompareImageA));
        }
    }

    public Beer? SelectedBeerB
    {
        get => _selectedBeerB;
        set
        {
            if (ReferenceEquals(_selectedBeerB, value))
            {
                return;
            }

            _selectedBeerB = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CompareNameB));
            OnPropertyChanged(nameof(CompareDetailsB));
            OnPropertyChanged(nameof(CompareImageB));
        }
    }

    public Beer? SelectedListBeer
    {
        get => _selectedListBeer;
        set
        {
            if (ReferenceEquals(_selectedListBeer, value))
            {
                return;
            }

            _selectedListBeer = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedListBeerName));
            OnPropertyChanged(nameof(SelectedListBeerTagText));
        }
    }

    public string SelectedListBeerName => SelectedListBeer?.Name ?? "Select a beer";

    public string SelectedListBeerTagText
    {
        get
        {
            if (SelectedListBeer is null)
            {
                return "Select a beer to mark as favorite or wishlist.";
            }

            return _favoritesService.GetTag(SelectedListBeer) switch
            {
                BeerTag.Favorite => "Status: Favorite",
                BeerTag.Wishlist => "Status: Wishlist",
                _ => "Status: None"
            };
        }
    }

    public string CompareNameA => SelectedBeerA?.Name ?? "Beer A";
    public string CompareNameB => SelectedBeerB?.Name ?? "Beer B";
    public string CompareImageA => SelectedBeerA?.Image ?? "lager.png";
    public string CompareImageB => SelectedBeerB?.Image ?? "lager.png";
    public string CompareDetailsA => FormatBeerDetails(SelectedBeerA);
    public string CompareDetailsB => FormatBeerDetails(SelectedBeerB);

    public async Task LoadAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        LoadStatusText = "Loading beers...";

        try
        {
            var apiBeers = new List<Beer>();
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                apiBeers = await _beerService.GetBeersAsync();
            }

            AllBeers.Clear();
            foreach (var beer in apiBeers)
            {
                AllBeers.Add(beer);
            }

            foreach (var manualBeer in _beerCatalogService.ManualBeers)
            {
                AllBeers.Add(manualBeer);
            }

            ApplyFilter();

            SelectedBeerA ??= AllBeers.FirstOrDefault();
            SelectedBeerB ??= AllBeers.Skip(1).FirstOrDefault() ?? SelectedBeerA;
            SelectedListBeer = FilteredBeers.FirstOrDefault();

            LoadStatusText = AllBeers.Count > 0
                ? string.Empty
                : (_beerService.LastError ?? "No beers available.");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ToggleFavoriteForSelected()
    {
        if (SelectedListBeer is null)
        {
            return;
        }

        _favoritesService.ToggleFavorite(SelectedListBeer);
        OnPropertyChanged(nameof(SelectedListBeerTagText));
        ApplyFilter();
    }

    private void ToggleWishlistForSelected()
    {
        if (SelectedListBeer is null)
        {
            return;
        }

        _favoritesService.ToggleWishlist(SelectedListBeer);
        OnPropertyChanged(nameof(SelectedListBeerTagText));
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var beers = SelectedFilter switch
        {
            "Favorites" => AllBeers.Where(_favoritesService.IsFavorite),
            "Wishlist" => AllBeers.Where(_favoritesService.IsWishlist),
            _ => AllBeers
        };

        FilteredBeers.Clear();
        foreach (var beer in beers)
        {
            FilteredBeers.Add(beer);
        }

        if (SelectedListBeer is not null && !FilteredBeers.Contains(SelectedListBeer))
        {
            SelectedListBeer = FilteredBeers.FirstOrDefault();
        }
    }

    private static string FormatBeerDetails(Beer? beer)
    {
        if (beer is null)
        {
            return "Select a beer to compare.";
        }

        if (!string.IsNullOrWhiteSpace(beer.Description))
        {
            return beer.Description;
        }

        var price = beer.Price ?? "n/a";
        var ratingText = beer.Rating is { } rating
            ? $"{rating.Average:0.0}/5 ({rating.Reviews ?? "n/a"})"
            : "n/a";

        return $"Price: {price} | Rating: {ratingText}";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

