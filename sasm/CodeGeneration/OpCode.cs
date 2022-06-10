namespace Sasm.CodeGeneration
{
    using System;

    public struct OpCode
    {
        public readonly OpCodeValues value;
        public readonly OperandType operand;
        public readonly int size;
        public readonly byte prefix;
        public readonly byte opcode;

        public OpCode(OpCodeValues opcode, OperandType operand)
        {
            this.value = opcode;
            this.operand = operand;
            size = (int)opcode < 256 ? 1 : 2;
            prefix = (byte)((short)opcode >> 8);
            this.opcode = (byte)opcode;
        }

        public override bool Equals(object obj)
        {
            return obj is OpCode code &&
                   value == code.value &&
                   operand == code.operand;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(value, operand);
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}