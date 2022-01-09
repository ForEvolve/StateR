using System.Text.Json;

namespace StateR.Blazor.WebStorage;

public sealed class JsonWebStorageSerializer : IWebStorageSerializer
{
    private readonly JsonWebStorageSerializerOptions _options;
    public JsonWebStorageSerializer(JsonWebStorageSerializerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public TValue? Deserialize<TValue>(string keyValue)
    {
        var value = JsonSerializer.Deserialize<TValue>(keyValue, _options.JsonSerializerOptions);
        return value;
    }

    public ValueTask<TValue?> DeserializeAsync<TValue>(string keyValue, CancellationToken cancellationToken)
    {
        var value = Deserialize<TValue>(keyValue);
        return new(value);
    }

    public string Serialize<TValue>(TValue keyValue)
    {
        var value = JsonSerializer.Serialize(keyValue, _options.JsonSerializerOptions);
        return value;
    }

    public ValueTask<string> SerializeAsync<TValue>(TValue keyValue, CancellationToken cancellationToken)
    {
        var value = Serialize(keyValue);
        return new(value);
    }
}
