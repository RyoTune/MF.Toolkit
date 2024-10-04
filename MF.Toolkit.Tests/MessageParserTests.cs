using MF.Toolkit.Reloaded.Messages.Parser;

namespace MF.Toolkit.Tests;

public class MessageParserTests
{
    [Fact]
    public void MessageParser_ParseTestMsg()
    {
        MessageParser.Parse(File.ReadAllText("./MessageParser/Test.msg"));
    }

    [Fact]
    public void MessageParser_ParseSpeakerIdMsg()
    {
        var msgs = MessageParser.Parse(File.ReadAllText("./MessageParser/SpeakIdTest.msg"));
        Assert.True(msgs.Count > 0);
    }
}
