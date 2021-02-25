namespace Sasm.Parsing.ParseTree
{
    using System;
    using System.Collections.Generic;

    public class NamedReference : ParseTreeNode
    {
        public string ReferenceName { get; }

        public override bool HasErrors => false;

        public NamedReference(
            string name,
            SourceReference sourceReference)
                : base(sourceReference, null)
        {
            ReferenceName = name;
        }

        public override string ToString()
        {
            return "Reference to " + ReferenceName;
        }
    }
}