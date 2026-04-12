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

    public void ToggleFavorite(Beer? beer)
    {
        if (beer is null)
        {
            return;
        }

        var currentTag = GetTag(beer);
        _tagsByBeerId[beer.Id] = currentTag == BeerTag.Favorite ? BeerTag.None : BeerTag.Favorite;
    }

    public void ToggleWishlist(Beer? beer)
    {
        if (beer is null)
        {
            return;
        }

        var currentTag = GetTag(beer);
        _tagsByBeerId[beer.Id] = currentTag == BeerTag.Wishlist ? BeerTag.None : BeerTag.Wishlist;
    }
}

