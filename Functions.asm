;FUNCTION;
factorial:
; calculates factorial of an input, eax -> eax
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
	
;FUNCTION;
print_int:
; prints integer from eax
	push eax
	
	mov ebx, format
	mov byte [ebx + 1], 'd'

	push format
	call _printf
	add esp, 8
	ret
	
;FUNCTION;
print_float:
; prints float from eax
	mov [__temp], eax
	fld dword [__temp]
	sub esp, 8
	fstp qword [esp]
	
	mov ebx, format
	mov byte [ebx + 1], 'f'

	push format
	call _printf
	add esp, 12
	ret
	
;FUNCTION;
print_bool:
; prints bool from eax
	push true_string	; default true
	; check if false
	cmp eax, 0
	jne skip_false
	; is false
	mov [esp], dword false_string
	
skip_false:
	
	mov ebx, format
	mov byte [ebx + 1], 's'

	push format
	call _printf
	add esp, 8
	ret

;FUNCTION;
print_char:
; prints char from eax
	mov ebx, format
	mov byte [ebx + 1], 'c'

	push eax
	push format
	call _printf
	add esp, 8
	ret
	
;FUNCTION;
print_ptr:
; prints pointer from eax
	push eax

	mov ebx, format
	mov byte [ebx + 1], 'x'
	
	push hex_str
	call _printf
	add esp, 4

	push format
	call _printf
	add esp, 8
	ret
	
;FUNCTION;
print_str:
; prints string from eax
	mov ebx, format
	mov byte [ebx + 1], 's'

	push eax
	push format
	call _printf
	add esp, 8
	ret

;FUNCTION;
pow:
; calculates eax = eax ** ebx
	fxch st1
	fyl2x
	fld1
	fld st1
	fprem
	f2xm1
	fadd
	fscale
	fxch st1
	fstp st0
	ret
	
;FUNCTION;
input_int:
; int input into eax
	mov ebx, formatin
	mov byte[ebx + 1], 'd'
	sub esp, 4
	push esp
	push formatin
	call _scanf
	add esp, 8
	pop eax
	ret
extern _scanf
