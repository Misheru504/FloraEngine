using System.Text;

namespace FloreEngine.Diagnostics;

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
        crashReport.AppendLine("=== CRASH REPORT ===");
        crashReport.AppendLine($"Time: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
        crashReport.AppendLine($"Is Fatal: {isTerminating}");
        crashReport.AppendLine($"Version: {Program.VERSION}");
        crashReport.AppendLine($"Debug build: {isDebug}");
        crashReport.AppendLine();

        if (args.ExceptionObject is Exception e)
        {
            crashReport.AppendLine($"Exception type: {e.GetType().FullName}");
            crashReport.AppendLine($"Message: {e.Message}");
            crashReport.AppendLine($"Source: {e.Source}");
            crashReport.AppendLine($"Target site: {e.TargetSite}");
            crashReport.AppendLine();


            crashReport.AppendLine("Stack trace:");
            crashReport.AppendLine(e.StackTrace);

            var inner = e.InnerException;
            int depth = 1;
            while (inner != null)
            {
                crashReport.AppendLine();
                crashReport.AppendLine($"-- Inner Exception ({depth}) --");
                crashReport.AppendLine($"Type: {inner.GetType().FullName}");
                crashReport.AppendLine($"Message: {inner.Message}");
                crashReport.AppendLine($"Stack Trace: {inner.StackTrace}");

                inner = inner.InnerException;
                depth++;
            }

            // If it's an AggregateException (common with async), log all
            if (e is AggregateException aggEx)
            {
                crashReport.AppendLine();
                crashReport.AppendLine("Aggregate Inner Exceptions:");
                foreach (var innerEx in aggEx.Flatten().InnerExceptions)
                {
                    crashReport.AppendLine($"  - {innerEx.GetType().Name}: {innerEx.Message}");
                }
            }
        }
        else
        {
            // Non-Exception object was thrown (rare, but possible)
            crashReport.AppendLine($"Non-Exception object thrown: {args.ExceptionObject}");
        }

        crashReport.AppendLine();
        crashReport.AppendLine("-- Environment --");
        crashReport.AppendLine($"OS: {Environment.OSVersion}");
        crashReport.AppendLine($"64-bit: {Environment.Is64BitOperatingSystem && Environment.Is64BitProcess}");
        crashReport.AppendLine($"CLR Version: {Environment.Version}");
        crashReport.AppendLine($"Working Set: {Environment.WorkingSet / 1024 / 1024} MB");

        crashReport.AppendLine("====================");

        try
        {
            if (!Directory.Exists(Logger.LOG_FOLDER)) Directory.CreateDirectory(Logger.LOG_FOLDER);
            string crashFile = Path.Combine(Logger.LOG_FOLDER, $"{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.crash");
            File.WriteAllText(crashFile, crashReport.ToString());
        }
        catch (Exception ex)
        {
            Logger.Print($"Couldn't save crash log: {ex}", Logger.LogLevel.ERROR);
        }

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
}
