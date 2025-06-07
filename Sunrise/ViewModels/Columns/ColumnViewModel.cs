using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Sunrise.Controls;

namespace Sunrise.ViewModels.Columns;

public abstract class ColumnViewModel : PropertyDescriptor, INotifyPropertyChanged
{
    private string _caption;
    private bool _isVisible = true;
    private bool _isReadOnly;
    private double _width = double.NaN;
    private bool _canUserReorder = true;
    private bool _canUserResize = true;
    private bool _canUserSort = true;
    private IDataTemplate _cellTemplate;
    private IDataTemplate _editTemplate;

    protected ColumnViewModel(string name, bool isReadOnly = true)
        : base(name, null)
        => _isReadOnly = isReadOnly;

    public string Caption
    {
        get => _caption;
        set => SetProperty(ref _caption, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    public bool CanUserReorder
    {
        get => _canUserReorder;
        set => SetProperty(ref _canUserReorder, value);
    }

    public bool CanUserResize
    {
        get => _canUserResize;
        set => SetProperty(ref _canUserResize, value);
    }

    public bool CanUserSort
    {
        get => _canUserSort;
        set => SetProperty(ref _canUserSort, value);
    }

    public IDataTemplate CellTemplate
    {
        get => _cellTemplate;
        set => SetProperty(ref _cellTemplate, value);
    }

    public IDataTemplate EditTemplate
    {
        get => _editTemplate;
        set => SetProperty(ref _editTemplate, value);
    }

    public void ChangeIsReadOnly(bool value) => SetProperty(ref _isReadOnly, value, nameof(IsReadOnly));

    public DataGridColumn CreateDataColumn() => new DataGridControlColumn(this);

    #region PropertyDescriptor

    public override bool IsReadOnly => _isReadOnly;

    public override Type ComponentType => typeof(ColumnViewModel);

    public override bool CanResetValue(object component) => false;

    public override void ResetValue(object component) => throw new NotSupportedException();

    public override void SetValue(object? component, object? value) { }

    public override bool ShouldSerializeValue(object component) => false;

    #endregion
    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(storage, value))
            return false;

        storage = value;

        if (propertyName is not null)
            OnPropertyChanged(propertyName);

        return true;
    }

    #endregion
}
