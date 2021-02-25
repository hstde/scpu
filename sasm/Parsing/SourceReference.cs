using System;
using Sasm.Tokenizing;

namespace Sasm.Parsing
{
    public struct SourceReference
    {
        public readonly int lineNumber;
        public readonly int start;
        public readonly int length;

        public SourceReference(int lineNumber, int start, int length)
        {
            this.lineNumber = lineNumber;
            this.start = start;
            this.length = length;
        }

        public override bool Equals(object obj)
        {
            return obj is SourceReference reference &&
                   lineNumber == reference.lineNumber &&
                   start == reference.start &&
                   length == reference.length;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(lineNumber, start, length);
        }
    }
}