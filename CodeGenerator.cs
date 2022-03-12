using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class CodeGenerator
	{
		AST_Node _tree;

		// Constructor
		// program: program's source code as string
		public CodeGenerator(string program)
		{
			_tree = new Parser(program).Parse();
		}

		// Method generates assembly program from input program
		// input: none
		// return: none
		public string GenerateAssembly()
		{
			return
				"global _main\n" +
				"extern _printf\n" +
				"\n" +

				"section .data\n" +
				"\tformat: db \"result: %d\", 0xa, 0\n" +
				"\n" +

				"section .text\n" +
				"_main:\n" +
				Indent(
					ToAssembly(_tree) +
					"\n" +
					AssemblyPrintResult() + 
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
			if (tree is BinaryOperator)
				return ToAssembly((BinaryOperator)tree);
			else if (tree is Primitive<int>)
				return ToAssembly((Primitive<int>)tree);
			else
				return "";
		}
		// operator assembly rules:
		// result	ax
		// args		ax, bx
		// temps	cx, dx, di, si
		private string ToAssembly(BinaryOperator op)
		{
			string result = "";
			// get arg2 on stack
			result += ToAssembly(op.GetChild(1));
			result += "push eax\n";
			// get arg1 in eax
			result += ToAssembly(op.GetChild(0));
			// pop arg2 to ebx
			result += "pop ebx\n";
			// calculate based on operator
			switch (op.Operator)
			{
				case TokenCode.ADD_OP:
					result += "add eax, ebx\n";
					break;

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

		// primitive int
		// place at eax
		private string ToAssembly(Primitive<int> op)
		{
			return "mov eax, " + op.Value + "\n";
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
		private string AssemblyPrintResult()
		{
			return
				"push eax\n" +
				"push format\n" +
				"call _printf\n" +
				"add esp, 8\n";
		}
	}
}
