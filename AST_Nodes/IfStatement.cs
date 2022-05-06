using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class IfStatement : Statement
	{
		public IfStatement(Expression condition, Statement trueExec, Statement? falseExec = null) : base(condition.Line)
		{
			AddChild(condition);
			AddChild(trueExec);
			if (falseExec != null)
				AddChild(falseExec);
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
		public Statement GetFalseBlock()
		{
			if (HasElse())
				return (Statement)GetChild(2);
			else 
				return null;
		}

		// Method checks whether or not this if statement includes an else
		// input: none
		// return: true if there's an else
		public bool HasElse()
		{
			return Children.Count == 3;
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return "If Statement" + base.ToString(indent);
		}
	}
}
