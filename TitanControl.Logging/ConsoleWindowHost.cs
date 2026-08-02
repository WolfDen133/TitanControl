using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TitanControl.Logging;

internal sealed class ConsoleWindowHost
{
    private readonly object _sync = new();
    private bool _isEnabled;
    private bool _allocatedByLogger;

    public void EnableExistingOutput()
    {
        lock (_sync)
            _isEnabled = true;
    }

    /// <summary>
    /// Allocates a new console window on Windows. On other platforms this enables
    /// writing to the process's existing stdout, but does not spawn a terminal.
    /// </summary>
    public bool Open(string title)
    {
        lock (_sync)
        {
            if (!OperatingSystem.IsWindows())
            {
                _isEnabled = true;
                return true;
            }

            if (GetConsoleWindow() == IntPtr.Zero)
            {
                if (!AllocConsole())
                    return false;

                _allocatedByLogger = true;
                RebindStandardStreams();
            }

            try
            {
                Console.Title = title;
            }
            catch
            {
                // A title is cosmetic.
            }

            _isEnabled = true;
            return true;
        }
    }

    public void Close()
    {
        lock (_sync)
        {
            _isEnabled = false;

            if (OperatingSystem.IsWindows() && _allocatedByLogger)
            {
                try
                {
                    FreeConsole();
                }
                catch
                {
                    // Closing a diagnostic console must not affect application exit.
                }

                _allocatedByLogger = false;
            }
        }
    }

    public void Write(LogEntry entry, string formattedEntry)
    {
        lock (_sync)
        {
            if (!_isEnabled)
                return;

            ConsoleColor previousColor = ConsoleColor.White;
            bool restoreColor = false;

            try
            {
                previousColor = Console.ForegroundColor;
                restoreColor = true;

                Console.ForegroundColor = GetColor(entry.Level);
                Console.WriteLine(formattedEntry);
            }
            catch
            {
                // Console output is optional. File logging remains authoritative.
            }
            finally
            {
                if (restoreColor)
                {
                    try
                    {
                        Console.ForegroundColor = previousColor;
                    }
                    catch
                    {
                        // Ignore unavailable console state.
                    }
                }
            }
        }
    }

    private static void RebindStandardStreams()
    {
        try
        {
            var standardOut = new StreamWriter(
                Console.OpenStandardOutput(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };

            var standardError = new StreamWriter(
                Console.OpenStandardError(),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };

            Console.SetOut(TextWriter.Synchronized(standardOut));
            Console.SetError(TextWriter.Synchronized(standardError));
        }
        catch
        {
            // AllocConsole may still be useful even if rebinding fails.
        }
    }

    private static ConsoleColor GetColor(LogLevel level) => level switch
    {
        LogLevel.Trace => ConsoleColor.DarkGray,
        LogLevel.Debug => ConsoleColor.Gray,
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.Magenta,
        _ => ConsoleColor.White
    };

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();
}
