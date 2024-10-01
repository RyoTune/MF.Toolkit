using System.Text.Json;

namespace MF.Toolkit.Reloaded.Common;

public static class JsonFileSerializer
{
    private readonly static JsonSerializerOptions defaults = new() { WriteIndented = true };

    public static T Deserialize<T>(string filePath)
    {
        var data = File.ReadAllBytes(filePath);
        return JsonSerializer.Deserialize<T>(data) ?? throw new Exception();
    }

    public static void Serialize<T>(string filePath, T obj)
    {
        var data = JsonSerializer.Serialize(obj, defaults);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, data);
    }
}