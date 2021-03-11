namespace Sasm.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Irony.Ast;
    using Irony.Parsing;

    public class WarningNode : AstNode
    {
        public ConstantListNode ConstantList { get; private set; }
        public override object Evaluate(EvaluationContext context)
        {
            var list = ConstantList.EvaluateAll(context);
            foreach (var e in list)
            {
                context.Out.Write(e);
            }

            context.Out.WriteLine();
            return null;
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            var children = parseNode.GetMappedChildNodes();
            ConstantList = AddChild(children[1]) as ConstantListNode;
        }
    }
}