using System;
using System.Diagnostics;
using Sasm.Parsing;

namespace Sasm.Tokenizing
{
    public struct Token
    {
        public TokenType TokenType { get; }
        public string Content { get; }
        public SourceReference Source { get; }

        public Token(TokenType type, int lineNumber, string line, int start, int length)
        {
            TokenType = type;
            Content = line.Substring(start, length);
            Source = new SourceReference(lineNumber, start, length);
        }

        public Token(TokenType type, int lineNumber, string content, int start)
        {
            TokenType = type;
            Content = content;
            Source = new SourceReference(lineNumber, start, content.Length);
        }

        public override string ToString()
        {
            return $"{TokenType} @ {Source.start}:{Source.length} \"{Content}\"";
        }

        public override bool Equals(object obj)
        {
            return obj is Token token &&
                   TokenType == token.TokenType &&
                   Content == token.Content &&
                   Source.Equals(token.Source);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TokenType, Content, Source);
        }
    }
}