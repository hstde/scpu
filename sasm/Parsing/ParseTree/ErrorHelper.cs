namespace Sasm.Parsing.ParseTree
{
    using System;
    using System.Collections.Generic;

    public static class ErrorHelper
    {
        public static bool IsError(ParseTreeNode node)
        {
            return (node is ErrorNode) || (!(node is null) && node.HasErrors);
        }

        public static IEnumerable<ErrorNode> CollectErrors(ParseTree tree)
        {
            return CollectErrors(tree.root);
        }

        private static IEnumerable<ErrorNode> CollectErrors(ParseTreeNode node)
        {
            if (node is ErrorNode en)
                yield return en;

            if (node.Children != null)
                foreach (var c in node.Children)
                    foreach (var ec in CollectErrors(c))
                        yield return ec;
        }
    }
}