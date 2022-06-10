namespace Sasm.Ast
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Loader;
    using Irony.Ast;
    using Irony.Parsing;

    public abstract class AstNode : IAstNodeInit, IBrowsableAstNode
    {
        private List<AstNode> childNodes;

        public string FileName { get; private set; }
        public SourceSpan Span { get; private set; }
        public int Position => Span.Location.Position;
        public int ChildCount => childNodes?.Count ?? 0;

        public AstNode()
        {
        }

        public AstNode AddChild(ParseTreeNode node)
        {
            if (childNodes is null)
                childNodes = new List<AstNode>();
            AstNode astNode = node.AstNode as AstNode;
            childNodes.Add(astNode);
            return astNode;
        }

        IEnumerable IBrowsableAstNode.GetChildNodes()
        {
            return GetChildNodes();
        }

        public IEnumerable<AstNode> GetChildNodes()
        {
            if (childNodes is null)
                return Enumerable.Empty<AstNode>();
            return childNodes;
        }

        public void Init(Irony.Ast.AstContext context, ParseTreeNode parseNode)
        {
            var ctx = context as AstContext;
            Span = parseNode.Span;
            FileName = ctx.FileName;
            parseNode.AstNode = this;
            Init(ctx, parseNode);
        }

        public abstract void Init(AstContext context, ParseTreeNode parseNode);

        public abstract object Evaluate(EvaluationContext context);

        public abstract void Assemble(AssemblyContext context);
    }
}