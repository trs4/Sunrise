using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;
using Sunrise.Utils;

namespace Sunrise.Behaviors;

internal sealed class RowDoubleClickDataGridBehavior : Behavior<DataGrid>
{
    public static readonly DirectProperty<RowDoubleClickDataGridBehavior, ICommand?> DoubleClickCommandProperty
        = AvaloniaProperty.RegisterDirect<RowDoubleClickDataGridBehavior, ICommand?>(
            nameof(DoubleClickCommand), o => o.DoubleClickCommand, (o, v) => o.DoubleClickCommand = v);

    private ICommand? _doubleClickCommand;

    public ICommand? DoubleClickCommand
    {
        get => _doubleClickCommand;
        set => SetAndRaise(DoubleClickCommandProperty, ref _doubleClickCommand, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
            AssociatedObject.DoubleTapped += OnDoubleTapped;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
            AssociatedObject.DoubleTapped -= OnDoubleTapped;

        base.OnDetaching();
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        AssociatedObject?.CommitEdit();
        var command = DoubleClickCommand;

        if (command is null || e.Source is not Control control)
            return;

        var cell = control.FindLogicalAncestorOfType<DataGridCell>();

        if (cell is null)
            return;

        object parameter = cell.DataContext;

        if (parameter is null)
            return;

        UIDispatcher.Run(() =>
        {
            if (command.CanExecute(parameter))
                command.Execute(parameter);
        });
    }

}
