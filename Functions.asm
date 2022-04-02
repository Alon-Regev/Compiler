; calculates factorial of an input
; input: n (int at eax)
; output: n! (int at eax)
factorial:
	cmp eax, 2
	jge calculate_factorial
	; default return 1
	mov eax, 1
	ret

calculate_factorial:
	mov ecx, eax
	dec ecx
	; for(ecx = n; ecx > 0; ecx--)
factorial_loop:
	mul ecx
	loop factorial_loop

	ret