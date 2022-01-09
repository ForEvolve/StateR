using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace StateR.Blazor.WebStorage;

public class WebStorageOptions
{
    public StorageType DefaultStorageType { get; set; } = StorageType.Local;

    [NotNull]
    public IWebStorageSerializer? Serializer { get; internal set; }
}

public class PostConfigureWebStorageOptions : IPostConfigureOptions<WebStorageOptions>
{
    private readonly IWebStorageSerializer _webStorageSerializer;
    public PostConfigureWebStorageOptions(IWebStorageSerializer webStorageSerializer)
    {
        _webStorageSerializer = webStorageSerializer ?? throw new ArgumentNullException(nameof(webStorageSerializer));
    }

    public void PostConfigure(string name, WebStorageOptions options)
    {
        options.Serializer = _webStorageSerializer;
        StorageExtensions.WebStorageOptions = options;
    }
}