namespace Sasm.Ast
{
    using Irony.Ast;
    using Irony.Parsing;

    public class WarningNode : AstNode
    {
        public override object Evaluate(EvaluationContext context)
        {
            foreach (var child in GetChildNodes())
            {
                context.Out.Write(child.Evaluate(context));
            }

            context.Out.WriteLine();
            return null;
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