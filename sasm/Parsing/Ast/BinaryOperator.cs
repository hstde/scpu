namespace Sasm.Parsing.Ast
{
    using System;
    using Sasm.Parsing.ParseTree;

    public class BinaryOperator : AstNode
    {
        public enum ExpressionType
        {
            Addition, Subtraction, Multiplication, Dividing
        }

        public readonly AstNode left;
        public readonly AstNode right;
        public readonly ExpressionType op;

        public BinaryOperator(SourceReference reference, ParseTreeNode node) : base(reference)
        {
        }
    }
}