namespace Sasm.Ast
{
    using System.Collections.Generic;
    using Irony.Ast;
    using Irony.Parsing;

    public class ConstantListNode : AstNode
    {
        public override void Assemble(AssemblyContext context)
        {
            throw new System.NotImplementedException();
        }

        public override object Evaluate(EvaluationContext context)
        {
            return EvaluateAll(context);
        }

        public List<object> EvaluateAll(EvaluationContext context)
        {
            var list = new List<object>();

            foreach (var child in GetChildNodes())
            {
                list.Add(child.Evaluate(context));
            }

            return list;
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            foreach (var child in parseNode.GetMappedChildNodes())
            {
                if (!(child.AstNode is null))
                    AddChild(child);
            }
        }
    }
}