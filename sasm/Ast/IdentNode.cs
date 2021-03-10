namespace Sasm.Ast
{
    using Irony.Parsing;

    public class IdentNode : AstNode
    {
        public string Ident { get; private set; }

        public override object Evaluate(EvaluationContext context)
        {
            return context.GetConstant(Ident);
        }

        public void SetValue(EvaluationContext context, object value)
        {
            context.SetConstant(Ident, value);
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            Ident = parseNode.Token.ValueString;
        }
    }
}