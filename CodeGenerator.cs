using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class CodeGenerator
	{
		private AST_Node _tree = null;

		// Constructor
		// program: program's code to compile as string
		public CodeGenerator(string program)
		{
			_tree = new Parser(program).Parse();
		}

		// function turns tree to string of assembly code
		// input: tree to turn to ASM (default entire program)
		// return: assembly code as string
		public string GenerateAssembly()
		{
			return GenerateAssembly(_tree);
		}
		public string GenerateAssembly(AST_Node tree)
		{
			if(tree is Primitive<int>)
			{
				return "mov ax, " + ((Primitive<int>)tree).Value + "\n";
			}
			else if(tree is BinaryOperator)
			{
				BinaryOperator op = (BinaryOperator)tree;
				return 
					GenerateAssembly(op.GetChild(0)) + 
					"mov ax, bx\n" +
					GenerateAssembly(op.GetChild(1)) + 
					"add ax, bx\n";
			}	
			else
			{
				return "; I don't know what this is...";
			}
		}
	}
}
