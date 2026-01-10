using FloreEngine.Utils;
using System.Runtime.CompilerServices;
using System.Text;

namespace FloreEngine.Diagnostics;

// Apparently, using a static class is wrong for logging because we are using dependencies. A singleton is what we need.
// I will have a look into this later.

/// <summary>
/// Wrapper around the console for better debugging and logging
/// </summary>
public static class Logger
{
    private static readonly StringBuilder logs;
    public static readonly string logFilePath;
    public const string LOG_FOLDER = @".\logs";
    
    private const string INFO_PREFIX     = "INFO";
    private const string WARN_PREFIX     = "WARN";
    private const string ERROR_PREFIX    = "ERROR";
    private const string FATAL_PREFIX    = "FATAL";
    private const string DEBUG_PREFIX    = "DEBUG";
    private const string RENDER_PREFIX   = "RENDER";
    private static readonly string[] levels = [INFO_PREFIX, WARN_PREFIX, ERROR_PREFIX, FATAL_PREFIX, DEBUG_PREFIX, RENDER_PREFIX];

#if DEBUG
    private const bool isDebug = true;
#else
    private const bool isDebug = false;
#endif

    static Logger()
    {
        string logFilePath = Path.Combine(LOG_FOLDER, $"{DateTime.Now:dd-MM-yyyy_HH-mm}.log");
        Logger.logFilePath = logFilePath;
        logs = new StringBuilder();
    }

    /// <summary>
    /// Saves a text file in a folder called logs where the program is executed
    /// </summary>
    public static void SaveLogFile()
    {
        Print($"Saving log file (at {logFilePath})...");
        logs.AppendLine();

        TextParser.AppendFile(logFilePath, logs.ToString());
    }

    /// <summary>
    /// Print a message on the console and the log file
    /// </summary>
    /// <param name="message">Message to print</param>
    /// <param name="level">Log level of the message</param>
    /// <param name="saveOnFile">Should the message be printed on the file</param>
    /// <param name="filename">Name of the file printing it (default: the class)</param>
    public static void Print(object message, LogLevel level = LogLevel.INFO, bool saveOnFile = true, [CallerFilePath] string filename = "")
    {
        //string timestamp = DateTime.Now.ToString("HH:mm");
        filename = Path.GetFileNameWithoutExtension(filename);

        // Example of a log message:
        // [ INFO ] MainRenderer: Loading...
        string formattedLog = $"[ {levels[(int) level]} ] " + // Show the level of the error
            $"{filename}" + // File that called it
            $"{(filename != "" ? ": " : "")}" + // Add ": " if the file isn't empty, else add nothing
            $"{message}"; // The message to show

        if(saveOnFile) logs.Append(formattedLog);
        LogColors(formattedLog);
    }

    /// <summary>
    /// Print a message with the prefix DEBUG
    /// </summary>
    /// <param name="message">Message to print</param>
    /// <param name="filename">Name of the file printing it (default: the class)</param>
    public static void Debug(object message, [CallerFilePath] string filename = "")
    {
        if(isDebug) Print(message, LogLevel.DEBUG, true, filename);
    }

    /// <summary>
    /// Print a message with the prefix RENDER
    /// </summary>
    /// <param name="message">Message to print</param>
    /// <param name="filename">Name of the file printing it (default: the class)</param>
    public static void Render(object message, [CallerFilePath] string filename = "") => Print(message, LogLevel.RENDER, true, filename);

    private static void LogColors(string log)
    {
        string[] words = log.Split(' ');

        for(int i = 0; i < words.Length; i++)
        {
            switch (words[i])
            {
                default:
                    Console.Write(words[i] + " ");
                    break;
                case INFO_PREFIX:
                    WriteColor(words[i] + " ", ConsoleColor.Green);
                    break;
                case WARN_PREFIX:
                    WriteColor(words[i] + " ", ConsoleColor.Yellow);
                    break;
                case ERROR_PREFIX:
                    WriteColor(words[i] + " ", ConsoleColor.Red);
                    break;
                case FATAL_PREFIX:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    WriteColor(words[i], ConsoleColor.White);
                    Console.Write(" ");
                    break;
                case DEBUG_PREFIX:
                    WriteColor(words[i] + " ", ConsoleColor.Blue);
                    break;
                case RENDER_PREFIX:
                    WriteColor(words[i] + " ", ConsoleColor.Magenta);
                    break;
            }
        }

        Console.WriteLine();
    }

    private static void WriteColor(string messsage, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(messsage);
        Console.ResetColor();
    }

    /// <summary>
    /// Delete the logs folder (if this file is saved, it will recreate it afterwards)
    /// </summary>
    internal static void ClearLogFolder()
    {
        if (!Directory.Exists(LOG_FOLDER)) return;
        Directory.Delete(LOG_FOLDER, true);
        Debug($"Cleared log directory (at {Path.GetFullPath(LOG_FOLDER)})");
    }

    /// <summary>
    /// Shows all prefixes to test the colors in the console
    /// </summary>
    internal static void TestColors()
    {
        Debug("Testing colors...");

        string testColors = "NORMAL";

        for(int i = 0; i < levels.Length; i++)
        {
            testColors += $" {levels[i]}";
        }

        Debug(testColors);
    }

    /// <summary>
    /// Different levels of the logs
    /// </summary>
    /// <remarks>DEBUG logs will not be printed in a release build</remarks>
    public enum LogLevel
    {
        INFO = 0,
        WARNING = 1,
        ERROR = 2,
        FATAL = 3,
        DEBUG = 4,
        RENDER = 5,
    }
}
