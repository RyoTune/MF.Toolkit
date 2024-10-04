using MF.Toolkit.Interfaces.Messages.Models;
using MF.Toolkit.Reloaded.Messages.Models;
using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace MF.Toolkit.Reloaded.Messages.Parser;

public partial class MessageParser
{
    private static readonly Tokenizer<MessageToken> _tokenizer = new TokenizerBuilder<MessageToken>()
        .Ignore(Comment.CPlusPlusStyle)
        .Ignore(Character.WhiteSpace)
        .Match(Character.EqualTo('#'), MessageToken.Hash)
        .Match(Character.EqualTo('@'), MessageToken.AtSymbol)
        .Match(Character.EqualTo('{'), MessageToken.CurlyOpen)
        .Match(Character.EqualTo('}'), MessageToken.CurlyClose)
        .Match(Span.Regex(@".*"), MessageToken.Text)
        .Build();

    private static readonly TokenListParser<MessageToken, string> Content =
        from content in Token.EqualTo(MessageToken.Text).Or(Token.EqualTo(MessageToken.AtSymbol)).Many()
        .Between(Token.EqualTo(MessageToken.CurlyOpen), Token.EqualTo(MessageToken.CurlyClose))
        select string.Join('\n', content.Select(x => x.ToStringValue()));

    private static readonly TokenListParser<MessageToken, string> Identifier =
        from _ in Token.EqualTo(MessageToken.AtSymbol)
        from identifier in Token.EqualTo(MessageToken.Text)
        select identifier.ToStringValue();

    private static readonly TokenListParser<MessageToken, string?> Speaker =
        from _ in Token.EqualTo(MessageToken.Hash)
        from speaker in Token.EqualTo(MessageToken.Text)
        select speaker.ToStringValue();

    private static readonly TokenListParser<MessageToken, Message> ItemParser =
        from speaker in Speaker.OptionalOrDefault()
        from identifier in Identifier
        from content in Content
        select new Message() { Content = content, Identifier = identifier, Speaker = speaker };

    private static readonly TokenListParser<MessageToken, Message[]> Parser = ItemParser.Many();

    public static MessageDictionary Parse(string input)
    {
        var tokens = _tokenizer.Tokenize(input.ReplaceLineEndings("\n"));
        var msgs = Parser.Parse(tokens);
        return new(msgs);
    }
}
