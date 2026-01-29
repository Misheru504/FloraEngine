using FloraEngine.Utils;
using System.Text;

namespace FloraEngine.Diagnostics;

/// <summary>
/// Wrapper used to catch exceptions and prints them in the console and a separate file
/// </summary>
public static class Reporter
{
#if DEBUG
    private const bool isDebug = true;
#else
    private const bool isDebug = false;
#endif

    /// <summary>
    /// Catch exceptions and create a detailed crash report
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    internal static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        bool isTerminating = args.IsTerminating;

        StringBuilder crashReport = new StringBuilder();
        PrintHeader(crashReport, isTerminating);

        if (args.ExceptionObject is Exception e)
        {
            PrintExceptionType(crashReport, e);

            Exception? inner = e.InnerException;
            int depth = 1;

            while (inner != null)
            {
                PrintInnerException(crashReport, inner, depth);

                inner = inner.InnerException;
                depth++;
            }

            // If it's an AggregateException (common with async), log all
            if (e is AggregateException aggEx)
            {
                PrintAggregateException(crashReport, aggEx);
            }
        }
        else
        {
            // Non-Exception object was thrown (rare, but possible)
            crashReport.AppendLine($"Non-Exception object thrown: {args.ExceptionObject}");
        }

        PrintEnvironment(crashReport);

        crashReport.AppendLine("====================");

        string crashFile = Path.Combine(Logger.LOG_FOLDER, $"{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.crash");
        TextParser.WriteFile(crashFile, crashReport.ToString());

        if (isTerminating)
        {
            Logger.Print(crashReport.ToString(), Logger.LogLevel.FATAL, true, "");
            Logger.SaveLogFile();
            Environment.Exit(1);
        }
        else
        {
            Logger.Print(crashReport.ToString(), Logger.LogLevel.ERROR, true, "");
        }
    }

    private static void PrintHeader(StringBuilder builder, bool isTerminating)
    {
        builder.AppendLine("=== CRASH REPORT ===");
        builder.AppendLine($"Time: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
        builder.AppendLine($"Is Fatal: {isTerminating}");
        builder.AppendLine($"Version: {Program.VERSION}");
        builder.AppendLine($"Debug build: {isDebug}");
        builder.AppendLine();
    }

    private static void PrintExceptionType(StringBuilder builder, Exception exception)
    {
        builder.AppendLine($"Exception type: {exception.GetType().FullName}");
        builder.AppendLine($"Message: {exception.Message}");
        builder.AppendLine($"Source: {exception.Source}");
        builder.AppendLine($"Target site: {exception.TargetSite}");
        builder.AppendLine();

        builder.AppendLine("Stack trace:");
        builder.AppendLine(exception.StackTrace);
    }

    private static void PrintInnerException(StringBuilder builder, Exception inner, int depth)
    {
        builder.AppendLine();
        builder.AppendLine($"-- Inner Exception ({depth}) --");
        builder.AppendLine($"Type: {inner.GetType().FullName}");
        builder.AppendLine($"Message: {inner.Message}");
        builder.AppendLine($"Stack Trace: {inner.StackTrace}");
    }

    private static void PrintAggregateException(StringBuilder builder, AggregateException aggregateException)
    {
        builder.AppendLine();
        builder.AppendLine("Aggregate Inner Exceptions:");
        foreach (var innerEx in aggregateException.Flatten().InnerExceptions)
        {
            builder.AppendLine($"  - {innerEx.GetType().Name}: {innerEx.Message}");
        }
    }

    private static void PrintEnvironment(StringBuilder builder)
    {
        builder.AppendLine();
        builder.AppendLine("-- Environment --");
        builder.AppendLine($"OS: {Environment.OSVersion}");
        builder.AppendLine($"64-bit: {Environment.Is64BitOperatingSystem && Environment.Is64BitProcess}");
        builder.AppendLine($"CLR Version: {Environment.Version}");
        builder.AppendLine($"Working Set: {Environment.WorkingSet / 1024 / 1024} MB");
    }
}
