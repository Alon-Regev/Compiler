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
				case IPrimitive p:
					return ToAssembly(p);
				case Cast c:
					return ToAssembly(c);
				default:
					return "";
			}
		}
		// operator assembly rules:
		// result	ax
		// args		ax, bx
		// temps	cx, dx, di, si, [__temp]
		private string ToAssembly(BinaryOperator op)
		{
			string result = "";
			// get arg2 on stack
			result += ToAssembly(op.Operand(1));
			result += "push eax\n";
			// get arg1 in eax
			result += ToAssembly(op.Operand(0));
			// pop arg2 to ebx
			result += "pop ebx\n";
			// calculate based on operator
			switch (op.Operator)
			{
				case TokenCode.ADD_OP:
					return result + AddOpASM(op.Type);

				case TokenCode.SUB_OP:
					result += "sub eax, ebx\n";
					break;

				case TokenCode.MUL_OP:
					result += "mul ebx\n";
					break;

				case TokenCode.DIV_OP:
					result += "div ebx\n";
					break;

				default:
					break;
			}

			return result;
		}
		private string AddOpASM(TypeCode type)
		{
			switch (type)
			{
				case TypeCode.INT:
					return "add eax, ebx\n";
				case TypeCode.FLOAT:
					return
						"mov [__temp], eax\n" +
						"fld dword [__temp]\n" +
						"mov [__temp], ebx\n" +
						"fld dword [__temp]\n" +
						"faddp\n" +
						"fstp dword [__temp]\n" +
						"mov eax, [__temp]\n";
				default:
					return "";
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
