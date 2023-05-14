main:
    jsr test
    jmp done
    lda #$69
    lda #$69
    lda #$69
    lda #$69
    lda #$69
    lda #$69
    lda #$69
    lda #$69
    lda #$69
    lda #$69

test:
    rts

done:
    ldx #$40