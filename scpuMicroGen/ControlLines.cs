namespace ScpuMicroGen
{
    using System;

    [Flags]
    public enum ControlLines : ulong
    {
        None = 0,
        Write_ReadPinWrite = 1ul << 0,
        Write_ReadPinRead = 0ul << 0,
        BusEnablePin = 1ul << 1,
        //IRLoad = 1ul << 2,
        TogglePhase = 1ul << 3,

        // 4 AluOp (x3), And Or Not Xor shl shr Add Sub
        // 5 AluOp,
        // 6 AluOp,
        AluOpAnd = 0ul << 4,
        AluOpOr = 1ul << 4,
        AluOpNot = 2ul << 4,
        AluOpXor = 3ul << 4,
        AluOpShl = 4ul << 4,
        AluOpShr = 5ul << 4,
        AluOpAdd = 6ul << 4,
        AluOpSub = 7ul << 4,
        AOut = 1ul << 7,

        ALoad = 1ul << 8,
        A2Alu = 1ul << 9,
        TempOut = 1ul << 10,
        TempLoad = 1ul << 11,

        Temp2Alu = 1ul << 12,
        ACBALoad = 1ul << 13,
        ACBBLoad = 1ul << 14,
        DBus2HLBus = 1ul << 15,

        DBus2A = 1ul << 16,
        DBus2DBuf = 1ul << 17,
        DBus2Flags = 1ul << 18,
        DBus2Temp = 1ul << 19,

        // 20 ACOp (x2; +1, +ACBB +LACBBSignExt -1)
        // 21 ACOp,
        ACOpInc = 0ul << 20,
        ACOpAdd = 1ul << 20,
        ACOpAdd8Sign = 2ul << 20,
        ACOpDec = 3ul << 20,
        ACRes2HLBus = 1ul << 22,
        DBufOut = 1ul << 23,

        DBufLoad = 1ul << 24,
        ABufLoad = 1ul << 25,
        ACRes2ACB = 1ul << 26,
        LoadZeroFlag = 1ul << 27,

        LoadSignFlag = 1ul << 28,
        LoadCarryFlag = 1ul << 29,
        LoadAluFlags = LoadZeroFlag | LoadSignFlag | LoadCarryFlag,
        LoadInterruptEnableFlag = 1ul << 30,
        Alu2Flags = 1ul << 31,

        Flags2Alu = 1ul << 32,
        ATempLoad = 1ul << 33,
        ATemp2ABus = 1ul << 34,
        HLBus2DBusSelL = 0ul << 35,
        HLBus2DBusSelH = 1ul << 35,
        LBus2DBus = HLBus2DBusSelL | HLBus2DBus,
        HBus2DBus = HLBus2DBusSelH | HLBus2DBus,

        DBus2HLBusSelL = 0ul << 36,
        DBus2HLBusSelH = 1ul << 36,
        DBus2LBus = DBus2HLBusSelL | DBus2HLBus,
        DBus2HBus = DBus2HLBusSelH | DBus2HLBus,
        // 37 Reg2HBusSel,(x3; none W B D H SPh PCh 0)
        // 38 Reg2HBusSel,
        // 39 Reg2HBusSel,
        Reg2HBusSelW = 1ul << 37,
        Reg2HBusSelB = 2ul << 37,
        Reg2HBusSelD = 3ul << 37,
        Reg2HBusSelH = 4ul << 37,
        Reg2HBusSelSPh = 5ul << 37,
        Reg2HBusSelSpec0h = 6ul << 37,
        Reg2HBusSelSpec1h = 7ul << 37,
        Reg2HBusSelPCh = SpecialRegisterSet0 | Reg2HBusSelSpec0h,
        Reg2HBusSelLRh = SpecialRegisterSet0 | Reg2HBusSelSpec1h,
        Reg2HBusSelIXh = SpecialRegisterSet1 | Reg2HBusSelSpec0h,
        Reg2HBusSelIYh = SpecialRegisterSet1 | Reg2HBusSelSpec1h,
        Reg2HBusSelIVh = SpecialRegisterSet2 | Reg2HBusSelSpec0h,

        // 40 HBus2RegSel ,(x3; none W B D H SPh PCh LRh)
        // 41 HBus2RegSel,
        // 42 HBus2RegSel,
        HBus2RegSelW = 1ul << 40,
        HBus2RegSelB = 2ul << 40,
        HBus2RegSelD = 3ul << 40,
        HBus2RegSelH = 4ul << 40,
        HBus2RegSelSPh = 5ul << 40,
        HBus2RegSelSpec0h = 6ul << 40,
        HBus2RegSelSpec1h = 7ul << 40,
        HBus2RegSelPCh = SpecialRegisterSet0 | HBus2RegSelSpec0h,
        HBus2RegSelLRh = SpecialRegisterSet0 | HBus2RegSelSpec1h,
        HBus2RegSelIXh = SpecialRegisterSet1 | HBus2RegSelSpec0h,
        HBus2RegSelIYh = SpecialRegisterSet1 | HBus2RegSelSpec1h,
        HBus2RegSelIVh = SpecialRegisterSet2 | HBus2RegSelSpec0h,
        // 43 Reg2LBusSel ,(x3; none Z C E L SPl PCl LRl)

        // 44 Reg2LBusSel,
        // 45 Reg2LBusSel,
        Reg2LBusSelZ = 1ul << 43,
        Reg2LBusSelC = 2ul << 43,
        Reg2LBusSelE = 3ul << 43,
        Reg2LBusSelL = 4ul << 43,
        Reg2LBusSelSPl = 5ul << 43,
        Reg2LBusSelSpec0l = 6ul << 43,
        Reg2LBusSelSpec1l = 7ul << 43,
        Reg2LBusSelPCl = SpecialRegisterSet0 | Reg2LBusSelSpec0l,
        Reg2LBusSelLRl = SpecialRegisterSet0 | Reg2LBusSelSpec1l,
        Reg2LBusSelIXl = SpecialRegisterSet1 | Reg2LBusSelSpec0l,
        Reg2LBusSelIYl = SpecialRegisterSet1 | Reg2LBusSelSpec1l,
        Reg2LBusSelIVl = SpecialRegisterSet2 | Reg2LBusSelSpec0l,
        // 46 LBus2RegSel ,(x3; none Z C E L SPl PCl LRl)
        // 47 LBus2RegSel,

        // 48 LBus2RegSel,
        LBus2RegSelZ = 1ul << 46,
        LBus2RegSelC = 2ul << 46,
        LBus2RegSelE = 3ul << 46,
        LBus2RegSelL = 4ul << 46,
        LBus2RegSelSPl = 5ul << 46,
        LBus2RegSelSpec0l = 6ul << 46,
        LBus2RegSelSpec1l = 7ul << 46,
        LBus2RegSelPCl = SpecialRegisterSet0 | LBus2RegSelSpec0l,
        LBus2RegSelLRl = SpecialRegisterSet0 | LBus2RegSelSpec1l,
        LBus2RegSelIXl = SpecialRegisterSet1 | LBus2RegSelSpec0l,
        LBus2RegSelIYl = SpecialRegisterSet1 | LBus2RegSelSpec1l,
        LBus2RegSelIVl = SpecialRegisterSet2 | LBus2RegSelSpec0l,
        // 49 DBusSourceSel (x3)
        // 50 DBusSourceSel
        // 51 DBusSourceSel
        Imm2DBus = 1ul << 49,
        Flags2DBus = 2ul << 49,
        RBus2DBus = 3ul << 49,
        Temp2DBus = 4ul << 49,
        A2DBus = 5ul << 49,
        DBuf2DBus = 6ul << 49,
        HLBus2DBus = 7ul << 49,

        // 52 Imm (x8),
        // 53 Imm,
        // 54 Imm,
        // 55 Imm,

        // 56 Imm,
        // 57 Imm,
        // 58 Imm,
        // 59 Imm,

        // 60 Imm2BusSel (x2)
        // 61 Imm2BusSel
        ToggleExtendedInstruction = 1ul << 60,
        Imm2ABus = 2ul << 60,
        Imm2BBus = 3ul << 60,
        // 62 SpecialRegisterSetSelect (x2)
        // 63 SpecialRegisterSetSelect
        SpecialRegisterSet0 = 0ul << 62,
        SpecialRegisterSet1 = 1ul << 62,
        SpecialRegisterSet2 = 2ul << 62,
        SpecialRegisterSet3 = 3ul << 62,
    }

    [Flags]
    public enum ControlLines2 : uint
    {
        None = 0,
        SetExtInstFlag = 1u << 0,
        // 2 AckSel Int, DMA, Sync
        // 3 AckSel
        SendIntAck = 1u << 2,
        SendDMAAck = 2u << 2,
        SendSync = 3u << 2,
    }

    public static class ControlLinesExtensions
    {
        public static ControlLines SetImmediate(this ControlLines lines, byte imm) => lines | (ControlLines)((ulong)imm << 52);
    }
}