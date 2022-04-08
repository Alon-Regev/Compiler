using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class UnaryOperator : Expression
	{
		public TokenCode Operator { get; private set; }
		public bool Prefix { get; private set; }

		// constructor
		// line: line number of the node
		// op: operator code which this node presents.
		// operand: Operand to apply the operator on.
		public UnaryOperator(TokenCode op, Expression operand, bool prefix) : base(operand.Line)
		{
			Operator = op;
			// set children
			AddChild(operand);

			Prefix = prefix;
		}

		// Getter for single operand
		public Expression Operand()
		{
			return (Expression)GetChild(0);
		}

		// ToString prints operator and children
		public override string ToString(int indent)
		{
			string baseline = "Unary Op " + Operator;
			return baseline + base.ToString(indent);
		}
	}
}
