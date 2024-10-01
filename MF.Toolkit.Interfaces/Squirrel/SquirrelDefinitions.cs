namespace MF.Toolkit.Interfaces.Squirrel;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public unsafe static class SquirrelDefinitions
{
    public delegate void sq_pushroottable(SQVM* v);

    public delegate void sq_pushconsttable(SQVM* v);

    public delegate void sq_pushnull(SQVM* v);

    public delegate int sq_next(SQVM* v, int idx);

    public delegate void sq_pop(SQVM* v, int nelementstopop);

    public delegate int sq_gettop(SQVM* v);

    public delegate int sq_getstring(SQVM* v, int idx, nint* c);

    public delegate nint sq_rawget(SQVM* v, int idx);

    public delegate int sq_getclosure(SQVM* v, int idx, nint* p);

    public delegate nint SQVM_GetAt(SQVM* v, int idx);

    public delegate void sq_newclosure(SQVM* v, nint func, int nfreevars);

    public delegate int sq_getuserdata(SQVM* v, int idx, nint* p, nint* typeTag);
}

