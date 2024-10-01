using MF.Toolkit.Interfaces.Library;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Inventory;
using MF.Toolkit.Reloaded.Library;
using MF.Toolkit.Reloaded.Squirrel;
using MF.Toolkit.Reloaded.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using SharedScans.Interfaces;
using System.Diagnostics;

namespace MF.Toolkit.Reloaded;

public class Mod : ModBase, IExports
{
    public const string NAME = "MF.Toolkit.Reloaded";
    private readonly IModLoader modLoader;
    private readonly IReloadedHooks? hooks;
    private readonly ILogger log;
    private readonly IMod owner;

    private Config config;
    private readonly MetaphorLibrary metaphor;
    private readonly IModConfig modConfig;

    private readonly SquirrelService squirrel;

    private readonly List<IUseConfig> configurables = [];
    private readonly InventoryService inventory;

    public Mod(ModContext context)
    {
        this.modLoader = context.ModLoader;
        this.hooks = context.Hooks;
        this.log = context.Logger;
        this.owner = context.Owner;
        this.config = context.Configuration;
        this.modConfig = context.ModConfig;

#if DEBUG
        Debugger.Launch();
#endif

        Project.Init(this.modConfig, this.modLoader, this.log, true);

        var modDir = this.modLoader.GetDirectoryForModId(this.modConfig.ModId);
        this.modLoader.GetController<ISharedScans>().TryGetTarget(out var scans);

        this.squirrel = new(scans!, modDir);
        this.configurables.Add(this.squirrel);
        this.modLoader.AddOrReplaceController<ISquirrel>(this.owner, this.squirrel);

        this.metaphor = new MetaphorLibrary(scans!);
        this.modLoader.AddOrReplaceController<IMetaphorLibrary>(this.owner, this.metaphor);

        this.inventory = new InventoryService();
        this.configurables.Add(this.inventory);

        this.ConfigurationUpdated(this.config);
        Project.Start();
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        config = configuration;
        log.WriteLine($"[{modConfig.ModId}] Config Updated: Applying");

        foreach (var item in this.configurables) item.ConfigChanged(config);
    }

    public Type[] GetTypes() => [ typeof(ISquirrel), typeof(IMetaphorLibrary) ];
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}