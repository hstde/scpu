namespace Sasm.Parsing
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using Irony.Parsing;

    public class Grammar : Irony.Parsing.Grammar
    {
        public static Grammar Instance { get; } = new Grammar();

        public Terminal Comment { get; private set; }
        public Terminal Ident { get; private set; }
        public Terminal Number { get; private set; }
        public Terminal NormalString { get; private set; }
        public Terminal EscapedString { get; private set; }
        public Terminal CharLiteral { get; private set; }
        public Terminal Separator { get; private set; }

        public NonTerminal Start { get; private set; }
        public NonTerminal Line { get; private set; }
        public NonTerminal Instruction { get; private set; }
        public NonTerminal LabelDefinition { get; private set; }
        public NonTerminal Operation { get; private set; }
        public NonTerminal Op { get; private set; }
        public NonTerminal OperandList { get; private set; }
        public NonTerminal Operand { get; private set; }
        public NonTerminal Mnemonic { get; private set; }
        public NonTerminal RegisterAlias { get; private set; }
        public NonTerminal Register { get; private set; }
        public NonTerminal Constant { get; private set; }
        public NonTerminal BinExpr { get; private set; }
        public NonTerminal Literal { get; private set; }
        public NonTerminal BinOp { get; private set; }
        public NonTerminal AbsoluteMemAccess { get; private set; }
        public NonTerminal IndirectMemAccess { get; private set; }
        public NonTerminal DisplacementMemAccess { get; private set; }
        public NonTerminal OffsetMemAccess { get; private set; }
        public NonTerminal Directive { get; private set; }
        public NonTerminal SegmentDirective { get; private set; }
        public NonTerminal OriginDirective { get; private set; }
        public NonTerminal IncludeDirective { get; private set; }
        public NonTerminal TimesDirective { get; private set; }
        public NonTerminal DataDirective { get; private set; }
        public NonTerminal WarningDirective { get; private set; }
        public NonTerminal ConstDirective { get; private set; }
        public NonTerminal DataDefinition { get; private set; }
        public NonTerminal DataConstantList { get; private set; }
        public NonTerminal DataConstant { get; private set; }

        private Grammar() : base(false)
        {
            SetUpTerminals();

            SetUpNonTerminals();

            SetUpOperators();

            this.Root = Start;

            //this.LanguageFlags = LanguageFlags.CreateAst;
            this.LanguageFlags |= LanguageFlags.NewLineBeforeEOF;
        }

        private void SetUpOperators()
        {
            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");

            RegisterOperators(25, "<<", ">>");
            RegisterOperators(30, "+", "-");
            RegisterOperators(40, "*", "/");
        }

        private void SetUpNonTerminals()
        {
            CreateNonTerminals();

            Start.Rule = MakeStarRule(Start, Line);

            Line.Rule = LabelDefinition + Instruction + NewLine
                | LabelDefinition + NewLine
                | Instruction + NewLine
                | Empty + NewLine;

            Line.ErrorRule = SyntaxError + NewLine;

            LabelDefinition.Rule = Ident + ":";

            Instruction.Rule = Directive | Operation;

            Operation.Rule = Op + OperandList;

            Op.Rule = Mnemonic | Ident;

            Mnemonic.Rule = TerminalsFromEnum<Mnemonics>("opcode");

            OperandList.Rule = MakeListRule(OperandList, Separator, Operand, TermListOptions.StarList);

            Operand.Rule = Register
                | Constant
                | AbsoluteMemAccess
                | IndirectMemAccess
                | DisplacementMemAccess
                | OffsetMemAccess;

            Register.Rule = TerminalsFromEnum<Registers>("register");

            Constant.Rule = Literal
                | BinExpr
                | "(" + Constant + ")";

            Literal.Rule = Number | Ident | CharLiteral;

            BinExpr.Rule = Constant + BinOp + Constant;

            BinOp.Rule = ToTerm("+") | "-" | "*" | "/" | ">>" | "<<";

            AbsoluteMemAccess.Rule = "[" + Constant + "]";

            IndirectMemAccess.Rule = "[" + Register + "]";

            DisplacementMemAccess.Rule = "["
                + (Register + "+" + Constant | Constant + "+" + Register)
                + "]";

            OffsetMemAccess.Rule = "[" + Register + "+" + Register + "]";

            Directive.Rule = SegmentDirective
                | OriginDirective
                | IncludeDirective
                | TimesDirective
                | DataDirective
                | WarningDirective
                | ConstDirective;

            SegmentDirective.Rule = ToTerm(".segment") + Ident + (Constant | Empty);

            OriginDirective.Rule = ToTerm(".org") + Constant;

            IncludeDirective.Rule = ToTerm(".include") + NormalString;

            TimesDirective.Rule = ToTerm(".times") + Constant + (DataDirective | Operation);

            DataDirective.Rule = DataDefinition + DataConstantList;

            DataDefinition.Rule = (ToTerm("db") | "dw");

            DataConstantList.Rule = MakeListRule(DataConstantList, Separator, DataConstant, TermListOptions.PlusList);

            DataConstant.Rule = Constant | NormalString | EscapedString;

            WarningDirective.Rule = ToTerm(".warning") + DataConstantList;

            ConstDirective.Rule = ToTerm(".const") + Ident + DataConstant;

            MarkReservedWords(
                Enum.GetNames<Mnemonics>()
                    .Union(Enum.GetNames<Registers>())
                    .Append(".segment")
                    .Append(".org")
                    .Append(".include")
                    .Append(".times")
                    .Append("db").Append("dw")
                    .Append(".warning")
                    .Append(".const")
                    .ToArray());
        }

        private void CreateNonTerminals()
        {
            Start = new NonTerminal(nameof(Start));
            Line = new NonTerminal(nameof(Line));
            LabelDefinition = new NonTerminal(nameof(LabelDefinition));
            Instruction = new NonTerminal(nameof(Instruction));
            Operation = new NonTerminal(nameof(Operation));
            Op = new NonTerminal(nameof(Op));
            OperandList = new NonTerminal(nameof(OperandList));
            Operand = new NonTerminal(nameof(Operand));
            Mnemonic = new NonTerminal(nameof(Mnemonic));
            Register = new NonTerminal(nameof(Register));
            Constant = new NonTerminal(nameof(Constant));
            BinExpr = new NonTerminal(nameof(BinExpr));
            Literal = new NonTerminal(nameof(Literal));
            BinOp = new NonTerminal(nameof(BinOp));
            AbsoluteMemAccess = new NonTerminal(nameof(AbsoluteMemAccess));
            IndirectMemAccess = new NonTerminal(nameof(IndirectMemAccess));
            DisplacementMemAccess = new NonTerminal(nameof(DisplacementMemAccess));
            OffsetMemAccess = new NonTerminal(nameof(OffsetMemAccess));
            Directive = new NonTerminal(nameof(Directive));
            SegmentDirective = new NonTerminal(nameof(SegmentDirective));
            OriginDirective = new NonTerminal(nameof(OriginDirective));
            IncludeDirective = new NonTerminal(nameof(IncludeDirective));
            TimesDirective = new NonTerminal(nameof(TimesDirective));
            DataDirective = new NonTerminal(nameof(DataDirective));
            WarningDirective = new NonTerminal(nameof(WarningDirective));
            ConstDirective = new NonTerminal(nameof(ConstDirective));
            DataDefinition = new NonTerminal(nameof(DataDefinition));
            DataConstantList = new NonTerminal(nameof(DataConstantList));
            DataConstant = new NonTerminal(nameof(DataConstant));
        }

        private void SetUpTerminals()
        {
            Comment = new CommentTerminal(nameof(Comment), ";", "\n", "\r");
            NonGrammarTerminals.Add(Comment);

            var ident = new IdentifierTerminal(nameof(Ident), ".$_", ".$_");
            ident.Options = IdOptions.IsNotKeyword;
            Ident = ident;

            var number = new NumberLiteral(nameof(Number), NumberOptions.AllowUnderscore | NumberOptions.IntOnly | NumberOptions.AllowSign);
            number.AddPrefix("0x", NumberOptions.Hex);
            number.AddPrefix("0b", NumberOptions.Binary);
            number.AddPrefix("0", NumberOptions.Octal);
            Number = number;

            NormalString = new StringLiteral(nameof(NormalString), "\"", StringOptions.NoEscapes);
            EscapedString = new StringLiteral(nameof(EscapedString), "`", StringOptions.AllowsAllEscapes);
            CharLiteral = new StringLiteral(nameof(CharLiteral), "'", StringOptions.IsChar | StringOptions.AllowsAllEscapes);
            Separator = ToTerm(",", nameof(Separator));
        }

        private BnfExpression TerminalsFromEnum<TEnum>(string groupName)
            where TEnum : struct, Enum
        {
            BnfExpression expr = null;

            var names = Enum.GetNames<TEnum>();

            foreach (var name in names)
            {
                if (expr is null)
                    expr = ToTerm(name, groupName);
                else
                    expr |= ToTerm(name, groupName);
            }

            return expr;
        }
    }
}