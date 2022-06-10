;;
;; Simple monitor program.
;;
;; Accept strings from STDIN, and execute them.
;;
;; Built-in commands
;;
;;   [c]all xxxx  -> Call the routine at XXXX
;;
;;   [d]ump       -> Dump 16 bytes of RAM at a given address.
;;
;;   [i]nput      -> Enter bytes
;;
;; Other input will generate an "ERR" message, and be ignored.
;;

.macro outChar() { ld [0xFFF0], a }
.macro inChar() { ld a, [0xFFF1] }

.segment text 0

    ;;
    ;; Ensure we have a stack-pointer setup, with some room.
    ;;
    ld sp, stack_start

;;
;; Entry-point to the monitor.
;;
;; Read text into into `input_buffer`, appending to the buffer until a newline
;; is seen, then invoke `process` to handle the input.
;;
monitor:
        ;; show the prompt.
        ld a, '>'
        outChar()


        ld hl, input_buffer
monitor_input_loop:
        ;; overwrite an previous input
        ld [hl], '\n'

        ;; read and store the new character
        inChar()
        ld [hl],a

        ;; was it a newline?  If so process.
        test a, '\n'
        jz process_input_line

        ;; Otherwise loop round for more input.
        inc hl
        j monitor_input_loop



;;
;; process_input_line is called when the monitor has received a complete
;; newline-terminated line of text.
;;
;; We process the contents by looking for commands we understand, if we see
;; input we don't recognize we show a message and return, otherwise we invoke
;; the appropriate handler.
;;
;;  C => CALL
;;  D => DUMP
;;  I => INPUT
;;
process_input_line:

        ld hl, input_buffer
        ld a, [hl]

        ;; C == CALL
        test a, 'c'
        jz call_handler
        test a, 'C'
        jz call_handler

        ;; D == DUMP
        test a, 'd'
        jz dump_handler
        test a, 'D'
        jz dump_handler

        ;; I == INPUT
        test a, 'i'
        jz input_handler
        test a, 'I'
        jz input_handler

        ;;
        ;; Unknown command: show a message and restart our monitor
        ;;
        ;; We just show "ERR" which is simple, and saves bytes compared to
        ;; outputting a longer message and using a print-string routine.
        ;;
        ld a, 'E'
        outChar()
        ld a, 'R'
        outChar()
        outChar()
        ld a, '\n'
        outChar()
        j monitor




;;
;; Call is invoked with the address to call
;;
;; For example "C0003" will call the routine at 0x0003
;;
call_handler:

        ;; Our input-buffer will start with [cC], so we start looking at the
        ;; next character.
        ld hl, input_buffer+1

        ;; Read the address to call into BC
        jl read_16_bit_ascii_number

        ;; We'll be making a call, so we need to have the return
        ;; address on the stack so that when the call'd routine ends
        ;; execution goes somewhere sane.
        ;;
        ;; We'll want to re-load the monitor, so we'll store the
        ;; entry point on the stack
        ;;
        ld hl,monitor
        push hl

        ;; Now we jump, indirectly, to the address in the BC register.
        mov hl, bc
        j hl



;;
;; Dump 16 bytes from the current dump_address
;;
;; We're called with either "D" to keep going where we left off or
;; "D1234" if we should start at the given offset.
;;
dump_handler:

        ;; Our input-buffer will start with [dD], so we start looking at the
        ;; next character.
        ld hl, input_buffer+1

        ;; Look at the next input-byte.  If empty then no address.
        ld a, [hl]
        test '\n'
        jz dump_handler_no_number

        ;; OK we expect an (ASCII) address following HL - read it into BC.
        jl read_16_bit_ascii_number
        ld [dump_address], bc

dump_handler_no_number:
        ;; The address we start from
        ld hl, [dump_address]
        ;; show the address
        jl output_16_bit_number

        ;; Loop to print the next 16 bytes at that address.
        ld b, 16
dump_byte:
        ;; show a space
        ld a, ' '
        outChar()

        ;; show the memory-contents.
        ld c, [hl]
        jl output_8_bit_number
        inc hl
        dec b
        jnz dump_byte

        ;; all done
        ld a, '\n'
        outChar()

        ;; store our updated/final address.
        ld [dump_address], hl
