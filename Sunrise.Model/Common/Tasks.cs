using System.Runtime.ExceptionServices;

namespace Sunrise.Model.Common;

public static class Tasks
{
    public static void Execute(Task task)
    {
        if (task.Status == TaskStatus.RanToCompletion)
            return;

        try
        {
            Task.Run(async () => await task.ConfigureAwait(false)).Wait();
        }
        catch (AggregateException e)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
        }
    }

    public static TResult Execute<TResult>(Task<TResult> task)
    {
        if (task.Status == TaskStatus.RanToCompletion)
            return task.Result;

        try
        {
            return Task.Run(async () => await task.ConfigureAwait(false)).Result;
        }
        catch (AggregateException e)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
        }

        return default;
    }

    public static Task StartOnDefaultScheduler(Action action)
        => Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

    public static Task StartOnDefaultScheduler<TState>(Action<TState> action, TState state)
        => Task.Factory.StartNew(s => action((TState)s), state, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

    public static Task StartOnDefaultScheduler<TState1, TState2>(Action<TState1, TState2> action, TState1 state1, TState2 state2)
        => Task.Factory.StartNew(s =>
        {
            if (s is Tuple<TState1, TState2> tuple)
                action(tuple.Item1, tuple.Item2);
        }, Tuple.Create(state1, state2), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
}
