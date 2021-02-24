namespace Sasm.Parsing.Ast
{
    using System;

    public class Reference : AstNode
    {
        public readonly string name;

        public Reference(SourceReference reference, string labelName) : base(reference)
        {
            name = labelName;
        }
    }
}