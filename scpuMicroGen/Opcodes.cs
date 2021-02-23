namespace ScpuMicroGen
{
    public enum Opcodes : byte
    {
        NOP = 0x00,
        LD_A_IMM8 = 0x01,
        LD_B_IMM8 = 0x02,
        LD_C_IMM8 = 0x03,
        LD_D_IMM8 = 0x04,
        LD_E_IMM8 = 0x05,
        LD_H_IMM8 = 0x06,
        LD_L_IMM8 = 0x07,
        LD_BC_IMM16 = 0x08,
        LD_DE_IMM16 = 0x09,
        LD_HL_IMM16 = 0x0a,
        LD_IX_IMM16 = 0x0b,
        LD_IY_IMM16 = 0x0c,
        LD_A_ABS = 0x0d,
        LD_HL_ABS = 0x0e,
        LD_A_IND_BC = 0x0f,
        LD_HL_IND_BC = 0x10,
        LD_A_IND_DE = 0x11,
        LD_HL_IND_DE = 0x12,
        LD_A_IND_HL = 0x13,
        LD_B_IND_HL = 0x14,
        LD_C_IND_HL = 0x15,
        LD_D_IND_HL = 0x16,
        LD_E_IND_HL = 0x17,
        LD_H_IND_HL = 0x18,
        LD_L_IND_HL = 0x19,

        LD_A_IND_IX = 0x1d,
        LD_A_IND_IY = 0x25,
        LD_A_IND_SP = 0x2d,
        LD_HL_IND_SP = 0x2e,

    }

    public enum ExtOpcodes : byte
    {
        LD_SP_IMM16 = 0x00,
        LD_IV_IMM16 = 0x01,
        LD_B_ABS = 0x02,
        LD_C_ABS = 0x03,
        LD_D_ABS = 0x04,
        LD_E_ABS = 0x05,
        LD_H_ABS = 0x06,
        LD_L_ABS = 0x07,

        LD_BC_ABS = 0x08,
        LD_DE_ABS = 0x09,
        LD_IX_ABS = 0x0a,
        LD_IY_ABS = 0x0b,
        LD_SP_ABS = 0x0c,
        LD_IV_ABS = 0x0d,
    }
}