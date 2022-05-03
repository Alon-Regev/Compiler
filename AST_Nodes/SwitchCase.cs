using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class SwitchCase : Statement
	{
		public bool HasDefault { private set; get; } = false;

		public SwitchCase(Expression switchValue) : base(switchValue.Line)
		{
			AddChild(switchValue);
		}

		// methods to add children
		public void AddCase(Expression expression, Block block)
		{
			Children.Add(expression);
			Children.Add(block);
		}
		public void AddDefault(Block block)
		{
			HasDefault = true;
			Children.Insert(1, block);
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return "Switch Case" + base.ToString(indent);
		}
	}
}
