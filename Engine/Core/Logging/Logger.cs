using System;
using System.IO;
using FloraEngine.Core.Data;
using System.Runtime.CompilerServices;
using System.Text;

namespace FloraEngine.Core.Logging;

public static class Logger
{
    public const string LOG_FOLDER = @"logs";

    private static readonly string LogFile = Path.Combine(AppContext.BaseDirectory, LOG_FOLDER, $"{DateTime.Now:dd-MM-yyyy_HH-mm}.log");
    private static readonly StringBuilder _logLines = new StringBuilder();
    private static readonly object _lock = new object();

    public static void Info(string message, [CallerFilePath] string filename = "")
        => Log("INFO", message, filename, ConsoleColor.Green);

    public static void Warning(string message, [CallerFilePath] string filename = "")
        => Log("WARNING", message, filename, ConsoleColor.Yellow);

    public static void Error(string message, [CallerFilePath] string filename = "")
        => Log("ERROR", message, filename, ConsoleColor.Red);

    public static void Fatal(string message, [CallerFilePath] string filename = "")
        => Log("Fatal", message, filename, ConsoleColor.DarkRed);

    public static void Debug(string message, [CallerFilePath] string filename = "")
        => Log("DEBUG", message, filename, ConsoleColor.Blue);

    public static void Render(string message, [CallerFilePath] string filename = "")
        => Log("RENDER", message, filename, ConsoleColor.Magenta);

    private static void Log(string level, string message, string filename, ConsoleColor consoleColor)
    {
        lock (_lock)
        {
            filename = Path.GetFileNameWithoutExtension(filename);

            Console.Write("[ ");
            Console.ForegroundColor = consoleColor;
            Console.Write(level);
            Console.ResetColor();
            Console.WriteLine($" ] {filename}{(string.IsNullOrWhiteSpace(filename) ? "" : ": ")}{message}");

            _logLines.AppendLine($"[ {level} ] {filename}{(string.IsNullOrWhiteSpace(filename) ? "" : ": ")}{message}");
        }
    }

    public static void SaveLogFile()
    {
        Info($"Saving log file (at {LogFile})...");
        _logLines.AppendLine();

        TextParser.AppendFile(LogFile, _logLines.ToString());
    }

    public static void ClearLogFolder()
    {
        if (!Directory.Exists(LOG_FOLDER)) return;
        Directory.Delete(LOG_FOLDER, true);

        Debug($"Cleared log directory (at {Path.GetFullPath(LOG_FOLDER)})");
    }
}
