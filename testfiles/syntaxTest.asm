.segment abs
.org 100
    .include "test"
label: .times 100 db 0
.warning "test ", 100 + 3
.const constant (abs * 2)
db `escape string \n\0`, 'a', "blub"

ld a, 0xf0
add hl, bc
lea bc, [hl+constant]
ld iy, 0b1001_0101_0000_1101

ld a, $
ld b, -2
ld c, 'a'
ld d, '\\'

.const.segment.1:

;comment 
ld [1], 3
LD a , b