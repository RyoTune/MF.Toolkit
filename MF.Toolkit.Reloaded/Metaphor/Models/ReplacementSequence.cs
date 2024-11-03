namespace MF.Toolkit.Reloaded.Metaphor.Models;

public class ReplacementSequence
{
    public ReplacementSequence()
    {
    }

    public ReplacementSequence(string ogSeq, string newSeqSprite, string newSeq)
    {
        OriginalSeq = ogSeq;
        NewSeqSprite = newSeqSprite;
        NewSeq = newSeq;
    }

    public string OriginalSeq { get; set; } = string.Empty;

    public string NewSeqSprite { get; set; } = string.Empty;

    public string NewSeq { get; set; } = string.Empty;
}
