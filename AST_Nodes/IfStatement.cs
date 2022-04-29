using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class IfStatement : Statement
	{
		public IfStatement(Expression condition, Statement trueExec) : base(condition.Line)
		{
			AddChild(condition);
			AddChild(trueExec);
		}

		// Getter for children
		public Expression GetCondition()
		{
			return (Expression)GetChild(0);
		}
		public Statement GetTrueBlock()
		{
			return (Statement)GetChild(1);
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return "If Statement" + base.ToString(indent);
		}
	}
}
