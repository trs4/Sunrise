using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using Sunrise.Controls;
using Sunrise.ViewModels;
using Sunrise.ViewModels.Columns;

namespace Sunrise.Behaviors;

internal sealed class ColumnsDataGridBehavior : Behavior<DataGrid>
{
    private ObservableCollection<ColumnViewModel>? _columns;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
            AssociatedObject.DataContextChanged += OnDataContextChanged;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
            AssociatedObject.DataContextChanged -= OnDataContextChanged;

        _columns = null;
        base.OnDetaching();
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.DataContext is not MainDesktopViewModel mainViewModel) // %%TODO В интерфейс IDataGridColums
            return;

        var columns = _columns = mainViewModel.TracksColumns;

        if (columns is null)
            return;

        columns.CollectionChanged += OnCollectionChanged;
        dataGrid.Columns.Clear();

        foreach (var column in columns)
            dataGrid.Columns.Add(column.CreateDataColumn());
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
            return;

        if (e.OldItems is not null)
        {
            foreach (object? obj in e.OldItems)
            {
                if (obj is DataGridColumn column)
                    dataGrid.Columns.Remove(column);
                else if (obj is ColumnViewModel columnViewModel)
                {
                    column = dataGrid.Columns.FirstOrDefault(c => ReferenceEquals(c.Header, columnViewModel.Name));

                    if (column is not null)
                    {
                        if (column is DataGridControlColumn dataColumn)
                            dataColumn.OnClose();

                        dataGrid.Columns.Remove(column);
                    }
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (object? obj in e.NewItems)
            {
                if (obj is DataGridColumn column)
                    dataGrid.Columns.Add(column);
                else if (obj is ColumnViewModel columnViewModel)
                    dataGrid.Columns.Add(columnViewModel.CreateDataColumn());
            }
        }
    }

}
