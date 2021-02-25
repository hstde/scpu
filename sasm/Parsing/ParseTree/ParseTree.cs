namespace Sasm.Parsing.ParseTree
{
    using System;

    public class ParseTree
    {
        public readonly ParseTreeNode root;

        public bool HasError => root.HasErrors;

        public ParseTree(ParseTreeNode root)
        {
            this.root = root;
        }
    }
}