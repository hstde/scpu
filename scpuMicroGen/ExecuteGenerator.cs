namespace ScpuMicroGen
{
    using System;
    using Sasm.CodeGeneration;
    using static ScpuMicroGen.ControlLines;
    using static ScpuMicroGen.ControlLines2;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg16;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg8;
    using static Sasm.CodeGeneration.OpCodeValues;

    public static partial class ExecuteGenerator
    {
        public enum HLBusReg8
        {
            W,
            Z,
            B,
            C,
            D,
            E,
            H,
            L,
            SPh,
            SPl,
            PCh,
            PCl,
            LRh,
            LRl,
            IXh,
            IXl,
            IYh,
            IYl,
            IVh,
            IVl,
        }

        public enum HLBusReg16
        {
            WZ,
            BC,
            DE,
            HL,
            SP,
            PC,
            LR,
            IX,
            IY,
            IV,
        }

        public static (ControlLines[], ControlLines2[]) Generate(Flags flags, byte op)
        {
            // extend prefix?
            if (op == 0xFF)
                return ExtInst();

            if (!flags.HasFlag(Flags.ExtendedInstructions))
                return GenerateNormalSet(flags, (OpCodeValues)op);
            else
                return GenerateExtendedSet(flags, (OpCodeValues)(0xFF00 | op));
        }

        static (ControlLines[], ControlLines2[]) GenerateNormalSet(Flags flags, OpCodeValues op)
        {
            switch (op)
            {
                case LD_A_IMM8:
                    return LdAImm8();
                case LD_B_IMM8:
                    return LdReg8Imm8(B);
                case LD_C_IMM8:
                    return LdReg8Imm8(C);
                case LD_D_IMM8:
                    return LdReg8Imm8(D);
                case LD_E_IMM8:
                    return LdReg8Imm8(E);
                case LD_H_IMM8:
                    return LdReg8Imm8(H);
                case LD_L_IMM8:
                    return LdReg8Imm8(L);
                case LD_BC_IMM16:
                    return LdReg16Imm16(BC);
                case LD_DE_IMM16:
                    return LdReg16Imm16(DE);
                case LD_HL_IMM16:
                    return LdReg16Imm16(HL);
                case LD_IX_IMM16:
                    return LdReg16Imm16(IX);
                case LD_IY_IMM16:
                    return LdReg16Imm16(IY);
                case LD_A_ABS:
                    return LdAAbs();
                case LD_HL_ABS:
                    return LdReg16Abs(HL);
                case LD_A_IND_BC:
                    return LdAInd(BC);
                case LD_HL_IND_BC:
                    return LdReg16Ind(HL, BC);
                case LD_A_IND_DE:
                    return LdAInd(DE);
                case LD_HL_IND_DE:
                    return LdReg16Ind(HL, DE);
                case LD_A_IND_HL:
                    return LdAInd(HL);
                case LD_B_IND_HL:
                    return LdReg8Ind(B, HL);
                case LD_C_IND_HL:
                    return LdReg8Ind(C, HL);
                case LD_D_IND_HL:
                    return LdReg8Ind(D, HL);
                case LD_E_IND_HL:
                    return LdReg8Ind(E, HL);
                case LD_H_IND_HL:
                    return LdReg8Ind(H, HL);
                case LD_L_IND_HL:
                    return LdReg8Ind(L, HL);

                case LD_A_IND_IX:
                    return LdAInd(IX);
                case LD_A_IND_IY:
                    return LdAInd(IY);
                case LD_A_IND_SP:
                    return LdAInd(SP);

                /*case JL_IMM16:
                    return JlImm16();
                case JL_HL:
                    return JlReg16(HL);*/

                case NOP:
                default:
                    return Nop();
                    // Do nothing;
            }
        }

        static (ControlLines[], ControlLines2[]) GenerateExtendedSet(Flags flags, OpCodeValues op)
        {
            switch (op)
            {
                case LD_SP_IMM16:
                    return LdReg16Imm16(SP);
                case LD_IV_IMM16:
                    return LdReg16Imm16(IV);
                case LD_B_ABS:
                    return LdReg8Abs(B);
                case LD_C_ABS:
                    return LdReg8Abs(C);
                case LD_D_ABS:
                    return LdReg8Abs(D);
                case LD_E_ABS:
                    return LdReg8Abs(E);
                case LD_H_ABS:
                    return LdReg8Abs(H);
                case LD_L_ABS:
                    return LdReg8Abs(L);
                case LD_BC_ABS:
                    return LdReg16Abs(BC);
                case LD_DE_ABS:
                    return LdReg16Abs(DE);
                case LD_IX_ABS:
                    return LdReg16Abs(IX);
                case LD_IY_ABS:
                    return LdReg16Abs(IY);
                case LD_SP_ABS:
                    return LdReg16Abs(BC);
                case LD_IV_ABS:

                default:
                    // we really would want to generate a fault, but that doesn't exist :D
                    return Nop();
                    // Do nothing;
            }
        }

        static (ControlLines[], ControlLines2[]) ExtInst()
        {
            // toggle the extended instruction flag
            // and basically do a normal fetch
            return (new ControlLines[]
                {
                    // Set ext instruction flag
                    // PC -> ABuf, PC -> ACBA
                    (ReadReg16(PC) | ABufLoad | ACBALoad),
                    // ACBA + 1 -> HLBus, HLBus -> PC
                    // D -> IR
                    (ACOpInc | ACRes2HLBus | WriteReg16(PC)
                        | BusEnablePin | Write_ReadPinRead | DBuf2DBus
                        | TogglePhase)
                },
                new ControlLines2[]
                {
                    (ControlLines2.None),
                    (SetExtInstFlag)
                });
        }

        static (ControlLines[], ControlLines2[]) Nop() => (new[] { TogglePhase }, new[] { ControlLines2.None });

        static (ControlLines[], ControlLines2[]) JumpImm()
        {
            return (new[]
                {
                    // PC -> ACBA, PC -> ABuf
                    (Reg2LBusSelPCl | Reg2HBusSelPCh
                        | ABufLoad | ACBALoad),
                    // ACBA + 1 -> ACBA, ACBA + 1 -> ABuf
                    // Read(Abuf), D -> W
                    (ACOpInc | ACRes2ACB | ACBALoad | ABufLoad
                        | BusEnablePin | Write_ReadPinRead | DBuf2DBus
                        | DBus2HBus | HBus2RegSelW),
                    // ACBA + 1 -> PC
                    // Read(ABuf), D -> DBuf
                    (ACOpInc | ACRes2HLBus | HBus2RegSelPCh | LBus2RegSelPCl
                        | BusEnablePin | Write_ReadPinRead | DBufLoad),
                    // DBuf -> LBus
                    // W -> HBus
                    // HLBus -> PC
                    (DBufOut | DBuf2DBus | DBus2LBus
                        | Reg2HBusSelW
                        | HBus2RegSelPCh | LBus2RegSelPCl
                        | TogglePhase)
                },
                new[]
                {
                    ControlLines2.None,
                        ControlLines2.None,
                        ControlLines2.None,
                        ControlLines2.None,
                });
        }

        static (ControlLines[], ControlLines2[]) PcSkipImm16()
        {
            return (new[]
                {
                    // funny thing is, because the hl bus is blocked by PC
                    // we must have at least 3 cycles, because we can't load the constant 2 from the immediate field
                    // ADD PC, 2
                    // PC -> ACBA
                    (ReadReg16(PC) | ACBALoad),
                    // ACBA + 1 -> ACBA
                    (ACOpInc | ACRes2ACB | ACBALoad),
                    // ACBA + 1 -> PC
                    (ACOpInc | ACRes2HLBus | HBus2RegSelPCh | LBus2RegSelPCl
                        | TogglePhase),
                },
                new[]
                {
                    ControlLines2.None,
                        ControlLines2.None,
                        ControlLines2.None,
                });
        }

        private static (ControlLines[], ControlLines2[]) JlImm16()
        {
            // pc->acba, pc->aout
            // read, d->w, ainc, ACRes->acab, ACRes->aout
            // read, d->dbuf, ainc, ACRes->LR,
            // dbuf->PCl, W->PCh
            return (new ControlLines[]
            {
                (ReadReg16(PC) | ACBALoad | ABufLoad),
                (ACOpInc | ACRes2ACB | ACBALoad | ABufLoad
                    | BusEnablePin | Write_ReadPinRead | DBus2Reg8(W) | DBuf2DBus),
                (ACOpInc)
            },
            new ControlLines2[]
            {

            });
        }

        private static (ControlLines[], ControlLines2[]) JlReg16(HLBusReg16 reg)
        {
            // pc ->lr
            // hl->pc
            throw new NotImplementedException();
        }
        // ret
        // lr -> pc
        // 
        // push af
        // sp -> acba, sp -> aout
        // write, f -> D, ainc, ACRes -> acab, ACres -> aout
        // write, a -> D, ainc, ACRes -> SP
        // 
        // pop af
        // sp -> acba
        // adec, ACRes -> acab, ACRes -> aout
        // read, D -> a, adec, ACRes -> SP, ACres -> aout
        // read, D -> f
    }
}