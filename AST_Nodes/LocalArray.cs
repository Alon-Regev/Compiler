using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class LocalArray : Expression
	{
		public LocalArray(List<Expression> elements, int line) : base(line)
		{
			foreach(Expression element in elements)
			{
				AddChild(element);
			}
		}

		// ToString override prints list of elements
		public override string ToString(int indent)
		{
			return "Local Array" + base.ToString(indent);
		}
	}
}
