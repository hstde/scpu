namespace Sasm.Parsing.ParseTree
{
    using System.Collections.Generic;

    public class StartNode : ParseTreeNode
    {
        public StartNode(
            SourceReference sourceReference,
            IReadOnlyList<ParseTreeNode> children) 
            : base(sourceReference, children)
        {
        }
    }
}