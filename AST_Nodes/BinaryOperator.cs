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
		public BinaryOperator(TokenCode op, Expression operand1, Expression operand2) : base(operand1.Line)
		{
			Operator = op;
			// set children
			AddChild(operand1);
			AddChild(operand2);
		}

		// ToString prints operator and children
		public override string ToString(int indent)
		{
			string baseline = "Bin Op " + Operator;
			return baseline + base.ToString(indent);
		}

		// Getter for operand
		public Expression Operand(int i)
		{
			if (i >= 2 || i < 0)
				throw new ImplementationError("Invalid child index of binary operator");
			return (Expression)GetChild(i);
		}
	}
}
