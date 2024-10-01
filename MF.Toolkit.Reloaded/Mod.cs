using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Squirrel;
using MF.Toolkit.Reloaded.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using SharedScans.Interfaces;

namespace MF.Toolkit.Reloaded;

public class Mod : ModBase, IExports
{
    public const string NAME = "MF.Toolkit.Reloaded";
    private readonly IModLoader modLoader;
    private readonly IReloadedHooks? hooks;
    private readonly ILogger log;
    private readonly IMod owner;

    private Config config;
    private readonly IModConfig modConfig;

    private readonly SquirrelService squirrel;

    public Mod(ModContext context)
    {
        this.modLoader = context.ModLoader;
        this.hooks = context.Hooks;
        this.log = context.Logger;
        this.owner = context.Owner;
        this.config = context.Configuration;
        this.modConfig = context.ModConfig;

        Project.Init(this.modConfig, this.modLoader, this.log, true);

        this.modLoader.GetController<ISharedScans>().TryGetTarget(out var scans);

        this.squirrel = new(scans!);
        this.modLoader.AddOrReplaceController<ISquirrel>(this.owner, this.squirrel);

        Project.Start();
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        config = configuration;
        log.WriteLine($"[{modConfig.ModId}] Config Updated: Applying");
    }

    public Type[] GetTypes() => [ typeof(ISquirrel) ];
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}