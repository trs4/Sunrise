using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public class CategoryViewModel : ObservableObject
{
    private string _name;
    private bool _isChecked;
    private bool _editing;

    public CategoryViewModel(Category category)
    {
        Category = category ?? throw new ArgumentNullException(nameof(category));
        _name = category.Name;
    }

    public CategoryViewModel(Category category, bool isChecked)
        : this(category)
        => _isChecked = isChecked;

    public Category Category { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    public bool Editing
    {
        get => _editing;
        set => SetProperty(ref _editing, value);
    }

    public override string ToString() => _name;
}
