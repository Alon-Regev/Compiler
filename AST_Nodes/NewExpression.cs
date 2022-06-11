using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class NewExpression : Expression
	{
		public NewExpression(ValueType type, Expression size) : base(size.Line)
		{
			Type = type;
			Type.Pointer++;
			AddChild(size);
		}

		public Expression Size()
		{
			return GetChild(0) as Expression;
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			return "New" + base.ToString(indent);
		}
	}
}
