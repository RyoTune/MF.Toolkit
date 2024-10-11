using SharedScans.Interfaces;
using static MF.Toolkit.Interfaces.Squirrel.SquirrelDefinitions;

namespace MF.Toolkit.Reloaded.Squirrel;

internal static class SquirrelPatterns
{
    private static readonly Dictionary<string, string> patterns = new()
    {
        [nameof(sq_pushroottable)] = "48 83 EC 28 48 8D 51 ?? E8",
        [nameof(sq_pushnull)] = "48 8B 51 ?? 48 8D 42 ?? 48 C1 E2 04",
        [nameof(sq_next)] = "48 89 5C 24 ?? 57 48 83 EC 70 48 8B F9 48 85 D2",
        [nameof(sq_pop)] = "48 85 D2 7E ?? 48 89 5C 24",
        [nameof(sq_gettop)] = "48 8B 41 48 48 2B 41 50 C3",
        [nameof(sq_getstring)] = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 4C 89 C3 48 89 CF",
        //[nameof(sq_rawget)] = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F9 48 85 D2",
        [nameof(sq_pushconsttable)] = "48 83 EC 28 48 8B 91 ?? ?? ?? ?? 48 83 C2 68",
        [nameof(sq_getclosure)] = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B F0 48 8B EA 48 8B F9 48 C7 C2 FF FF FF FF",
        [nameof(SQVM_GetAt)] = "48 8B 41 ?? 48 03 C2 48 C1 E0 04",
        [nameof(sq_newclosure)] = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 49 8B F8 48 8B F1",
        [nameof(sq_getuserdata)] = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 49 8B D9 49 8B F8 48 8B F1",
    };

    public static void Scan(ISharedScans scans)
    {
        foreach (var pattern in patterns) scans.AddScan(pattern.Key, pattern.Value);
    }
}
