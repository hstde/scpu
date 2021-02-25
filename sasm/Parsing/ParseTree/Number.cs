namespace Sasm.Parsing.ParseTree
{
    using System;

    public class Number : ParseTreeNode
    {
        public long Value { get; }

        public override bool HasErrors => false;

        public Number(SourceReference sourceReference, long value)
            : base(sourceReference, null)
        {
            Value = value;
        }

        public override string ToString()
        {
            return base.ToString() + " Value: " + Value;
        }
    }
}