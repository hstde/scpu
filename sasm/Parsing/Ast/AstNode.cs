namespace Sasm.Parsing.Ast
{
    using System;

    public abstract class AstNode
    {
        public readonly SourceReference sourceReference;

        public AstNode(SourceReference reference)
        {
            sourceReference = reference;
        }
    }
}