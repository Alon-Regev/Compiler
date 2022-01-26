using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class BinaryOperator : Expression
	{
		public TokenCode Operator { get; private set; }

		// constructor
		// line: line number of the node
		// op: operator code which this node presents.
		// operand 1,2: Operands to apply the operator on.
		public BinaryOperator(int line, TokenCode op, AST_Node operand1, AST_Node operand2) : base(line)
		{
			Operator = op;
			// set children
			AddChild(operand1);
			AddChild(operand2);
		}
	}
}
