using System;
using System.Diagnostics;

namespace Sasm.Tokenizing
{
    public struct Token
    {
        public TokenType TokenType { get; }
        public string Content { get; }
        public int LineNumber { get; }
        public int Start { get; }
        public int Length => Content.Length;

        public Token(TokenType type, int lineNumber, string line, int start, int length)
        {
            TokenType = type;
            Start = start;
            Content = line.Substring(start, length);
            LineNumber = lineNumber;
        }

        public Token(TokenType type, int lineNumber, string content, int start)
        {
            TokenType = type;
            Start = start;
            Content = content;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return $"{TokenType} @ {Start}:{Length} \"{Content}\"";
        }

        public override bool Equals(object obj)
        {
            return obj is Token token &&
                   TokenType == token.TokenType &&
                   Content == token.Content &&
                   LineNumber == token.LineNumber &&
                   Start == token.Start &&
                   Length == token.Length;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TokenType, Content, LineNumber, Start, Length);
        }
    }
}