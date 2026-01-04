using System.Text;

namespace Sunrise.Model.Common;

public static class ExceptionHandler
{
    public static string? GetString(Exception exception)
    {
        var builder = CacheStringBuilder.Get();
        WriteException(builder, exception);
        return CacheStringBuilder.ToString(builder);
    }

    private static void WriteException(StringBuilder builder, Exception exception)
    {
        if (exception is not null)
        {
            builder.Append("Type: ").Append(exception.GetType().FullName)
                .Append("\r\nMessage: ").Append(exception.Message);

            if (!string.IsNullOrEmpty(exception.Source))
                builder.Append("\r\nSource: ").Append(exception.Source);

            if (!string.IsNullOrEmpty(exception.StackTrace))
                builder.Append("\r\nStack trace:\r\n").Append(exception.StackTrace);

            WriteInnerExceptions(builder, exception);
        }

        builder.AppendLine();
    }

    private static void WriteInnerExceptions(StringBuilder builder, Exception exception)
    {
        if (exception is AggregateException aggregateException)
        {
            for (int i = 0; i < aggregateException.InnerExceptions.Count; i++)
            {
                builder.Append($"\r\nAggregateException -> Inner {i + 1}/{aggregateException.InnerExceptions.Count}\r\n");
                WriteException(builder, aggregateException.InnerExceptions[i]);
            }
        }
        else if (exception.InnerException is not null)
        {
            builder.Append("\r\nInner:\r\n");
            WriteException(builder, exception.InnerException);
        }
    }

}
