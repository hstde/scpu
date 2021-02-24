namespace Sasm.Parsing.ParseTree
{
    using System;

    public abstract class ParseTreeNode
    {
        public readonly SourceReference sourceReference;

        public bool HasErrors { get; }

        protected ParseTreeNode(SourceReference sourceReference)
        {
            this.sourceReference = sourceReference;
        }
    }
}