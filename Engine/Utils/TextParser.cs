using FloreEngine.Diagnostics;

namespace FloraEngine.Utils;

/// <summary>
/// Provides methods for reading and writing simple files
/// </summary>
public static class TextParser
{
    /// <summary>
    /// Reads and returns the content of a file at <paramref name="filePath"/>
    /// </summary>
    /// <param name="filePath">Path of the file to read</param>
    /// <returns>The content of the file as a string</returns>
    /// <exception cref="FileNotFoundException">Thrown if the file is not found</exception>
    public static string ReadFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found at : {filePath}");
            return File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            Logger.Print($"Error when reading the file at {filePath} : {e}", Logger.LogLevel.ERROR);
            return string.Empty;
        }
    }

    /// <summary>
    /// Adds the <paramref name="content"/> text at the end of the file at <paramref name="filePath"/>
    /// </summary>
    /// <param name="filePath">Path of the file to write to</param>
    /// <param name="content">Text to append at the end of the file</param>
    public static void AppendFile(string filePath, string content)
    {
        try
        {
            string? folder = Path.GetDirectoryName(filePath);
            if (folder != null && !Directory.Exists(folder)) Directory.CreateDirectory(folder);

            File.AppendAllText(filePath, content);
        }
        catch (Exception e)
        {
            Logger.Print($"Error when appending the file at {filePath} : {e}", Logger.LogLevel.ERROR);
        }
    }

    /// <summary>
    /// Writes the <paramref name="content"/> text of the file at <paramref name="filePath"/>
    /// </summary>
    /// <remarks>
    /// The file is overwritten if it already exists
    /// </remarks>
    /// <param name="filePath">Path of the file to write to</param>
    /// <param name="content">Text to write on the file</param>
    public static void WriteFile(string filePath, string content)
    {
        try
        {
            string? folder = Path.GetDirectoryName(filePath);
            if (folder != null && !Directory.Exists(folder)) Directory.CreateDirectory(folder);

            File.WriteAllText(filePath, content);
        }
        catch (Exception e)
        {
            Logger.Print($"Error when writing the file at {filePath} : {e}", Logger.LogLevel.ERROR);
        }
    }
}
