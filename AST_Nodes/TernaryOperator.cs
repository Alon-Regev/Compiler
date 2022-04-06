using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class TernaryOperator : Expression
	{
		// constructor
		// line: line number of the node
		// operand 1,2,3: Operands to apply the operator on.
		public TernaryOperator(Expression operand1, Expression operand2, Expression operand3) : base(operand1.Line)
		{
			// set children
			AddChild(operand1);
			AddChild(operand2);
			AddChild(operand3);
		}

		// ToString prints operator and children
		public override string ToString(int indent)
		{
			return "Ternary Op" + base.ToString(indent);
		}

		// Getter for operand
		public Expression Operand(int i)
		{
			if (i >= 3 || i < 0)
				throw new ImplementationError("Invalid child index of binary operator");
			return (Expression)GetChild(i);
		}
	}
}
