using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Reloaded.Metaphor.Models;
using MF.Toolkit.Reloaded.Template.Configuration;
using System.ComponentModel;

namespace MF.Toolkit.Reloaded.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [DisplayName("Second Language")]
    [Description("The preferred language to use for files when a mod does not support a language.\nIf neither is supported, whatever is available will be used with a preference for English.")]
    [DefaultValue(Language.EN)]
    public Language SecondLanguage { get; set; } = Language.EN;

    [DisplayName("Edit Game Data")]
    [Description("Whether to apply edits to game variables (BITS, Counters, etc) to actual data.\nThis will make PERMANENT changes to game saves, enable with caution!")]
    [DefaultValue(false)]
    public bool ShouldEditData { get; set; } = false;

    [DisplayName("Developer Mode")]
    [Category("Developer")]
    [DefaultValue(false)]
    public bool DevMode { get; set; } = false;

    [DisplayName("Show Bits")]
    [Category("Developer")]
    [DefaultValue(GameVarLogMode.None)]
    public GameVarLogMode ShowBits { get; set; } = GameVarLogMode.None;

    [DisplayName("Show Counters")]
    [Category("Developer")]
    [DefaultValue(GameVarLogMode.None)]
    public GameVarLogMode ShowCounters { get; set; } = GameVarLogMode.None;

    [DisplayName("Dump Data")]
    [Description("Whether to dump useful game data to file.\nIncludes things such as functions, sprite info, etc.")]
    [Category("Developer")]
    [DefaultValue(false)]
    public bool DumpData { get; set; } = false;

    [DisplayName("Unlock All Items")]
    [Category("Developer")]
    [DefaultValue(false)]
    public bool UnlockAllItems { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
