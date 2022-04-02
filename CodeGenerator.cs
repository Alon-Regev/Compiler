using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class CodeGenerator
	{
		AST_Node _tree;

		List<DataSectionVar> _varsToDeclare = new List<DataSectionVar>
		{
			new DataSectionVar("__temp", DataSize.DWORD, "0")
		};

		// Constructor
		// program: program's source code as string
		public CodeGenerator(AST_Node tree)
		{
			_tree = tree;
			// set result format
			switch((_tree as Expression).Type)
			{
				case TypeCode.INT:
					_varsToDeclare.Add(DataSectionVar.StringConstant("format", "Result: %d"));
					break;
				case TypeCode.FLOAT:
					_varsToDeclare.Add(DataSectionVar.StringConstant("format", "Result: %f"));
					break;
				default:
					break;
			}
		}

		// Method generates assembly program from input program
		// input: none
		// return: none
		public string GenerateAssembly()
		{
			string programAssembly = ToAssembly(_tree);
			string data = DataSectionAssembly();
			return
				"global _main\n" +
				"extern _printf\n" +
				"\n" +

				"section .data\n" +
				Indent(data) +
				"\n" +

				"section .text\n" +
				"_main:\n" +
				Indent(
					programAssembly +
					"\n" +
					AssemblyPrintResult((_tree as Expression).Type) + 
					"\n" +
					"mov eax, 0\n" +
					"ret"
				);
		}

		// Methods generate assembly code from subtrees
		// tree: subtree to turn to assembly code
		// return: assembly as string
		private string ToAssembly(AST_Node tree)
		{
			switch(tree)
			{
				case BinaryOperator op:
					return ToAssembly(op);
				case UnaryOperator op:
					return ToAssembly(op);
				case IPrimitive p:
					return ToAssembly(p);
				case Cast c:
					return ToAssembly(c);
				default:
					return "";
			}
		}

		// binary operator assembly rules:
		// result	ax
		// operands ax, bx
		public static string DEFAULT_OPERATOR_BINARY = "Invalid binary operation passed semantic analysis";
		public static string DEFAULT_TYPE_BINARY = "Binary operator has no type (or invalid type) after semantic analysis";
		private string ToAssembly(BinaryOperator op)
		{
			string operandsASM = "";
			// get operand2 on stack
			operandsASM += ToAssembly(op.Operand(1));
			operandsASM += "push eax\n";
			// get operand1 in eax
			operandsASM += ToAssembly(op.Operand(0));
			// pop operand2 to ebx
			operandsASM += "pop ebx\n";

			// calculate based on type
			switch (op.Type)
			{
				case TypeCode.INT:
					return operandsASM +
						op.Operator switch
						{
							// --- Arithmetic
							TokenCode.ADD_OP => "add eax, ebx\n",
							TokenCode.SUB_OP => "sub eax, ebx\n",
							TokenCode.MUL_OP => "mul ebx\n",
							TokenCode.DIV_OP => "div ebx\n",
							TokenCode.MOD_OP => "div ebx\n" +
												"mov eax, edx\n",
							// --- Bitwise
							TokenCode.BIT_OR_OP =>  "or eax, ebx\n",
							TokenCode.BIT_XOR_OP => "xor eax, ebx\n",
							TokenCode.BIT_AND_OP => "and eax, ebx\n",
							TokenCode.LEFT_SHIFT => "mov cl, bl\n" +
													"shl eax, cl\n",
							TokenCode.RIGHT_SHIFT =>"mov cl, bl\n" +
													"shr eax, cl\n",
							_ => throw new ImplementationError(DEFAULT_OPERATOR_BINARY)
						};

				case TypeCode.FLOAT:
					return operandsASM +
						// load eax and ebx to fpu
						"mov [__temp], eax\n" +
						"fld dword [__temp]\n" +
						"mov [__temp], ebx\n" +
						"fld dword [__temp]\n" +
						// calculate operation
						op.Operator switch
						{
							TokenCode.ADD_OP => "faddp\n",
							TokenCode.SUB_OP => "fsubp\n",
							TokenCode.MUL_OP => "fmulp\n",
							TokenCode.DIV_OP => "fdivp\n",
							TokenCode.POW_OP => "fxch st1\n" +
												"fyl2x\n" + 
												"fld1\n" +	
												"fld st1\n" +
												"fprem\n" +
												"f2xm1\n" +
												"fadd\n" +
												"fscale\n" +
												"fxch st1\n" +
												"fstp st0\n",
							_ => throw new ImplementationError(DEFAULT_OPERATOR_BINARY)
						} +
						// mov result from fpu to eax
						"fstp dword [__temp]\n" +
						"mov eax, [__temp]\n";

				default:
					throw new ImplementationError(DEFAULT_TYPE_BINARY);
			}
		}

		// unary operator assembly rules:
		// operand -> result: ax -> ax
		public static string DEFAULT_OPERATOR_UNARY = "Invalid unary operator passed semantic analysis";
		public static string DEFAULT_TYPE_UNARY = "Unary operator has no type (or invalid type) after semantic analysis";
		private string ToAssembly(UnaryOperator op)
		{
			string operandASM = ToAssembly(op.Operand());
			// calculate result of op
			switch(op.Type)
			{
				case TypeCode.INT:
					return operandASM +
						(op.Operator, op.Prefix) switch
						{
							(TokenCode.BIT_NOT_OP, true) => "not eax\n",
							(TokenCode.SUB_OP, true) => "neg eax\n",	// negation
							(TokenCode.EXCLAMATION_MARK, false) => throw new ImplementationError("factorial not implemented yet"),
							_ => throw new ImplementationError(DEFAULT_OPERATOR_UNARY)
						};

				case TypeCode.FLOAT:
					return operandASM +
						// load operand to fpu
						"mov [__temp], eax\n" +
						"fld dword [__temp]\n" +
						(op.Operator, op.Prefix) switch
						{
							(TokenCode.SUB_OP, true) => "fchs\n",    // negation
							_ => throw new ImplementationError(DEFAULT_OPERATOR_UNARY)
						} +
						// move result back into eax
						"fstp dword [__temp]\n" +
						"mov eax, [__temp]\n";

				default:
					throw new ImplementationError(DEFAULT_TYPE_UNARY);
			}
		}

		// primitive
		// place at eax
		private string ToAssembly(IPrimitive primitive)
		{
			switch(primitive)
			{
				case Primitive<int> p:
					return "mov eax, " + p.Value + "\n";
				case Primitive<float> p:
					DataSectionVar floatConst = DataSectionVar.FloatConstant(p.Value);
					_varsToDeclare.Add(floatConst);
					return "mov eax, [" + floatConst.Name + "]\n";
				default:
					return "";
			}
		}

		// generate assembly for casting
		// operand at eax
		// result at eax
		private string ToAssembly(Cast cast)
		{
			switch(cast.FromType, cast.Type)
			{
				case (TypeCode.INT, TypeCode.FLOAT):
					return	// eax -> __temp -(cast)> fpu -> __temp -> eax
						ToAssembly(cast.Child()) + 
						"mov [__temp], eax\n" +
						"fild dword [__temp]\n" +
						"fstp dword [__temp]\n" +
						"mov eax, [__temp]\n";
				case (TypeCode.FLOAT, TypeCode.INT):
					return  // eax -> __temp -> fpu -(cast)> __temp -> eax
						ToAssembly(cast.Child()) +
						"mov [__temp], eax\n" +
						"fld dword [__temp]\n" +
						"fistp dword [__temp]\n" +
						"mov eax, [__temp]\n";
				default:
					return "";
			}
		}

		// Method adds necessary global variables after turning program to assembly
		// input: none
		// return: assembly code for data section
		private string DataSectionAssembly()
		{
			string result = "";
			foreach(DataSectionVar v in _varsToDeclare)
			{
				result += v.ToAssembly();
			}
			return result;
		}

		// Method adds indent before every line
		// input: string to indent
		// return: indented string
		private string Indent(string str)
		{
			return "\t" + str.Replace("\n", "\n\t");
		}

		// Method returns assembly code for printing final result
		// input: none
		// return: assembly code
		private string AssemblyPrintResult(TypeCode type)
		{
			switch (type)
			{
				case TypeCode.INT:
					return
						"push eax\n" +
						"push format\n" +
						"call _printf\n" +
						"add esp, 8\n";
				case TypeCode.FLOAT:
					return
						"mov [__temp], eax\n" +
						"fld dword [__temp]\n" +
						"sub esp, 8\n" +
						"fst qword [esp]\n" +
						"push format\n" +
						"call _printf\n" + 
						"add esp, 12\n";
				default:
					return "";
			}
		}
	}
}
