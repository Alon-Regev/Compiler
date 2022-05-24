using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
	class FunctionCall : Expression
	{
		private Variable _function;

		// constructor
		// variable: function variable
		public FunctionCall(Variable function) : base(function.Line)
		{
			_function = function;
		}

		// ToString prints operator and children
		public override string ToString(int indent)
		{
			string baseline = "Function call (" + _function.Identifier + ")";
			return baseline + base.ToString(indent);
		}
	}
}
