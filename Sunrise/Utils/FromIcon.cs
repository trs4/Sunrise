using System;
using Avalonia.Markup.Xaml;

namespace Sunrise.Utils;

public sealed class FromIcon : MarkupExtension
{
    public FromIcon(string name)
        => Name = name;

    public string Name { get; }

    public override object ProvideValue(IServiceProvider serviceProvider) => IconSource.From(Name);
}
