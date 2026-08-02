using System;
using System.IO;
using System.Text;

namespace TitanControl.Logging;

internal sealed class EmergencyLogWriter
{
    private readonly object _sync = new();
    private readonly string _path;

    public EmergencyLogWriter(string directory, string fileNamePrefix)
    {
        _path = Path.Combine(directory, $"{fileNamePrefix}-emergency.log");
    }

    public void Write(LogEntry entry, string reason)
    {
        WriteText(
            $"--- EMERGENCY WRITE: {reason} ---{Environment.NewLine}" +
            LogFormatter.Format(entry) +
            Environment.NewLine);
    }

    public void WriteText(string text)
    {
        try
        {
            lock (_sync)
            {
                string? directory = Path.GetDirectoryName(_path);

                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                File.AppendAllText(
                    _path,
                    text.EndsWith(Environment.NewLine, StringComparison.Ordinal)
                        ? text
                        : text + Environment.NewLine,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }
        }
        catch
        {
            // Last-chance logging must never throw into a crashing application.
        }
    }
}
