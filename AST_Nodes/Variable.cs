using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Variable : Expression
	{
		public string Identifier { get; private set; }

		// constructors

		// line: line number of the node
		// value: value to assign to the primitive
		public Variable(Token identifier) : base(identifier.Line)
		{
			Identifier = identifier.Value;
		}

		// ToString override prints primitive value
		public override string ToString(int indent)
		{
			return "Variable " + Identifier + base.ToString(indent);
		}
	}
}
