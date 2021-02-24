namespace ScpuMicroGen
{
    using System;
    using static ScpuMicroGen.ControlLines;
    using static ScpuMicroGen.ControlLines2;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg8;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg16;

    public static partial class ExecuteGenerator
    {
        static (ControlLines[], ControlLines2[]) LdReg16Abs(HLBusReg16 reg)
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
                    // HLBus -> ACBA
                    (DBufOut | DBuf2DBus | DBus2LBus
                        | Reg2HBusSelW
                        | ABufLoad
                        | ACBALoad),
                    // D -> regH, Read(Abuf)
                    // ACBA + 1 -> Abuf
                    (WriteHReg16(reg) | DBus2HLBusSelH | DBus2HBus | DBuf2DBus
                        | Write_ReadPinRead | BusEnablePin
                        | ACRes2ACB | ACOpInc | ABufLoad),
                    // D -> regL, Read(Abuf)
                    (WriteLReg16(reg) | DBus2HLBusSelL | DBus2LBus | DBuf2DBus
                        | Write_ReadPinRead | BusEnablePin
                        | TogglePhase)
                },
                new[]
                {
                    ControlLines2.None,
                    ControlLines2.None,
                    ControlLines2.None,
                    ControlLines2.None,
                    ControlLines2.None,
                });
        }

        static (ControlLines[], ControlLines2[]) LdReg16Imm16(HLBusReg16 reg)
        {
            return (new[]
                {
                    // ACBA + 1 -> ACBA, ACBA + 1 -> ABuf
                    // Read(Abuf), D -> arg1
                    (ACOpInc | ACRes2ACB | ACBALoad | ABufLoad
                        | BusEnablePin | Write_ReadPinRead
                        | DBuf2DBus | DBus2HBus | WriteHReg16(reg)),
                    // ACBA + 1 -> PC
                    // Read(ABuf), D -> DBuf
                    (ACOpInc | ACRes2HLBus | HBus2RegSelPCh | LBus2RegSelPCl
                        | BusEnablePin | Write_ReadPinRead | DBufLoad),
                    // DBuf -> arg2
                    (DBufOut | DBuf2DBus | DBus2LBus | WriteLReg16(reg)
                        | TogglePhase)
                },
                new[]
                {
                    ControlLines2.None,
                    ControlLines2.None,
                    ControlLines2.None,
                });
        }

        static (ControlLines[], ControlLines2[]) LdReg16Ind(HLBusReg16 target, HLBusReg16 ind)
        {
            return (new[]
                {
                    ControlLines.None
                },
                new[]
                {
                    ControlLines2.None
                });
        }
    }
}