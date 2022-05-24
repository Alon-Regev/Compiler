using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class FunctionCall : Expression
	{
		public Variable Function { private set; get; }

		// constructor
		// variable: function variable
		public FunctionCall(Variable function) : base(function.Line)
		{
			Function = function;
		}

		// ToString prints operator and children
		public override string ToString(int indent)
		{
			string baseline = "Function call (" + Function.Identifier + ")";
			return baseline + base.ToString(indent);
		}
	}
}
