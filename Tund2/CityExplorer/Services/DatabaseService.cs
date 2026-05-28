using Microsoft.Data.Sqlite;
using Tund2.CityExplorer.Models;

namespace Tund2.CityExplorer.Services;

public class DatabaseService
{
    private readonly string databasePath;
    private bool databaseIsReady;

    public DatabaseService()
    {
        SQLitePCL.Batteries_V2.Init();
        databasePath = Path.Combine(FileSystem.Current.AppDataDirectory, "cityexplorer.db3");
    }

    public async Task InitializeAsync()
    {
        if (databaseIsReady)
        {
            return;
        }

        Directory.CreateDirectory(FileSystem.Current.AppDataDirectory);

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

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

    public async Task<List<Place>> GetFavoritesAsync()
    {
        await InitializeAsync();

        var favorites = new List<Place>();

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

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
            favorites.Add(new Place
            {
                Id = reader.GetInt32(0),
                CategoryKey = reader.GetString(1),
                Image = GetCurrentImageName(reader.GetInt32(0), reader.GetString(2)),
                NameKey = reader.GetString(3),
                ShortDescriptionKey = reader.GetString(4),
                DetailKey = reader.GetString(5),
                IsFavorite = true
            });
        }

        return favorites;
    }

    public async Task<bool> FavoriteExistsAsync(int placeId)
    {
        await InitializeAsync();

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM FavoritePlaces WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", placeId);

        var count = (long)(await command.ExecuteScalarAsync() ?? 0L);
        return count > 0;
    }

    public async Task SaveFavoriteAsync(Place place)
    {
        await InitializeAsync();

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR REPLACE INTO FavoritePlaces
                (Id, CategoryKey, Image, NameKey, ShortDescriptionKey, DetailKey)
            VALUES
                ($id, $categoryKey, $image, $nameKey, $shortDescriptionKey, $detailKey);
            """;

        command.Parameters.AddWithValue("$id", place.Id);
        command.Parameters.AddWithValue("$categoryKey", place.CategoryKey);
        command.Parameters.AddWithValue("$image", place.Image);
        command.Parameters.AddWithValue("$nameKey", place.NameKey);
        command.Parameters.AddWithValue("$shortDescriptionKey", place.ShortDescriptionKey);
        command.Parameters.AddWithValue("$detailKey", place.DetailKey);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteFavoriteAsync(int placeId)
    {
        await InitializeAsync();

        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM FavoritePlaces WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", placeId);

        await command.ExecuteNonQueryAsync();
    }

    private static string GetCurrentImageName(int placeId, string savedImage)
    {
        return placeId switch
        {
            1 => "cityexplorer_toompea.jpg",
            2 => "cityexplorer_oldtown.jpg",
            3 => "cityexplorer_kadriorg.jpg",
            4 => "cityexplorer_pirita.jpg",
            5 => "cityexplorer_market.jpg",
            6 => "cityexplorer_telliskivi.jpg",
            _ => savedImage
        };
    }
}
