using MF.Toolkit.Interfaces.Messages.Models;
using MF.Toolkit.Reloaded.Messages.Models.MessageLists;
using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace MF.Toolkit.Reloaded.Messages.Parser;

public class MessageParser
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

    private static readonly TokenListParser<MessageToken, string> Label =
        from _ in Token.EqualTo(MessageToken.AtSymbol)
        from label in Token.EqualTo(MessageToken.Text)
        select label.ToStringValue();

    private static readonly TokenListParser<MessageToken, string?> Speaker =
        from _ in Token.EqualTo(MessageToken.Hash)
        from speaker in Token.EqualTo(MessageToken.Text)
        select speaker.ToStringValue();

    private static readonly TokenListParser<MessageToken, Message> ItemParser =
        from speaker in Speaker.OptionalOrDefault()
        from label in Label
        from content in Content
        select new Message() { Content = content, Label = label, Speaker = speaker };

    private static readonly TokenListParser<MessageToken, Message[]> Parser = ItemParser.Many();

    public static MessageList Parse(string input)
    {
        var tokens = _tokenizer.Tokenize(input.ReplaceLineEndings("\n"));
        var msgs = Parser.Parse(tokens);
        return new(msgs);
    }
}
