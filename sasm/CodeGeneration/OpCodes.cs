namespace Sasm.CodeGeneration
{
    using System;

    public static partial class OpCodes
    {

        public static bool TakesSingleByteArgument(OpCode op)
        {
            switch (op.operand)
            {
                case OperandType.Implicit8:
                case OperandType.IndirectDisplacement:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TakesShortArgument(OpCode op)
        {
            switch (op.operand)
            {
                case OperandType.Absolute:
                case OperandType.Implicit16:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TakesNoArgument(OpCode op)
        {
            return !(TakesSingleByteArgument(op) || TakesShortArgument(op));
        }
    }
}