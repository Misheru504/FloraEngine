using FloraEngine.Diagnostics;
using Utf8Json;

namespace FloraEngine.Utils;

public static class JsonMapper
{
    public static void Serialize(object data, string path)
    {
        byte[] jsonBytes = JsonSerializer.Serialize(data);
        string? directory = Path.GetDirectoryName(path);
        if (directory != null) Directory.CreateDirectory(directory);
        File.WriteAllBytes(path, jsonBytes);
        Logger.Print($"Saved data of type '{data.GetType()}' at '{path}'");
    }

    public static void PrettySerialize(object data, string path)
    {
        byte[] jsonBytes = JsonSerializer.PrettyPrintByteArray(JsonSerializer.Serialize(data));
        string? directory = Path.GetDirectoryName(path);
        if(directory != null) Directory.CreateDirectory(directory);
        File.WriteAllBytes(path, jsonBytes);
        Logger.Print($"Saved pretty data of type '{data.GetType()}' at '{path}'");
    }

    public static T Parse<T>(string path)
    {
        try
        {
            byte[] jsonBytes = File.ReadAllBytes(path);
            T value = JsonSerializer.Deserialize<T>(jsonBytes);
            return value;
        }
        catch (Exception ex)
        {
            Logger.Print($"Cannot parse JSON file at '{path}': {ex}", Logger.LogLevel.ERROR);
            throw;
        }
    }

    public static bool TryParse<T>(string path, out T? data)
    {
        try
        {
            data = Parse<T>(path);
            return true;
        }
        catch
        {
            data = default;
            return false;
        }
    }
}
