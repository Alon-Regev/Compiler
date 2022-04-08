using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler
{
	class CodeGenerator
	{
		AST_Node _tree;

		private int _labelCounter = 0;

		List<DataSectionVar> _varsToDeclare = new List<DataSectionVar>
		{
			new DataSectionVar("__temp", DataSize.DWORD, "0")
		};

		private HashSet<string> _helperFunctionsUsed = new HashSet<string>();
		private HashSet<string> _macrosUsed = new HashSet<string>();

		// Constructor
		// program: program's source code as string
		public CodeGenerator(AST_Node tree)
		{
			_tree = tree;
		}

		// Method generates assembly program from input program
		// input: none
		// return: none
		public string GenerateAssembly()
		{
			string programAssembly = ToAssembly(_tree);
			string data = DataSectionAssembly();
			string result =
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
					"mov eax, 0\n" +
					"ret"
				) + "\n\n" +
				HelperFunctionsAssembly();

			return MacrosAssembly() + result;
		}

		// Methods generate assembly code from subtrees
		// tree: subtree to turn to assembly code
		// return: assembly as string
		private string ToAssembly(AST_Node tree)
		{
			switch(tree)
			{
				// --- Expressions
				case BinaryOperator op:
					return ToAssembly(op);
				case UnaryOperator op:
					return ToAssembly(op);
				case TernaryOperator op:
					return ToAssembly(op);
				case IPrimitive p:
					return ToAssembly(p);
				case Cast c:
					return ToAssembly(c);
				// --- Statements
				case ExpressionStatement stmt:
					return ToAssembly(stmt.GetExpression());
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
			if (op.Operand(0).Type != TypeCode.BOOL)	// bool logical operators use operands differently (short-circuit)
			{
				// get operand2 on stack
				operandsASM += ToAssembly(op.Operand(1));
				operandsASM += "push eax\n";
				// get operand1 in eax
				operandsASM += ToAssembly(op.Operand(0));
				// pop operand2 to ebx
				operandsASM += "pop ebx\n";
			}

			// calculate based on input type
			switch (op.Operand(0).Type)
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
							// --- Relational
							TokenCode.LESS_OP => "cmp eax, ebx\nmov eax, 0\nsetl al\n",
							TokenCode.LESS_EQUAL_OP => "cmp eax, ebx\nmov eax, 0\nsetle al\n",
							TokenCode.GREATER_OP => "cmp eax, ebx\nmov eax, 0\nsetg al\n",
							TokenCode.GREATER_EQUAL_OP => "cmp eax, ebx\nmov eax, 0\nsetge al\n",
							TokenCode.EQUAL_OP => "cmp eax, ebx\nmov eax, 0\nsete al\n",
							TokenCode.NOT_EQUAL_OP => "cmp eax, ebx\nmov eax, 0\nsetne al\n",
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
							// --- Arithmetic
							TokenCode.ADD_OP => "faddp\n",
							TokenCode.SUB_OP => "fsubp\n",
							TokenCode.MUL_OP => "fmulp\n",
							TokenCode.DIV_OP => "fdivp\n",
							TokenCode.POW_OP => HelperCall("pow"),
							// --- Relational
							TokenCode.LESS_OP => Macro("float_comparison", "0000000000000000b"),	// not condition flags
							TokenCode.LESS_EQUAL_OP => Macro("float_comparison_inverse", "0000000100000000b"),  // not greater,
							TokenCode.GREATER_OP => Macro("float_comparison", "0000000100000000b"),	// carry flag
							TokenCode.GREATER_EQUAL_OP => Macro("float_comparison_inverse", "0000000000000000b"),	// not less
							TokenCode.EQUAL_OP => Macro("float_comparison", "0100000000000000b"),	// zero flag
							TokenCode.NOT_EQUAL_OP => Macro("float_comparison_inverse", "0100000000000000b"),	// not equal
							_ => throw new ImplementationError(DEFAULT_OPERATOR_BINARY)
						} +
						// mov result from fpu to eax if type is float
						(op.Type == TypeCode.FLOAT ?
						"fstp dword [__temp]\n" +
						"mov eax, [__temp]\n" : "");

				case TypeCode.BOOL:
					string label = GetLabel();
					return operandsASM +
						op.Operator switch
						{
							TokenCode.LOGIC_AND_OP =>	ToAssembly(op.Operand(0)) + 
														"cmp eax, 0\n" +
														"je " + label + "\n" +
														ToAssembly(op.Operand(1)) +
														label + ":\n",

							TokenCode.LOGIC_OR_OP =>	ToAssembly(op.Operand(0)) +
														"cmp eax, 1\n" +
														"je " + label + "\n" +
														ToAssembly(op.Operand(1)) +
														label + ":\n",

							_ => throw new ImplementationError(DEFAULT_OPERATOR_BINARY)
						};

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
							(TokenCode.EXCLAMATION_MARK, false) => HelperCall("factorial"),
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

				case TypeCode.BOOL:
					return operandASM +
						(op.Operator, op.Prefix) switch
						{
							(TokenCode.EXCLAMATION_MARK, true) =>	"cmp eax, 0\n" +
																	"mov eax, 0\n" +
																	"sete al\n",    // logical not
							_ => throw new ImplementationError(DEFAULT_OPERATOR_UNARY)
						};

				default:
					throw new ImplementationError(DEFAULT_TYPE_UNARY);
			}
		}

		// ternary operator assembly:
		// condition operand: eax
		// operands: ebx, ecx
		// result: eax
		private string ToAssembly(TernaryOperator op)
		{
			string operandsASM = "";
			// get operand3 on stack
			operandsASM += ToAssembly(op.Operand(2));
			operandsASM += "push eax\n";
			// get operand2 on the stack
			operandsASM += ToAssembly(op.Operand(1));
			operandsASM += "push eax\n";
			// put all operands in place
			operandsASM += ToAssembly(op.Operand(0));
			operandsASM += "pop ebx\npop ecx\n";

			// add code for ternary op
			return operandsASM +
				"cmp eax, 0\n" +
				"cmove eax, ecx\n" +
				"cmovne eax, ebx\n";
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
				case Primitive<bool> p:
					return "mov eax, " + (p.Value ? 1 : 0) + "\n";
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
				case (TypeCode.INT, TypeCode.BOOL):
				case (TypeCode.BOOL, TypeCode.INT):
					return ToAssembly(cast.Child());	// no need to change data
				default:
					throw new TypeError(cast);
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

		// Method returns an assembly call to a helper function and makes sure it's added to the final assembly.
		// input: name of helper function
		// return: function call assembly as string
		private string HelperCall(string functionName)
		{
			_helperFunctionsUsed.Add(functionName);
			return "call " + functionName + "\n";
		}

		// Method returns an assembly macro use and makes sure it's added to the final assembly
		// input: name of macro, parameters
		// return: macro call
		private string Macro(string macroName, string parameters)
		{
			_macrosUsed.Add(macroName);
			return macroName + " " + parameters + "\n";
		}

		// Method returns assembly code for used helper functions and macros
		// input: none
		// return: assembly code for definitions of used functions
		private string HelperFunctionsAssembly()
		{
			string[] functions = Properties.Resources.Functions.Split(";FUNCTION;\r\n", StringSplitOptions.RemoveEmptyEntries);
			string result = "";
			// add used functions to result
			foreach(string function in functions)
			{
				string name = function.Split(":")[0];
				// if this function was used, add its definition
				if (_helperFunctionsUsed.Contains(name))
					result += function;
			}
			return result;
		}
		private string MacrosAssembly()
		{
			string result = "";
			string[] macros = Properties.Resources.Macros.Split("%macro ", StringSplitOptions.RemoveEmptyEntries);
			// add used macros to result
			foreach (string macro in macros)
			{
				string name = macro.Split()[0];
				// if this function was used, add its definition
				if (_macrosUsed.Contains(name))
					result += "%macro " + macro + "\n";
			}
			return result;
		}

		// Method returns a unique label name every call
		// input: none
		// return: unique label name as string
		private string GetLabel()
		{
			return "__" + _labelCounter++;
		}
	}
}
