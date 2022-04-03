%macro float_comparison 1
; compares two floats from the top of the fpu stack
; input: the status Word Register condition bits required state
; important bits: 8-CF, 14-ZF...
	xor eax, eax
	; set status register at ax
	fcom
    fstsw ax	; store status register
	
	; check bits
	and ax, 0100011100000000b
	cmp ax, %1
	sete al	; res result
	mov ah, 0
%endmacro

%macro float_comparison_inverse 1
; compares two floats from the top of the fpu stack
; input: status word state which isn't allowed (inverse condition)
	xor eax, eax
	; set status register at ax
	fcom
    fstsw ax	; store status register
	
	; check bits
	and ax, 0100011100000000b
	cmp ax, %1
	setne al	; res result
	mov ah, 0
%endmacro
