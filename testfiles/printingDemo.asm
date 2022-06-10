.const initialScreenoffset 0
.const screenwidth 40
.const screenheight 25
.const maxScreenoffset screenwidth * screenheight * 2
.const scrollByteCount screenwidth * (screenheight - 1) * 2

.segment .rodata
message: db `Hello, world!\0`

.segment .text 0
inti:   ld sp, 0xff00
        ld bc, initialScreenoffset
        ld [screenoffset], bc
        jl main
        j $

main:   push lr
        ld hl, message
        ld b, 0x07
        jl println
        pop lr
        ret

println:push lr
        jl print
        ld hl, [screenoffset]
        jl lf_cr
        ld [screenoffset], hl
        pop lr
        ret
print:
        ld de, hl
        ld iy, screenbuffer
        ld hl, [screenoffset]

.loop:  ld a, [de]
        test a, 0
            jz .end
        lea ix, [iy+hl]
        ld [ix], a
        ld [ix+1], b
        inc de
        add hl, 2
.end:   
        cmp hl, maxScreenoffset
            jns scroll
        ret

lf_cr:  
        ;todo figure out how to calculate the begin of each line without 25 adds
        ret

scroll: push lr
        ld ix, screenbuffer + screenwidth
        ld iy, screenbuffer
        ld hl, scrollByteCount
        jl memcopy
        ld hl, [screenoffset]
        sub hl, screenwidth
        ld [screenoffset], hl
        pop lr
        ret

; copy hl count
; from ix to iy
memcopy:
.loop:  test hl, 0
            jz .end
        lea bc, [ix+hl]
        lea de, [iy+hl]
        ld a, [bc]
        ld [de], a
        dec hl
        j .loop
.end:   ret

.segment .bss 0x4000
screenbuffer:

.segment .bss 0x8000
; bytes placed here are just placeholders to be discarded
screenoffset: dw 0
charactersUntilScroll: dw 0