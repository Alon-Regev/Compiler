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
	}
}
