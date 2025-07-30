using Avalonia.Svg.Skia;
using Sunrise.Model.Resources;
using System.Collections.Concurrent;
using System.Text;

namespace Sunrise.Utils;

public static class IconSource
{
    private static readonly ConcurrentDictionary<string, SvgImage> _icons = [];

    public static SvgImage From(string name) => _icons.GetOrAdd(name, Load);

    private static SvgImage Load(string name)
    {
        var bytes = (byte[])Icons.ResourceManager.GetObject(name);
        string svg = Encoding.UTF8.GetString(bytes);
        return new SvgImage() { Source = SvgSource.LoadFromSvg(svg) };
    }

}
