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
				"\n" +

				"section .data\n" +
				"\n" +

				"section .text\n" +
				"_main:\n" +
				Indent(
					ToAssembly(_tree) +
					"mov eax, 0\n" +
					"ret"
				);
		}

		// Methods generate assembly code from subtrees
		// tree: subtree to turn to assembly code
		// return: assembly as string
		private string ToAssembly(AST_Node tree)
		{
			return "";
		}

		// Method adds indent before every line
		// input: string to indent
		// return: indented string
		private string Indent(string str)
		{
			return "\t" + str.Replace("\n", "\n\t");
		}
	}
}
