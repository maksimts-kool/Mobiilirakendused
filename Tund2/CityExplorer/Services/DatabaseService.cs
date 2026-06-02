using Microsoft.Data.Sqlite;
using Tund2.CityExplorer.Models;

namespace Tund2.CityExplorer.Services;

public class DatabaseService
{
    private const string DatabaseFileName = "cityexplorer.db3";

    private readonly string databasePath;
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private bool databaseIsReady;

    public DatabaseService()
    {
        SQLitePCL.Batteries_V2.Init();
        databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, DatabaseFileName);
    }

    public async Task InitializeAsync()
    {
        if (databaseIsReady)
        {
            return;
        }

        await initializationLock.WaitAsync();

        try
        {
            if (databaseIsReady)
            {
                return;
            }

            Directory.CreateDirectory(FileSystem.Current.AppDataDirectory);

            await using var connection = await OpenConnectionAsync(ensureInitialized: false);
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                CREATE TABLE IF NOT EXISTS FavoritePlaces
                (
                    Id INTEGER PRIMARY KEY,
                    CategoryKey TEXT NOT NULL,
                    Image TEXT NOT NULL,
                    NameKey TEXT NOT NULL,
                    ShortDescriptionKey TEXT NOT NULL,
                    DetailKey TEXT NOT NULL
                );
                """;

            await command.ExecuteNonQueryAsync();
            databaseIsReady = true;
        }
        finally
        {
            initializationLock.Release();
        }
    }

    public async Task<List<Place>> GetFavoritesAsync()
    {
        var favorites = new List<Place>();

        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, CategoryKey, Image, NameKey, ShortDescriptionKey, DetailKey
            FROM FavoritePlaces
            ORDER BY Id;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            favorites.Add(ReadFavorite(reader));
        }

        return favorites;
    }

    public async Task<HashSet<int>> GetFavoriteIdsAsync()
    {
        var favoriteIds = new HashSet<int>();

        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id
            FROM FavoritePlaces
            ORDER BY Id;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            favoriteIds.Add(reader.GetInt32(0));
        }

        return favoriteIds;
    }

    public async Task<bool> FavoriteExistsAsync(int placeId)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM FavoritePlaces WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", placeId);

        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);
        return count > 0;
    }

    public async Task SaveFavoriteAsync(Place place)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR REPLACE INTO FavoritePlaces
                (Id, CategoryKey, Image, NameKey, ShortDescriptionKey, DetailKey)
            VALUES
                ($id, $categoryKey, $image, $nameKey, $shortDescriptionKey, $detailKey);
            """;

        AddPlaceParameters(command, place);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteFavoriteAsync(int placeId)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM FavoritePlaces WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", placeId);

        await command.ExecuteNonQueryAsync();
    }

    private async Task<SqliteConnection> OpenConnectionAsync(bool ensureInitialized = true)
    {
        if (ensureInitialized)
        {
            await InitializeAsync();
        }

        var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        return connection;
    }

    private static Place ReadFavorite(SqliteDataReader reader)
    {
        var id = reader.GetInt32(0);

        return new Place
        {
            Id = id,
            CategoryKey = reader.GetString(1),
            Image = GetCurrentImageName(id, reader.GetString(2)),
            NameKey = reader.GetString(3),
            ShortDescriptionKey = reader.GetString(4),
            DetailKey = reader.GetString(5),
            IsFavorite = true
        };
    }

    private static void AddPlaceParameters(SqliteCommand command, Place place)
    {
        command.Parameters.AddWithValue("$id", place.Id);
        command.Parameters.AddWithValue("$categoryKey", place.CategoryKey);
        command.Parameters.AddWithValue("$image", place.Image);
        command.Parameters.AddWithValue("$nameKey", place.NameKey);
        command.Parameters.AddWithValue("$shortDescriptionKey", place.ShortDescriptionKey);
        command.Parameters.AddWithValue("$detailKey", place.DetailKey);
    }

    private static string GetCurrentImageName(int placeId, string savedImage)
    {
        return CityExplorerCatalog.GetCurrentImageName(placeId, savedImage);
    }
}
