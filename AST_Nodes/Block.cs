using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Block : Statement
	{
		public Block(int line) : base(line)
		{
		}

		// Method adds a statement to the block
		// input: statement to add
		// return: none
		public void AddStatement(Statement statement)
		{
			AddChild(statement);
		}

		// Method gets a statement from the block based on the index
		// input: index
		// return: statement
		public Statement GetStatement(int i)
		{
			return (Statement)GetChild(i);
		}

		// ToString override shows block and it's content
		public override string ToString(int indent)
		{
			return "Block" + base.ToString(indent);
		}
	}
}
