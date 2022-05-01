using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ForLoop : Statement
	{
		public ForLoop(VariableDeclaration initialization, Expression condition, Expression action, Statement body) : base(condition.Line)
		{
			AddChild(initialization);
			AddChild(condition);
			AddChild(action);
			AddChild(body);
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return "For Loop" + base.ToString(indent);
		}
	}
}
