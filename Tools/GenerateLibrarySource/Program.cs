// See https://aka.ms/new-console-template for more information
using System.Text.Json;

Console.WriteLine("Hello, World!");

var defsFile = "./functions.json.def";

var defs = JsonSerializer.Deserialize<FunctionDefiniton[]>(File.ReadAllBytes(defsFile), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;

var definitions = new List<string>();
var properties = new List<string>();
var setters = new List<string>();
var fields = new List<string>();
var methods = new List<string>();

foreach (var def in defs)
{
    if (def.Name == "print"
        || def.Name == "Random")
    {
        continue;
    }

    if (def.Name == "PrintLog" || def.Name == "PrintLogSub" || def.Name == "AssertPrint")
    {
        def.Ret = "void";
        def.Args = ["nint"];
    }

    if (def.Name == "AssertPrint")
    {
        def.Ret = "void";
        def.Args = ["int", "nint"];
    }

    var definition = GetDefinition(def);
    definitions.Add(definition);

    var prop = GetProperty(def);
    properties.Add(prop);

    var set = GetSetter(def);
    setters.Add(set);

    var field = GetField(def);
    fields.Add(field);

    var method = GetMethod(def);
    methods.Add(method);
}

var interfacesFile = "./IMetaphorLibrary.cs";
var definitionsFile = "./MetaphorDefinitions.cs";
var libraryFile = "./MetaphorLibrary.cs";

GenerateFiles();

void GenerateFiles()
{
    var ogNs = "namespace generate_defs;";
    var newNsInterfaces = "namespace MF.Toolkit.Interfaces.Library;";
    var newNsReloaded = "namespace MF.Toolkit.Reloaded.Library;";

    var defText = File.ReadAllText(definitionsFile)
        .Replace("// [definitions]", string.Join('\n', definitions))
        .Replace(ogNs, $"using MF.Toolkit.Interfaces.Squirrel;\n\n{newNsInterfaces}");
    File.WriteAllText(definitionsFile, defText);

    var intText = File.ReadAllText(interfacesFile)
        .Replace("// [methods]", string.Join('\n', methods))
        .Replace(ogNs, $"using static MF.Toolkit.Interfaces.Library.MetaphorDefinitions;\n\n{newNsInterfaces}");
    File.WriteAllText(interfacesFile, intText);

    var libText = File.ReadAllText(libraryFile)
        .Replace("// [fields]", string.Join('\n', fields))
        .Replace("// [setters]", string.Join('\n', setters))
        .Replace("// [properties]", string.Join('\n', properties))
        .Replace(ogNs, $"using MF.Toolkit.Interfaces.Library;\nusing static MF.Toolkit.Interfaces.Library.MetaphorDefinitions;\n\n{newNsReloaded}");
    File.WriteAllText(libraryFile, libText);
}

string GetSetter(FunctionDefiniton def) => $"\t\t_{def.Name} = scans.CreateWrapper<{def.Name}>(Mod.NAME);";

string GetProperty(FunctionDefiniton def) => $"\tpublic {def.Name} {def.Name} => _{def.Name}.Wrapper;";

string GetField(FunctionDefiniton def) => $"\tprivate readonly WrapperContainer<{def.Name}> _{def.Name};";

string GetMethod(FunctionDefiniton def) => $"\tpublic {def.Name} {def.Name} {{ get; }}";

string GetDefinition(FunctionDefiniton def)
{
    var args = new List<string>();
    for (int i = 0; i < def.Args.Length; i++) args.Add($"{GetType(def.Args[i])} param{i + 1}");

    return $"public delegate {GetType(def.Ret)} {def.Name}({string.Join(", ", args)});";
}

string GetType(string type)
    => type switch
    {
        "int" or "void" or "bool" or "byte" or "short" or "SQVM *" or "void * *" or "int *"
        or "ushort" or "void *" or "nint" => type,
        "uint" => "int",
        "uint *" => "int *",
        "undefined8" => "nint",
        "undefined4" => "int",
        "undefined2" => "short",
        "undefined" or "char" or "undefined1" => "byte",
        "undefined8 *" => "nint *",
        "undefined4 *" => "int *",
        "undefined2 *" => "short *",
        "undefined *" or "char *" => "byte *",
        "ulonglong" or "longlong" or "size_t" => "nint",
        "longlong *" or "ulonglong *" => "nint *",
        "undefined * *" => "byte * *",
        _ => throw new Exception($"Unknown type: {type}")
    };

class FunctionDefiniton
{
    public string Name { get; set; }

    public string Ret { get; set; }

    public string[] Args { get; set; }
}