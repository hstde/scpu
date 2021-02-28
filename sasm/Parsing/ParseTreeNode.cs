using System;
using System.Collections.Generic;
using System.Linq;
using Sasm.Parsing.Tokenizing;

namespace Sasm.Parsing
{
    public class ParseTreeNode
    {
        private List<ParseTreeNode> children;

        public ParseTreeNodeType NodeType { get; }
        public Token Token { get; }
        public SourceReference Source { get; }
        public IReadOnlyList<ParseTreeNode> Children => children
            ?? (IReadOnlyList<ParseTreeNode>)Array.Empty<ParseTreeNode>();

        public ParseTreeNode(Token token, ParseTreeNodeType type)
        {
            Token = token;
            Source = token.Source;
            NodeType = type;
        }

        public void AddChild(ParseTreeNode node)
        {
            if (children is null)
                children = new List<ParseTreeNode>();
            children.Add(node);
        }
    }
}