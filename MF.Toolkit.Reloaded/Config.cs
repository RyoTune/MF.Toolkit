using MF.Toolkit.Reloaded.Template.Configuration;
using System.ComponentModel;

namespace MF.Toolkit.Reloaded.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("Log Level")]
    [DefaultValue(LogLevel.Information)]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    [DisplayName("Developer Mode")]
    [DefaultValue(false)]
    public bool DevMode { get; set; } = false;

    [DisplayName("Dump Functions")]
    [DefaultValue(false)]
    public bool DumpFunctions { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}
