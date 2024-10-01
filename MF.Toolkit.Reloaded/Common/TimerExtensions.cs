namespace MF.Toolkit.Reloaded.Common;

internal static class TimerExtensions
{
    public static void Restart(this System.Timers.Timer timer) { timer.Stop(); timer.Start(); }
}
