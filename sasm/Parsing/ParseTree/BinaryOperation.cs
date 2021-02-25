namespace Sasm.Parsing.ParseTree
{
    using System;
    using System.Collections.Generic;

    public class BinaryOperation : ParseTreeNode
    {
        public enum OperationType
        {
            Add, Sub, Mul, Div
        }

        public OperationType Type { get; }

        public BinaryOperation(
            OperationType type,
            SourceReference sourceReference,
            ParseTreeNode left,
            ParseTreeNode right)
            : base(sourceReference, new [] { left, right })
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}