using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class Expression : AST_Node
	{
		public ValueType Type { get; set; }

		// constructor
		// line: Line number of the node
		protected Expression(int line) : base(line)
		{
			Type = ValueType.Unknown(line);
		}

		// ToString override specifies this is an expression
		public override string ToString(int indent)
		{
			string res = " (expr";
			if (Type.TypeCode != TypeCode.UNKNOWN)
				res += ", " + Type;
			res += ") ";
			return res + base.ToString(indent);
		}
	}
}
