.segment .rodata
numberCharacters: db "0123456789ABCDEF"

.segment .text

; converts the number in bc to hex string
; and stores it in the buffer pointed to by hl
toHex:
    push lr
    push hl
    ld d, c
    jl .conv8
    ; stack: low low nibble, low high nibble
    ld d, b
    jl .conv8
    ; stack: low low, low high, high low, high high
    pop bc
    pop hl
    ld [hl], bc
    pop bc
    ld [hl+2], bc
    pop lr
    ret
.conv8:
    ld hl, numberCharacters
    mov a, d
    and 0x0F
    add hl, a
    ld e, [hl]
    mov a, d
    shr
    shr
    shr
    shr
    ld hl, numberCharacters
    add hl, a
    ld d, [hl]
    push de
    ret