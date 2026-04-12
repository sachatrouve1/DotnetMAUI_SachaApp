using SachaApp.Model;
using SQLite;

namespace SachaApp.Services;

public class LocalDatabaseService
{
    private readonly SQLiteAsyncConnection _connection;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public LocalDatabaseService()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "sachaapp.db3");
        _connection = new SQLiteAsyncConnection(databasePath);
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            await _connection.CreateTableAsync<ManualBeerRecord>();
            await _connection.CreateTableAsync<BeerTagRecord>();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<List<ManualBeerRecord>> GetManualBeerRecordsAsync()
    {
        await InitializeAsync();
        return await _connection.Table<ManualBeerRecord>().OrderByDescending(b => b.Id).ToListAsync();
    }

    public async Task SaveManualBeerAsync(Beer beer)
    {
        await InitializeAsync();

        await _connection.InsertOrReplaceAsync(new ManualBeerRecord
        {
            Id = beer.Id,
            Name = beer.Name ?? string.Empty,
            Description = beer.Description ?? string.Empty,
            Image = beer.Image ?? string.Empty,
            Price = beer.Price ?? string.Empty
        });
    }

    public async Task<List<BeerTagRecord>> GetBeerTagsAsync()
    {
        await InitializeAsync();
        return await _connection.Table<BeerTagRecord>().ToListAsync();
    }

    public async Task SaveBeerTagAsync(int beerId, BeerTag tag)
    {
        await InitializeAsync();

        if (tag == BeerTag.None)
        {
            await _connection.DeleteAsync<BeerTagRecord>(beerId);
            return;
        }

        await _connection.InsertOrReplaceAsync(new BeerTagRecord
        {
            BeerId = beerId,
            TagValue = (int)tag
        });
    }
}

