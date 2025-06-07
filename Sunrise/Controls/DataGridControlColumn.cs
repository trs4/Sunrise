using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Utils;
using Avalonia.Data;
using Avalonia.Layout;
using Sunrise.ViewModels.Columns;

namespace Sunrise.Controls;

public class DataGridControlColumn : DataGridTemplateColumn
{
    private readonly IDataTemplate _defaultDataTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock
    {
        VerticalAlignment = VerticalAlignment.Center,
        [!TextBlock.TextProperty] = new Binding(nameof(ColumnWrapperViewModel.Value))
    });

    private readonly IDataTemplate _defaultBoolDataTemplate = new FuncDataTemplate<object>((_, _) => new CheckBox
    {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        [!ToggleButton.IsCheckedProperty] = new Binding(nameof(ColumnWrapperViewModel.Value)),
    });

    public DataGridControlColumn()
        => CellTemplate = _defaultDataTemplate;

    public DataGridControlColumn(ColumnViewModel columnViewModel)
    {
        ColumnViewModel = columnViewModel ?? throw new ArgumentNullException(nameof(columnViewModel));
        Header = columnViewModel.Caption;
        Getter = new Func<object?, object?>(columnViewModel.GetValue);
        Setter = new Action<object, object>(columnViewModel.SetValue);

        IsReadOnly = columnViewModel.IsReadOnly;
        IsVisible = columnViewModel.IsVisible;
        CanUserReorder = columnViewModel.CanUserReorder;
        CanUserResize = columnViewModel.CanUserResize;
        CanUserSort = columnViewModel.CanUserSort;

        if (!double.IsNaN(columnViewModel.Width))
            Width = new DataGridLength(columnViewModel.Width);

        if (columnViewModel.CellTemplate is IDataTemplate cellTemplate)
            CellTemplate = cellTemplate;

        if (columnViewModel.EditTemplate is IDataTemplate editTemplate)
            CellEditingTemplate = editTemplate;

        columnViewModel.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ColumnViewModel is null)
            return;

        if (e.PropertyName == nameof(ColumnViewModel.IsVisible))
            IsVisible = ColumnViewModel.IsVisible;
    }

    public void OnClose()
    {
        if (ColumnViewModel is not null)
            ColumnViewModel.PropertyChanged -= OnPropertyChanged;
    }

    public ColumnViewModel? ColumnViewModel { get; }

    public Delegate? Getter { get; }

    public Delegate? Setter { get; }

    protected override Control? GenerateElement(DataGridCell cell, object dataItem)
    {
        var viewModel = new ColumnWrapperViewModel(dataItem, Getter, Setter, ColumnViewModel);

        if (CellTemplate is null && ColumnViewModel is not null)
        {
            if (ColumnViewModel.PropertyType == typeof(bool))
                CellTemplate = _defaultBoolDataTemplate;
        }

        CellTemplate ??= _defaultDataTemplate;

        var control = base.GenerateElement(cell, viewModel.Value);
        control.DataContext = viewModel;
        return control;
    }

    protected override Control GenerateEditingElement(DataGridCell cell, object dataItem, out ICellEditBinding binding)
    {
        var wrapperVm = new ColumnWrapperViewModel(dataItem, Getter, Setter, ColumnViewModel);

        var control = base.GenerateEditingElement(cell, wrapperVm.Value, out binding);
        control.DataContext = wrapperVm;
        return control;
    }

}
