using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TitanControl.Logging;

internal static class LogFormatter
{
    public static string Format(LogEntry entry)
    {
        var builder = new StringBuilder(256);

        builder.Append(entry.Timestamp.ToString(
            "yyyy-MM-dd HH:mm:ss.fff zzz",
            CultureInfo.InvariantCulture));

        builder.Append(" [");
        builder.Append(GetLevelCode(entry.Level));
        builder.Append("] [T:");
        builder.Append(entry.ManagedThreadId.ToString("000", CultureInfo.InvariantCulture));
        builder.Append(']');

        if (entry.OperationId is Guid operationId)
        {
            builder.Append(" [OP:");
            builder.Append(ShortId(operationId));

            if (entry.ParentOperationId is Guid parentId)
            {
                builder.Append(" P:");
                builder.Append(ShortId(parentId));
            }

            builder.Append(']');
        }

        if (!string.IsNullOrWhiteSpace(entry.Category))
        {
            builder.Append(" [");
            builder.Append(entry.Category);
            builder.Append(']');
        }

        builder.Append(' ');
        builder.Append(entry.Message);

        if (entry.Properties is { Count: > 0 })
        {
            builder.Append(" {");
            bool first = true;

            foreach (KeyValuePair<string, object?> property in entry.Properties)
            {
                if (!first)
                    builder.Append(", ");

                first = false;
                builder.Append(property.Key);
                builder.Append('=');
                builder.Append(FormatValue(property.Value));
            }

            builder.Append('}');
        }

        if (entry.Exception is not null)
        {
            builder.AppendLine();
            builder.Append(entry.Exception);
        }

        return builder.ToString();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "null",
            string text => '"' + Escape(text) + '"',
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
             .Replace("\r", "\\r", StringComparison.Ordinal)
             .Replace("\n", "\\n", StringComparison.Ordinal)
             .Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string ShortId(Guid id) => id.ToString("N")[..8];

    private static string GetLevelCode(LogLevel level) => level switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        LogLevel.Critical => "CRT",
        _ => "UNK"
    };
}
