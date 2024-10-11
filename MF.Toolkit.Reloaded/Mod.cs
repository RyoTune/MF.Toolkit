using CNutSharp.Library.Squirrel;
using CriFs.V2.Hook.Interfaces;
using MF.Toolkit.Interfaces.Inventory;
using MF.Toolkit.Interfaces.Library;
using MF.Toolkit.Interfaces.Messages;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Inventory;
using MF.Toolkit.Reloaded.Library;
using MF.Toolkit.Reloaded.Messages;
using MF.Toolkit.Reloaded.Squirrel;
using MF.Toolkit.Reloaded.Squirrel.Scripts;
using MF.Toolkit.Reloaded.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using SharedScans.Interfaces;
using System.Diagnostics;

namespace MF.Toolkit.Reloaded;

public class Mod : ModBase, IExports
{
    public const string NAME = "MF.Toolkit.Reloaded";
    private readonly IModLoader _modLoader;
    private readonly IReloadedHooks? hooks;
    private readonly ILogger log;
    private readonly IMod owner;

    private Config config;
    private readonly IModConfig modConfig;

    private readonly MLogger _mlog;
    private readonly List<IUseConfig> _configurables = [];
    private readonly List<IRegisterMod> _modders = [];

    private readonly MetaphorLibrary _metaphor;
    private readonly SquirrelService _squirrel;
    private readonly ScriptsRegistry _scriptsRegistry;
    private readonly ScriptsService _scriptsService;
    private readonly GameFileProvider _fileProvider;
    private readonly SqCompiler _sqCompiler;
    private readonly InventoryService _inventory;
    private readonly MessageService _message;
    private readonly MessageRegistry _messageRegistry;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        hooks = context.Hooks;
        log = context.Logger;
        owner = context.Owner;
        config = context.Configuration;
        modConfig = context.ModConfig;

#if DEBUG
        Debugger.Launch();
#endif

        Project.Init(modConfig, _modLoader, log, true);
        Log.LogLevel = config.LogLevel;
        _mlog = new();

        var modDir = _modLoader.GetDirectoryForModId(modConfig.ModId);
        _modLoader.GetController<ISharedScans>().TryGetTarget(out var scans);
        _modLoader.GetController<ICriFsRedirectorApi>().TryGetTarget(out var criFs);

        _fileProvider = new(criFs!);
        _sqCompiler = new SqCompiler(Path.Join(modDir, "Squirrel", "Compiler", "sq.exe"), _mlog);

        _squirrel = new SquirrelService(scans!, modDir);
        _scriptsRegistry = new ScriptsRegistry(criFs!, _fileProvider, _sqCompiler, modDir);
        _scriptsService = new ScriptsService(_scriptsRegistry);
        _configurables.Add(_squirrel);
        _modders.Add(_scriptsRegistry);
        _modLoader.AddOrReplaceController<ISquirrel>(owner, _squirrel);

        _metaphor = new MetaphorLibrary(scans!);
        _modLoader.AddOrReplaceController<IMetaphorLibrary>(owner, _metaphor);

        _inventory = new InventoryService();
        _configurables.Add(_inventory);
        _modLoader.AddOrReplaceController<IInventory>(owner, _inventory);

        _messageRegistry = new MessageRegistry();
        _modders.Add(_messageRegistry);
        _configurables.Add(_messageRegistry);

        _message = new MessageService(_messageRegistry);
        _configurables.Add(_message);
        _modLoader.AddOrReplaceController<IMessage>(owner, _message);

        _modLoader.ModLoaded += OnModLoaded;
        _modLoader.OnModLoaderInitialized += OnInitialized;
        ConfigurationUpdated(config);
        Project.Start();
    }

    private void OnInitialized()
    {
        _scriptsRegistry.MergeNuts();
    }

    private void OnModLoaded(IModV1 mod, IModConfigV1 config)
    {
        if (config.ModDependencies.Contains(modConfig.ModId))
        {
            var modDir = _modLoader.GetDirectoryForModId(config.ModId);
            var metaDir = Path.Join(modDir, "Metaphor");
            if (Directory.Exists(metaDir))
            {
                foreach (var modder in _modders)
                {
                    modder.RegisterMod(config.ModId, metaDir);
                }
            }
        }
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        config = configuration;
        log.WriteLine($"[{modConfig.ModId}] Config Updated: Applying");

        Log.LogLevel = configuration.LogLevel;
        foreach (var item in _configurables) item.ConfigChanged(config);
    }

    public Type[] GetTypes() => [ typeof(ISquirrel), typeof(IMetaphorLibrary), typeof(IMessage) ];
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}