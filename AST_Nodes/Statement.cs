using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Statement : AST_Node
	{
		// Constructor
		// line: line number of the statement
		public Statement(int line) : base(line)
		{
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return " (stmt)" + base.ToString(indent);
		}
	}
}
