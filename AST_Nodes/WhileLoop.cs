using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class WhileLoop : Statement
	{
		public WhileLoop(Expression condition, Statement block) : base(condition.Line)
		{
			AddChild(condition);
			AddChild(block);
		}

		// Getters for children
		public Expression GetCondition()
		{
			return (Expression)GetChild(0);
		}
		public Statement GetBlock()
		{
			return (Statement)GetChild(1);
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return "While Loop" + base.ToString(indent);
		}
	}
}
