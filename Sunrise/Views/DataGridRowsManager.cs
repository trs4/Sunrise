using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Utils;

namespace Sunrise.Views;

internal static class DataGridRowsManager
{
    private const BindingFlags _internal = BindingFlags.Instance ^ BindingFlags.NonPublic;
    private static readonly PropertyInfo _dataConnectionProperty;
    private static readonly PropertyInfo _dataConnectionListProperty;

    static DataGridRowsManager()
    {
        _dataConnectionProperty = typeof(DataGrid).GetProperty("DataConnection", _internal) ?? throw new ArgumentNullException();
        _dataConnectionListProperty = _dataConnectionProperty.PropertyType.GetProperty("List") ?? throw new ArgumentNullException();
    }

    public static T? GetFirstRow<T>(Window window)
        where T : ObservableObject
        => GetDataSource(window)?.OfType<T>().FirstOrDefault();

    public static T? GetLastRow<T>(Window window)
        where T : ObservableObject
        => GetDataSource(window)?.OfType<T>().LastOrDefault();

    public static T? GetPrevRow<T>(Window window, T? row)
        where T : ObservableObject
    {
        if (row is null)
            return null;

        var list = GetDataSource(window);
        return GetPrev(list, row);
    }

    public static T? GetNextRow<T>(Window window, T? row)
        where T : ObservableObject
    {
        if (row is null)
            return null;

        var list = GetDataSource(window);
        return GetNext(list, row);
    }

    public static T? GetPrev<T>(IList? list, T? row)
        where T : class
    {
        if (list is null)
            return null;

        int index = list.IndexOf(row) - 1;
        return index >= 0 && index < list.Count ? list[index] as T : null;
    }

    public static T? GetNext<T>(IList? list, T? row)
        where T : class
    {
        if (list is null)
            return null;

        int index = list.IndexOf(row) + 1;
        return index >= 0 && index < list.Count ? list[index] as T : null;
    }

    private static IList? GetDataSource(Window window)
    {
        var dataGrid = UIDispatcher.Run(() => window.FindControl<DataGrid>("tracksGrid"));

        if (dataGrid is null)
            return null;

        object dataConnection = _dataConnectionProperty.GetValue(dataGrid);
        return _dataConnectionListProperty.GetValue(dataConnection) as IList;
    }

}
