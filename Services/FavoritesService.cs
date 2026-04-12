using SachaApp.Model;

namespace SachaApp.Services;

public enum BeerTag
{
    None,
    Favorite,
    Wishlist
}

public class FavoritesService
{
    private readonly Dictionary<int, BeerTag> _tagsByBeerId = [];
    private readonly LocalDatabaseService _databaseService;
    private bool _isLoaded;

    public FavoritesService()
        : this(new LocalDatabaseService())
    {
    }

    public FavoritesService(LocalDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task EnsureLoadedAsync()
    {
        if (_isLoaded)
        {
            return;
        }

        var storedTags = await _databaseService.GetBeerTagsAsync();
        _tagsByBeerId.Clear();

        foreach (var record in storedTags)
        {
            var mappedTag = Enum.IsDefined(typeof(BeerTag), record.TagValue)
                ? (BeerTag)record.TagValue
                : BeerTag.None;
            if (mappedTag != BeerTag.None)
            {
                _tagsByBeerId[record.BeerId] = mappedTag;
            }
        }

        _isLoaded = true;
    }

    public BeerTag GetTag(Beer? beer)
    {
        if (beer is null)
        {
            return BeerTag.None;
        }

        return _tagsByBeerId.TryGetValue(beer.Id, out var tag) ? tag : BeerTag.None;
    }

    public bool IsFavorite(Beer? beer) => GetTag(beer) == BeerTag.Favorite;

    public bool IsWishlist(Beer? beer) => GetTag(beer) == BeerTag.Wishlist;

    public async Task ToggleFavoriteAsync(Beer? beer)
    {
        if (beer is null)
        {
            return;
        }

        await EnsureLoadedAsync();

        var currentTag = GetTag(beer);
        var nextTag = currentTag == BeerTag.Favorite ? BeerTag.None : BeerTag.Favorite;

        if (nextTag == BeerTag.None)
        {
            _tagsByBeerId.Remove(beer.Id);
        }
        else
        {
            _tagsByBeerId[beer.Id] = nextTag;
        }

        await _databaseService.SaveBeerTagAsync(beer.Id, nextTag);
    }

    public async Task ToggleWishlistAsync(Beer? beer)
    {
        if (beer is null)
        {
            return;
        }

        await EnsureLoadedAsync();

        var currentTag = GetTag(beer);
        var nextTag = currentTag == BeerTag.Wishlist ? BeerTag.None : BeerTag.Wishlist;

        if (nextTag == BeerTag.None)
        {
            _tagsByBeerId.Remove(beer.Id);
        }
        else
        {
            _tagsByBeerId[beer.Id] = nextTag;
        }

        await _databaseService.SaveBeerTagAsync(beer.Id, nextTag);
    }
}

