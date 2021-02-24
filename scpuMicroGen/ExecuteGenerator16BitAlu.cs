namespace ScpuMicroGen
{
    using System;
    using static ScpuMicroGen.ControlLines;
    using static ScpuMicroGen.ControlLines2;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg8;
    using static ScpuMicroGen.ExecuteGenerator.HLBusReg16;

    public static partial class ExecuteGenerator
    {
        static (ControlLines[], ControlLines2[]) IncReg16(HLBusReg16 reg)
        {
            return (new[]
                {
                    // ADD L, 1
                    // L -> ATemp
                    (ReadLReg16(reg) | LBus2DBus | DBus2A | A2Alu | ATempLoad),
                    // Imm(1) -> BBus, ATemp -> ABus, ADD, UpdateFlags, AluRes -> L
                    (Imm2BBus | ATemp2ABus
                        | AluOpAdd | Alu2Flags | LoadAluFlags
                        | RBus2DBus | WriteLReg16(reg) | DBus2LBus).SetImmediate(1),
                        // ADC H, 0
                        // H -> ATemp
                    (ReadHReg16(reg) | HBus2DBus | DBus2A | A2Alu | ATempLoad),
                    // Imm(0) -> BBus, ATemp -> ABus, FlagsIn, ADD, UpdateFlags, AluRes -> H
                    (Imm2BBus | ATemp2ABus
                        | Flags2Alu | AluOpAdd | Alu2Flags | LoadAluFlags
                        | RBus2DBus | WriteHReg16(reg) | DBus2HBus
                        | TogglePhase).SetImmediate(0),
                },
                new[]
                {
                    ControlLines2.None,
                    ControlLines2.None,
                    ControlLines2.None,
                    ControlLines2.None,
                });
        }
    }
}