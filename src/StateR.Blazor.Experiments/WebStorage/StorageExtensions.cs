using System.Text.Json;

namespace StateR.Blazor.WebStorage;

public static class StorageExtensions
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public static void SetItem<T>(this IStorage storage, string keyName, T keyValue)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(storage, nameof(storage));
        var value = JsonSerializer.Serialize(keyValue, _options);
        storage.SetItem(keyName, value);
    }

    public static T? GetItem<T>(this IStorage storage, string keyName)
    {
        ArgumentNullException.ThrowIfNull(storage, nameof(storage));
        var rawValue = storage.GetItem(keyName);
        if( rawValue == null)
        {
            return default;
        }
        var value = JsonSerializer.Deserialize<T>(rawValue, _options);
        return value;
    }

    public static async ValueTask SetItemAsync<T>(this IStorage storage, string keyName, T keyValue, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(storage, nameof(storage));
        var value = JsonSerializer.Serialize(keyValue, _options);
        await storage.SetItemAsync(keyName, value, cancellationToken);
    }

    public static async ValueTask<T?> GetItemAsync<T>(this IStorage storage, string keyName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(storage, nameof(storage));
        var rawValue = await storage.GetItemAsync(keyName, cancellationToken);
        if (rawValue == null)
        {
            return default;
        }
        var value = JsonSerializer.Deserialize<T>(rawValue, _options);
        return value;
    }
}