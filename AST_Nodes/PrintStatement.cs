using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class PrintStatement : Statement
	{
		public PrintStatement(Expression expr) : base(expr.Line)
		{
			AddChild(expr);
		}

		// Getter for expression
		public Expression GetExpression()
		{
			return (Expression)GetChild(0);
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return "Print Statement" + base.ToString(indent);
		}
	}
}
