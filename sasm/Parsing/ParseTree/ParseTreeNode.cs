namespace Sasm.Parsing.ParseTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ParseTreeNode
    {
        public readonly SourceReference sourceReference;

        public IReadOnlyList<ParseTreeNode> Children;

        public virtual bool HasErrors => 
            Children is null || 
            Children.Any(c => c is null || c.HasErrors);

        protected ParseTreeNode(
            SourceReference sourceReference,
            IReadOnlyList<ParseTreeNode> children)
        {
            this.sourceReference = sourceReference;
            Children = children;
        }
    }
}