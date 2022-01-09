namespace StateR.Blazor.WebStorage;

public interface IWebStorageSerializer
{
    string Serialize<TValue>(TValue keyValue);
    TValue? Deserialize<TValue>(string keyValue);
    ValueTask<string> SerializeAsync<TValue>(TValue keyValue, CancellationToken cancellationToken);
    ValueTask<TValue?> DeserializeAsync<TValue>(string keyValue, CancellationToken cancellationToken);
}
