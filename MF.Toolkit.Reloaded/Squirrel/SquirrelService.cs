using MF.Toolkit.Interfaces.Squirrel;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using SharedScans.Interfaces;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using static MF.Toolkit.Interfaces.Squirrel.SquirrelDefinitions;
using static MF.Toolkit.Interfaces.Squirrel.Utils;

namespace MF.Toolkit.Reloaded.Squirrel;

internal unsafe class SquirrelService : ISquirrel, IUseConfig
{
    private readonly WrapperContainer<sq_pushroottable> _pushroottable;
    private readonly WrapperContainer<sq_pushnull> _pushnull;
    private readonly WrapperContainer<sq_next> _next;
    private readonly WrapperContainer<sq_pop> _pop;
    private readonly WrapperContainer<sq_gettop> _gettop;
    private readonly WrapperContainer<sq_getstring> _getstring;
    private readonly WrapperContainer<sq_rawget> _rawget;
    private readonly WrapperContainer<sq_pushconsttable> _pushconsttable;
    private readonly WrapperContainer<sq_getclosure> _getclosure;
    private readonly WrapperContainer<SQVM_GetAt> _sqvm_getat;
    private readonly WrapperContainer<sq_newclosure> _newclosure;
    private readonly WrapperContainer<sq_getuserdata> _getuserdata;

    private readonly HookContainer<sq_newclosure> newClosure;
    private readonly System.Timers.Timer timer;
    private readonly ISharedScans scans;
    private readonly HashSet<nint> knownFuncs = [];
    private readonly ObservableCollection<Function> functions = [];

    private readonly string dumpFile;
    private bool devMode;
    private bool dumpFunctions;

    public SquirrelService(ISharedScans scans, string modDir)
    {
        this.scans = scans;
        this.dumpFile = Path.Join(modDir, "dump", "functions.json");

        SquirrelPatterns.Scan(scans);
        _pushroottable = scans.CreateWrapper<sq_pushroottable>(Mod.NAME);
        _pushnull = scans.CreateWrapper<sq_pushnull>(Mod.NAME);
        _next = scans.CreateWrapper<sq_next>(Mod.NAME);
        _pop = scans.CreateWrapper<sq_pop>(Mod.NAME);
        _gettop = scans.CreateWrapper<sq_gettop>(Mod.NAME);
        _getstring = scans.CreateWrapper<sq_getstring>(Mod.NAME);
        _rawget = scans.CreateWrapper<sq_rawget>(Mod.NAME);
        _pushconsttable = scans.CreateWrapper<sq_pushconsttable>(Mod.NAME);
        _getclosure = scans.CreateWrapper<sq_getclosure>(Mod.NAME);
        _sqvm_getat = scans.CreateWrapper<SQVM_GetAt>(Mod.NAME);
        _newclosure = scans.CreateWrapper<sq_newclosure>(Mod.NAME);
        _getuserdata = scans.CreateWrapper<sq_getuserdata>(Mod.NAME);

        this.newClosure = scans.CreateHook<sq_newclosure>(this.NewClosure, Mod.NAME);

        this.timer = new System.Timers.Timer(1000) { AutoReset = false };
        this.timer.Elapsed += (sender, args) =>
        {
            if (this.dumpFunctions)
            {
                JsonFileSerializer.Serialize(this.dumpFile, this.functions);
                Log.Information($"Functions dumped: {this.dumpFile}");
            }
        };

        this.functions.CollectionChanged += (sender, args) => this.timer.Restart();
    }

    private unsafe void NewClosure(SQVM* v, nint func, int nfreevars)
    {
        var nameBuffer = (nint*)Marshal.AllocHGlobal(sizeof(nint));
        var funcBuffer = (nint*)Marshal.AllocHGlobal(sizeof(nint));

        var stackSize = sq_gettop(v);
        if (stackSize > 0 && SQ_SUCCEEDED(sq_getstring(v, stackSize * -1 + 1, nameBuffer)))
        {
            var funcLoc = func;

            // Get function pointer from userdata (game functions).
            if (stackSize > 2)
            {
                sq_getuserdata(v, stackSize * -1 + 2, funcBuffer, (nint*)0);
                funcLoc = **(nint**)funcBuffer;
            }

            if (this.knownFuncs.Contains(funcLoc) == false)
            {
                var funcName = Marshal.PtrToStringAnsi(*nameBuffer)!;

                // func = premade functions that take in the native function
                //        as a single userdata.
                // ig each func handles different arg/return possibilities?

                this.scans.Broadcast(funcName, funcLoc);
                this.knownFuncs.Add(funcLoc);
                this.functions.Add(new(funcName, funcLoc));
            }
        }
        else if (this.knownFuncs.Contains(func) == false)
        {
            Log.Verbose($"{nameof(sq_newclosure)} || Func: 0x{func:X} || Function has no pushed name or {nameof(sq_getstring)} failed.");
            this.knownFuncs.Add(func);
        }

        Marshal.FreeHGlobal((nint)nameBuffer);
        Marshal.FreeHGlobal((nint)funcBuffer);
        this.newClosure.Hook!.OriginalFunction(v, func, nfreevars);
    }

    public void ConfigChanged(Config config)
    {
        this.devMode = config.DevMode;
        this.dumpFunctions = config.DumpFunctions;
        this.timer.Restart();
    }

    public sq_pushroottable sq_pushroottable => _pushroottable.Wrapper;
    public sq_pushnull sq_pushnull => _pushnull.Wrapper;
    public sq_next sq_next => _next.Wrapper;
    public sq_pop sq_pop => _pop.Wrapper;
    public sq_gettop sq_gettop => _gettop.Wrapper;
    public sq_getstring sq_getstring => _getstring.Wrapper;
    public sq_rawget sq_rawget => _rawget.Wrapper;
    public sq_pushconsttable sq_pushconsttable => _pushconsttable.Wrapper;
    public sq_getclosure sq_getclosure => _getclosure.Wrapper;
    public sq_newclosure sq_newclosure => _newclosure.Wrapper;
    public sq_getuserdata sq_getuserdata => _getuserdata.Wrapper;

    private class Function(string name, nint address, int numArgs = -1, int retType = -1)
    {
        public string Name { get; } = name;

        public string Address { get; } = $"0x{address:X}";

        public string Offset { get; } = $"0x{address - Utilities.BaseAddress:X}";

        public int NumArgs { get; } = numArgs;

        public int RetType { get; } = retType;
    }
}
