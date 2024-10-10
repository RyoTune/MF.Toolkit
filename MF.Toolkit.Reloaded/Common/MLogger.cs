using Microsoft.Extensions.Logging;

namespace MF.Toolkit.Reloaded.Common;

internal class MLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        => LevelToRyoLevel(logLevel) >= RyoTune.Reloaded.Log.LogLevel;

    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);
        switch (logLevel)
        {
            case Microsoft.Extensions.Logging.LogLevel.Trace:
                RyoTune.Reloaded.Log.Verbose(message);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Debug:
                RyoTune.Reloaded.Log.Debug(message);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Information:
                RyoTune.Reloaded.Log.Information(message);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Warning:
                RyoTune.Reloaded.Log.Warning(message);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Error:
                RyoTune.Reloaded.Log.Error(message);
                break;
            case Microsoft.Extensions.Logging.LogLevel.Critical:
                RyoTune.Reloaded.Log.Error(message);
                break;
            case Microsoft.Extensions.Logging.LogLevel.None:
                RyoTune.Reloaded.Log.Information(message);
                break;
            default:
                break;
        }
    }

    private static RyoTune.Reloaded.LogLevel LevelToRyoLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
        => logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => RyoTune.Reloaded.LogLevel.Verbose,
            Microsoft.Extensions.Logging.LogLevel.Debug => RyoTune.Reloaded.LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => RyoTune.Reloaded.LogLevel.Information,
            Microsoft.Extensions.Logging.LogLevel.Warning => RyoTune.Reloaded.LogLevel.Warning,
            Microsoft.Extensions.Logging.LogLevel.Error => RyoTune.Reloaded.LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => RyoTune.Reloaded.LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.None => RyoTune.Reloaded.LogLevel.Information,
            _ => throw new NotImplementedException(),
        };
}