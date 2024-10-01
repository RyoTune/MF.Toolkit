using static MF.Toolkit.Interfaces.Squirrel.SquirrelDefinitions;

namespace MF.Toolkit.Reloaded.Squirrel;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface ISquirrel
{
    sq_getclosure sq_getclosure { get; }
    sq_getstring sq_getstring { get; }
    sq_gettop sq_gettop { get; }
    sq_getuserdata sq_getuserdata { get; }
    sq_newclosure sq_newclosure { get; }
    sq_next sq_next { get; }
    sq_pop sq_pop { get; }
    sq_pushconsttable sq_pushconsttable { get; }
    sq_pushnull sq_pushnull { get; }
    sq_pushroottable sq_pushroottable { get; }
    sq_rawget sq_rawget { get; }
}