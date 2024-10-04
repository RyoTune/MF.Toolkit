using MF.Toolkit.Interfaces.Common;
using MF.Toolkit.Reloaded.Template.Configuration;
using System.ComponentModel;

namespace MF.Toolkit.Reloaded.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [DisplayName("Preferred Language")]
    [Description("The preferred language to use for files when a mod does not support a language.\nIf a mod does not support the preferred language neither, a fallback will be used.")]
    [DefaultValue(Language.EN)]
    public Language LangPref { get; set; } = Language.EN;

    [DisplayName("Developer Mode")]
    [DefaultValue(false)]
    public bool DevMode { get; set; }

    [DisplayName("Dump Functions")]
    [DefaultValue(false)]
    public bool DumpFunctions { get; set; }

    [DisplayName("Unlock All Items")]
    [DefaultValue(false)]
    public bool UnlockAllItems { get; set; }
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
