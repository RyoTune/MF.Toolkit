using MF.Toolkit.Interfaces.Library;
using MF.Toolkit.Interfaces.Metaphor;
using MF.Toolkit.Interfaces.Metaphor.Models;
using MF.Toolkit.Reloaded.Common;
using MF.Toolkit.Reloaded.Configuration;
using MF.Toolkit.Reloaded.Metaphor.Services;

namespace MF.Toolkit.Reloaded.Metaphor;

internal class MetaphorToolkit : IMetaphorToolkit, IUseConfig
{
    private readonly IEnumerable<IRegisterMod> _modders;
    private readonly BitService _bits;
    private readonly CounterService _counters;
    private readonly CriFileService _files;

    public MetaphorToolkit(IEnumerable<IRegisterMod> modders, IMetaphorLibrary metaLib)
    {
        _modders = modders;
        _bits = new BitService(metaLib);
        _counters = new CounterService(metaLib);
        _files = new();
    }

    public void AddFolder(string modId, string folderPath)
    {
        foreach (var modder in _modders)
        {
            modder.RegisterMod(modId, folderPath);
        }

        var bitsFile = Path.Join(folderPath, "bits.mfv");
        if (File.Exists(bitsFile))
        {
            var bits = MetaVarSerializer.DeserializeFile<int, bool>(bitsFile);
            foreach (var kv in bits)
            {
                _bits.SetBit(kv.Key, kv.Value);
            }
        }

        var countersFile = Path.Join(folderPath, "counters.mfv");
        if (File.Exists(countersFile))
        {
            var counters = MetaVarSerializer.DeserializeFile<int, int>(countersFile);
            foreach (var kv in counters)
            {
                _counters.SetCounter(kv.Key, kv.Value);
            }
        }

        var redirectsFile = Path.Join(folderPath, "files.mfv");
        if (File.Exists(redirectsFile))
        {
            var redirects = MetaVarSerializer.DeserializeFile<string, string>(redirectsFile);
            foreach (var kv in redirects)
            {
                _files.RedirectFile(kv.Key, kv.Value);
            }
        }
    }

    public void ConfigChanged(Config config)
    {
        _bits.ConfigChanged(config);
        _counters.ConfigChanged(config);
    }

    public void RedirectFile(string ogPath, string newPath) => _files.RedirectFile(ogPath, newPath);

    public void SetBit(int bit, bool value) => _bits.SetBit(bit, value);

    public void SetCounter(int counter, int value) => _counters.SetCounter(counter, value);

    public void CheckOwnedDlc(Action<GameDlc[]> ownedDlc)
    {
        throw new NotImplementedException();
    }
}
