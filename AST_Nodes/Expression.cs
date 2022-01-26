using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Expression : AST_Node
	{
		// constructor
		// line: Line number of the node
		protected Expression(int line) : base(line)
		{
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return " (expr) " + base.ToString(indent);
		}
	}
}
