using CommunityToolkit.Mvvm.ComponentModel;

namespace Sunrise.ViewModels.Categories;

public class CategoryViewModel : ObservableObject
{
    public CategoryViewModel(byte[] icon, string name)
    {
        Icon = icon;
        Name = name;
    }

    public byte[] Icon { get; }

    public string Name { get; }

    public override string ToString() => Name;
}
