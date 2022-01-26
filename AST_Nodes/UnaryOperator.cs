using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class UnaryOperator : Expression
	{
		public TokenCode Operator { get; private set; }

		// constructor
		// op: operator code which this node presents.
		// operand: Operand to apply the operator on.
		public UnaryOperator(TokenCode op, AST_Node operand)
		{
			Operator = op;
			// set children
			AddChild(operand);
		}
	}
}
