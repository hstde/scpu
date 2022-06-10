LD SP, 0xFFFF
JL MAIN
J $

MSG: DB "HELLO WORLD", 0
SCREENBUFFERPOINTER: DW 0x4000
SCREENOFFSET: DW 0

MAIN:
    PUSH LR
    LD HL, MSG
    PUSH HL
    JL PRINT
    POP LR
    RET

PRINT:
    LD IY, [SCREENBUFFERPOINTER]
    LD HL, [SCREENOFFSET]

    POP BC

    LD B, 0x07
    .LOOP:
        LD A, [BC]
        INC BC
        TEST 0
            JZ .ENDLOOP ; while *ix != 0
        LEA IX, [IY + HL]
        LD [IX], A
        LD [IX + 1], B
        ADD HL, 2
        J .LOOP
    .ENDLOOP:

    LD [SCREENOFFSET], HL
    RET