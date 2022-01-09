using Microsoft.Extensions.Options;
using System.Text.Json;

namespace StateR.Blazor.WebStorage;

public class JsonWebStorageSerializerOptions
{
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
}

public class ConfigureJsonWebStorageSerializerOptions : IConfigureOptions<JsonWebStorageSerializerOptions>
{
    public void Configure(JsonWebStorageSerializerOptions options)
    {
        if (options.JsonSerializerOptions == null)
        {
            options.JsonSerializerOptions = new(JsonSerializerDefaults.Web);
        }
    }
}
