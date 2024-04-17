using System.Collections.Concurrent;
using System.IO;
using Avalonia.Svg.Skia;
using Sunrise.Model.Resources;

namespace Sunrise.Utils;

public static class IconSource
{
    private static readonly ConcurrentDictionary<string, SvgImage> _icons = [];

    public static SvgImage From(string name) => _icons.GetOrAdd(name, Load);

    private static SvgImage Load(string name)
    {
        var bytes = (byte[])Icons.ResourceManager.GetObject(name);
        using var stream = new MemoryStream(bytes);
        return new SvgImage() { Source = SvgSource.LoadFromStream(stream) };
    }

}
