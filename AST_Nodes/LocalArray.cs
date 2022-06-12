using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class LocalArray : Expression
	{
		static private int Count = 0;
		private int IdentifierNumber = 0;

		public LocalArray(List<Expression> elements, int line) : base(line)
		{
			foreach(Expression element in elements)
			{
				AddChild(element);
			}
			IdentifierNumber = Count++;
		}

		// Method returns symbol table identifier for local array identifier
		// input: none
		// return: string identifier
		public string GetIdentifier()
		{
			return "__local_array" + IdentifierNumber;
		}

		// ToString override prints list of elements
		public override string ToString(int indent)
		{
			return "Local Array" + base.ToString(indent);
		}
	}
}