jmp_monitor:
        j monitor



;;
;; Input handler allows code to be assembled at a given address
;;
;; Usage is:
;;
;;  I01234 01 02 03 04 0f
;;
;; i.e. "I<address> byte1 byte2 .. byteN"
;;
;; If there is no address keep going from the last time, which means this
;; works as you expect:
;;
;;   I1000 01 03
;;   I 03 04 0F
;;
input_handler:
        ;; Our input-buffer will start with [iI], so we start looking at the
        ;; next character.
        ld hl, input_buffer+1

        ;; Look at the next input-byte.  If it is a space then no address was
        ;; given, so we keep appending bytes to the address set previously.
        ld a, [hl]
        test a, ' '
        jz input_handler_no_address

        ;; OK we expect an (ASCII) address following HL - Read it into BC.
        jl read_16_bit_ascii_number
        ld [input_address], bc

input_handler_no_address:

        ;; HL contains the a string.  Get the next byte
        ld a,[hl]
        inc hl

        ;; space? skip
        test a, ' '
        jz input_handler_no_address

        ;; newline? If so we're done
        test a, '\n'
        jz jmp_monitor

        ;; OK then we have a two-digit number
        dec hl
        jl read_8_bit_ascii_number

        ;; store the byte in RAM
        ld bc, [input_address]
        ld [bc], a

        ;; bump to the next address
        inc bc
        ld [input_address], bc

        ;; continue
        j input_handler_no_address




;;
;; Convert a 4-digit ASCII number, pointed to by HL to a number.
;; Return that number in BC.
;;
read_16_bit_ascii_number:
    ;; HL is a pointer to a four-char string
    ;; This is read as a 16 bit hex number
    ;; The number is stored in BC
    push lr
    jl read_8_bit_ascii_number
    ld b, a
    jl read_8_bit_ascii_number
    ld c, a
    pop lr
    ret





;;
;; Read the two-digit HEX number from HL, and convert to an integer
;; stored in the A-register.
;;
;; HL will be incremented twice.
;;
read_8_bit_ascii_number:
        ld a, [hl]
        ;; is it lower-case?  If so upper-case it.
        cmp a, 'a'
        jc read_8_bit_ascii_number_uc
        cmp a, 'z'
        jnc read_8_bit_ascii_number_uc
        sub a, 32
read_8_bit_ascii_number_uc:
        push lr

        jl read_8_bit_ascii_number_hex
        add a, a
        add a, a
        add a, a
        add a, a
        ld d, a
        inc hl
        ld a, [hl]
        jl read_8_bit_ascii_number_hex
        or d
        inc hl

        pop lr
        ret
read_8_bit_ascii_number_hex:
        sub a, '0'
        test a, 10
        jc read_8_bit_ascii_number_hex_return
        sub a,'A'-'0'-10
read_8_bit_ascii_number_hex_return:
        ret


;;
;; Display the 16-bit number stored in HL in hex.
;;
output_16_bit_number:
        push lr
        ld  c,h
        jl  output_8_bit_number
        ld  c,l
        jl output_8_bit_number
        pop lr
        ret

;;
;; Display the 8-bit number stored in C in hex.
;;
output_8_bit_number:
        push lr
        ld a, c
        scr
        scr
        scr
        scr
        jl Conv
        ld a, c
        pop lr
Conv:
        and a, 0x0F
        cmp a, 9
        jz Conv09
        jc Conv09
        add a, 'A' - 9
        ret
Conv09:
        add a, '0'
        ret

.segment data 0x4000
;;;;;;;;
;;;;;;;; RAM stuff
;;;;;;;;

;;
;; Here we store some values.
;;

;; DUMP: We track of the address from which we're dumping.
dump_address:
        dw 0
;; INPUT: Keep track of the address to which we next write.
input_address:
        dw 0

;; We don't nest calls too deeply ..
stack_end:
        dw 0
        dw 0
        dw 0
        dw 0
        dw 0
        dw 0
        dw 0
        dw 0
        dw 0
        dw 0
stack_start:

;; Command-line input buffer.
input_buffer: