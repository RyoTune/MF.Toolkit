using System.Text;

namespace MF.Toolkit.Reloaded.Common;

internal class MetaVarSerializer
{
    public static Dictionary<TKey, TValue> DeserializeFile<TKey, TValue>(string filePath)
        where TKey : notnull
    {
        var lines = File.ReadAllLines(filePath);
        var result = new Dictionary<TKey, TValue>();
        foreach (var line in lines)
        {
            if (line.StartsWith("//"))
                continue;

            var parts = line.Split("=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                continue;

            var key = GetValue<TKey>(parts[0])!;
            var valueObj = GetValue<TValue>(parts[1])!;
            result[key] = valueObj;
        }

        return result;
    }

    public static void SerializeFile<TKey, TValue>(string filePath, Dictionary<TKey, TValue> obj)
        where TKey : notnull
    {
        var sb = new StringBuilder();
        foreach (var kv in obj)
        {
            sb.AppendLine($"{kv.Key!.ToString()}={kv.Value!.ToString()}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, sb.ToString());
    }

    private static TValue? GetValue<TValue>(string valueInput) => (TValue)Convert.ChangeType(valueInput, typeof(TValue));
}
