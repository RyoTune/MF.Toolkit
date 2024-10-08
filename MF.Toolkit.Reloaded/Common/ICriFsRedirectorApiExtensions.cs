using CriFs.V2.Hook.Interfaces;

namespace MF.Toolkit.Reloaded.Common;

internal static class ICriFsRedirectorApiExtensions
{
    public static void AddBind(
        this ICriFsRedirectorApi api,
        string ogPath,
        string newPath)
    {
        api.AddBindCallback(context =>
        {
            context.RelativePathToFileMap[$@"R2\{ogPath}"] =
            [
                new()
                {
                    FullPath = newPath,
                    LastWriteTime = DateTime.UtcNow,
                    ModId = Mod.NAME,
                },
            ];

            Log.Information($"Binded: {ogPath}\nPath: {newPath}");
        });
    }
}