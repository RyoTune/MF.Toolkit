using System.Runtime.InteropServices;
using System.Text;

namespace MF.Toolkit.Reloaded.Common;

internal static class MarshalEx
{
    public static nint StringToHGlobalUni8(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s + "\0");
        var ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return ptr;
    }
}
