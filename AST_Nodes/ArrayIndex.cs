using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class ArrayIndex : Expression
	{
		// constructor
		// variable: function variable
		public ArrayIndex(Expression array, Expression index) : base(array.Line)
		{
			AddChild(array);
			AddChild(index);
		}

		// returns array node
		public Expression Array()
		{
			return GetChild(0) as Expression;
		}

		// returns index node
		public Expression Index()
		{
			return GetChild(1) as Expression;
		}

		// ToString prints operator and children
		public override string ToString(int indent)
		{
			string baseline = "Array Index";
			return baseline + base.ToString(indent);
		}
	}
}
