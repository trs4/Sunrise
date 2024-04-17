using System;
using Avalonia.Threading;

namespace Sunrise.Utils;

internal static class UIDispatcher
{
    public static void Run(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
        {
            var operation = Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Normal);
            operation.Wait();
        }
    }

    public static T Run<T>(Func<T> action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            return action();

        var operation = Dispatcher.UIThread.InvokeAsync(action, DispatcherPriority.Normal);
        operation.Wait();
        return operation.Result;
    }

}
