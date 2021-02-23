namespace ScpuMicroGen
{
    using System;
    using static ScpuMicroGen.ControlLines;
    using static ScpuMicroGen.ControlLines2;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg8;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg16;

    public static partial class ExecuteGenerator
    {
        static (ControlLines[], ControlLines2[]) LdAImm8()
        {
            // the fetch will have abuf preloaded with argument pc
            // the acba is also preloaded with argument pc
            return (new[]
                {
                    (ACOpInc | ACRes2HLBus | WriteReg16(PC)
                        | BusEnablePin | Write_ReadPinRead | DBuf2DBus
                        | DBus2A | ALoad
                        | TogglePhase)
                },
                new[]
                {
                    ControlLines2.None,
                });
        }

        static (ControlLines[], ControlLines2[]) LdReg8Imm8(HLBusReg8 reg)
        {
            return (new[]
                {
                    (BusEnablePin | Write_ReadPinRead
                        | DBuf2DBus | DBus2Reg8(reg)),
                    (WriteReg16(PC) | ACRes2HLBus | ACOpInc
                        | TogglePhase)
                },
                new[]
                {
                    ControlLines2.None,
                    ControlLines2.None,
                });
        }

        static (ControlLines[], ControlLines2[]) LdAbsA()
        {
            return (new[]
                {
                    // ACBA + 1 -> ACBA, ACBA + 1 -> ABuf
                    // D -> W
                    (ACOpInc | ACRes2ACB | ACBALoad | ABufLoad
                        | BusEnablePin | Write_ReadPinRead
                        | DBuf2DBus | DBus2Reg8(W)),
                    // ACBA + 1 -> PC
                    // D -> DBuf
                    (ACOpInc | ACRes2HLBus | WriteReg16(PC)
                        | BusEnablePin | Write_ReadPinRead | DBufLoad),
                    // DBuf -> LBus
                    // W -> HBus
                    // HLBus -> ABuf
                    (DBufOut | DBuf2DBus | DBus2LBus
                        | Reg2HBusSelW
                        | ABufLoad),
                    // A -> D, Write(Abuf)
                    (A2DBus | AOut | DBus2DBuf | Write_ReadPinWrite | BusEnablePin
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

        static (ControlLines[], ControlLines2[]) LdIndA(HLBusReg16 reg)
        {
            return (new[]
                {
                    // HL -> ABuf
                    (ReadHReg16(reg) | ReadLReg16(reg) | ABufLoad),
                    // A -> D, Write(Abuf)
                    (A2DBus | AOut | DBus2DBuf | Write_ReadPinWrite | BusEnablePin
                        | TogglePhase)
                },
                new[]
                {
                    ControlLines2.None,
                        ControlLines2.None,
                });
        }

        static (ControlLines[], ControlLines2[]) LdAInd(HLBusReg16 reg)
        {
            return (new[]
                {
                    // HL -> ABuf
                    (ReadReg16(reg) | ABufLoad),
                    // D -> A, Write(Abuf)
                    (DBus2A | ALoad | DBuf2DBus | Write_ReadPinRead | BusEnablePin
                        | TogglePhase)
                },
                new[]
                {
                    ControlLines2.None,
                        ControlLines2.None,
                });
        }

        static (ControlLines[], ControlLines2[]) LdAAbs()
        {
            return (new[]
                {
                    // this is a mixture of lda (WZ)
                    // ACBA + 1 -> ACBA, ACBA + 1 -> ABuf
                    // Read(Abuf), D -> W
                    (ACOpInc | ACRes2ACB | ACBALoad | ABufLoad
                        | BusEnablePin | Write_ReadPinRead | DBuf2DBus | DBus2Reg8(W)),
                    // ACBA + 1 -> PC
                    // Read(ABuf), D -> DBuf
                    (ACOpInc | ACRes2HLBus | WriteReg16(PC)
                        | BusEnablePin | Write_ReadPinRead | DBufLoad),
                    // DBuf -> LBus
                    // W -> HBus
                    // HLBus -> ABuf
                    (DBufOut | DBuf2DBus | DBus2LBus
                        | Reg2HBusSelW
                        | ABufLoad),
                    // D -> A, Read(Abuf)
                    (DBus2A | ALoad | DBuf2DBus
                        | Write_ReadPinRead | BusEnablePin
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

        static (ControlLines[], ControlLines2[]) LdReg8Abs(HLBusReg8 reg)
        {
            return (new[]
                {
                    // this is a mixture of lda (WZ)
                    // ACBA + 1 -> ACBA, ACBA + 1 -> ABuf
                    // Read(Abuf), D -> W
                    (ACOpInc | ACRes2ACB | ACBALoad | ABufLoad
                        | BusEnablePin | Write_ReadPinRead | DBuf2DBus | DBus2Reg8(W)),
                    // ACBA + 1 -> PC
                    // Read(ABuf), D -> DBuf
                    (ACOpInc | ACRes2HLBus | WriteReg16(PC)
                        | BusEnablePin | Write_ReadPinRead | DBufLoad),
                    // DBuf -> LBus
                    // W -> HBus
                    // HLBus -> ABuf
                    (DBufOut | DBuf2DBus | DBus2LBus
                        | ReadReg8(W)
                        | ABufLoad),
                    // D -> A, Read(Abuf)
                    (DBus2Reg8(reg) | DBuf2DBus
                        | Write_ReadPinRead | BusEnablePin
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

        static (ControlLines[], ControlLines2[]) LdReg8Ind(HLBusReg8 target, HLBusReg16 ind)
        {
            return (new[]
                {
                    (ReadReg16(ind) | ABufLoad),
                    (DBuf2DBus | DBus2Reg8(target)
                        | Write_ReadPinRead | BusEnablePin
                        | TogglePhase)
                },
                new[]
                {
                    ControlLines2.None,
                    ControlLines2.None,
                });
        }
    }
}