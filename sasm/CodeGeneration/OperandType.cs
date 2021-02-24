namespace Sasm.CodeGeneration
{
    using System;

    public enum OperandType
    {
        None,
        Implicit8,
        Implicit16,
        Absolute,
        Indirect,
        IndirectDisplacement,
        IndirectOffset
    }
}