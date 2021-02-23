using System;
using System.Diagnostics;

namespace Sasm.Tokenizing
{
    public struct Token
    {
        public TokenType TokenType { get; }
        public string Content { get; }
        public int Start { get; }
        public int Length => Content.Length;

        public Token(TokenType type, string line, int start, int length)
        {
            TokenType = type;
            Start = start;
            Content = line.Substring(start, length);
        }

        public Token(TokenType type, string content, int start)
        {
            TokenType = type;
            Start = start;
            Content = content;
        }

        public override string ToString()
        {
            return $"{TokenType} @ {Start}:{Length} \"{Content}\"";
        }
    }
}