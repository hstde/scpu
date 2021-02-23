namespace ScpuMicroGen
{
    using System;

    [Flags]
    public enum Flags : byte
    {
        None = 0,
        Zero = 1 << 0,
        Sign = 1 << 1,
        Carry = 1 << 2,
        EnableInterrupts = 1 << 3,
        ExtendedInstructions = 1 << 4,
    }
}